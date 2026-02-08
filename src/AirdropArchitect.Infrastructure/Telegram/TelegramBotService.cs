using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Core.Models;
using Microsoft.Extensions.Logging;

namespace AirdropArchitect.Infrastructure.Telegram;

public class TelegramBotService : ITelegramBotService
{
    private readonly ITelegramBotClient _bot;
    private readonly IUserService _userService;
    private readonly IBlockchainService _blockchainService;
    private readonly IPaymentService _paymentService;
    private readonly ICryptoPaymentService? _cryptoPaymentService;
    private readonly IAirdropService? _airdropService;
    private readonly IPointsService? _pointsService;
    private readonly ILocalizationService _localizer;
    private readonly IGeoRestrictionService _geoRestriction;
    private readonly ILogger<TelegramBotService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TelegramBotService(
        ITelegramBotClient bot,
        IUserService userService,
        IBlockchainService blockchainService,
        IPaymentService paymentService,
        ILocalizationService localizer,
        IGeoRestrictionService geoRestriction,
        ILogger<TelegramBotService> logger,
        ICryptoPaymentService? cryptoPaymentService = null,
        IAirdropService? airdropService = null,
        IPointsService? pointsService = null)
    {
        _bot = bot;
        _userService = userService;
        _blockchainService = blockchainService;
        _paymentService = paymentService;
        _localizer = localizer;
        _geoRestriction = geoRestriction;
        _cryptoPaymentService = cryptoPaymentService;
        _airdropService = airdropService;
        _pointsService = pointsService;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(string updateJson, CancellationToken ct = default)
    {
        Update? update;
        try
        {
            update = JsonSerializer.Deserialize<Update>(updateJson, JsonOptions);
            if (update == null)
            {
                _logger.LogWarning("Failed to deserialize Telegram update: null result");
                return;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize Telegram update JSON");
            return;
        }

        try
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await HandleMessageAsync(update.Message, ct);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                await HandleCallbackAsync(update.CallbackQuery, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Telegram update {UpdateId}", update.Id);
        }
    }

    private async Task HandleMessageAsync(Message message, CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        var text = message.Text!;
        var telegramId = message.From?.Id ?? 0;

        if (telegramId == 0)
        {
            _logger.LogWarning("Received message without user ID from chat {ChatId}", chatId);
            return;
        }

        var user = await _userService.EnsureUserExistsAsync(telegramId, ct);

        // Capture Telegram's language code for localization
        var lang = message.From?.LanguageCode ?? user.PreferredLanguage;

        // Update stored language preference if Telegram provides a different one
        if (!string.IsNullOrEmpty(message.From?.LanguageCode)
            && user.PreferredLanguage != message.From.LanguageCode)
        {
            user.PreferredLanguage = message.From.LanguageCode;
            await _userService.UpdateUserAsync(user, ct);
        }

        // Geo-restriction check (effective when country detection is available)
        if (!string.IsNullOrEmpty(user.CountryCode) && !_geoRestriction.IsCountryAllowed(user.CountryCode))
        {
            _logger.LogWarning("Blocked user {TelegramId} from country {Country}", telegramId, user.CountryCode);
            await _bot.SendMessage(
                chatId,
                _localizer.Get("errors", "CountryRestricted", lang),
                cancellationToken: ct);
            return;
        }

        var command = text.Split(' ')[0].ToLowerInvariant();
        var args = text.Split(' ').Skip(1).ToArray();

        switch (command)
        {
            case "/start":
                await HandleStartCommandAsync(chatId, telegramId, args, lang, ct);
                break;
            case "/check":
                await HandleCheckCommandAsync(chatId, telegramId, args, lang, ct);
                break;
            case "/points":
                await HandlePointsCommandAsync(chatId, telegramId, args, lang, ct);
                break;
            case "/track":
                await HandleTrackCommandAsync(chatId, telegramId, args, lang, ct);
                break;
            case "/wallets":
                await HandleWalletsCommandAsync(chatId, telegramId, lang, ct);
                break;
            case "/status":
                await HandleStatusCommandAsync(chatId, telegramId, lang, ct);
                break;
            case "/upgrade":
                await HandleUpgradeCommandAsync(chatId, lang, ct);
                break;
            case "/help":
                await SendHelpAsync(chatId, lang, ct);
                break;
            default:
                if (IsValidWalletAddress(text))
                {
                    await HandleCheckCommandAsync(chatId, telegramId, new[] { text }, lang, ct);
                }
                break;
        }
    }

    private async Task HandleStartCommandAsync(
        long chatId,
        long telegramId,
        string[] args,
        string? lang,
        CancellationToken ct)
    {
        // Check for deep link parameters (e.g., /start payment_success)
        if (args.Length > 0)
        {
            var param = args[0].ToLowerInvariant();

            // Handle payment success callback
            if (param.StartsWith("payment_success"))
            {
                await HandlePaymentSuccessAsync(chatId, telegramId, lang, ct);
                return;
            }

            // Handle payment cancelled callback
            if (param.StartsWith("payment_cancelled"))
            {
                await _bot.SendMessage(
                    chatId,
                    _localizer.Get("PaymentCancelled", lang),
                    cancellationToken: ct);
                return;
            }

            // Handle reveal success callback
            if (param.StartsWith("reveal_success"))
            {
                await _bot.SendMessage(
                    chatId,
                    _localizer.Get("RevealSuccess", lang),
                    cancellationToken: ct);
                return;
            }

            // Handle reveal cancelled callback
            if (param.StartsWith("reveal_cancelled"))
            {
                await _bot.SendMessage(
                    chatId,
                    _localizer.Get("RevealCancelled", lang),
                    cancellationToken: ct);
                return;
            }
        }

        // Default: show welcome message
        await SendWelcomeAsync(chatId, lang, ct);
    }

    private async Task HandlePaymentSuccessAsync(
        long chatId,
        long telegramId,
        string? lang,
        CancellationToken ct)
    {
        var user = await _userService.GetUserByTelegramIdAsync(telegramId, ct);
        var tier = user.SubscriptionTier;

        // If webhook hasn't processed yet, tier might still be "free"
        // In that case, show a generic success message
        if (tier == "free")
        {
            await _bot.SendMessage(
                chatId,
                _localizer.Get("PaymentReceived", lang),
                parseMode: ParseMode.Html,
                cancellationToken: ct);
            return;
        }

        var tierName = tier.ToUpperInvariant();
        var features = tier switch
        {
            "tracker" => _localizer.Get("TrackerFeatures", lang),
            "architect" => _localizer.Get("ArchitectFeatures", lang),
            "api" => _localizer.Get("ApiFeatures", lang),
            _ => ""
        };

        var expiresInfo = user.SubscriptionExpiresAt.HasValue
            ? _localizer.GetFormatted("SubscriptionRenews", lang, user.SubscriptionExpiresAt.Value)
            : "";

        await _bot.SendMessage(
            chatId,
            _localizer.GetFormatted("SubscriptionWelcome", lang, tierName, features, expiresInfo),
            parseMode: ParseMode.Html,
            cancellationToken: ct);
    }

    private async Task SendWelcomeAsync(long chatId, string? lang, CancellationToken ct)
    {
        await _bot.SendMessage(
            chatId,
            _localizer.Get("Welcome", lang),
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task SendHelpAsync(long chatId, string? lang, CancellationToken ct)
    {
        await _bot.SendMessage(
            chatId,
            _localizer.Get("Help", lang),
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleCheckCommandAsync(
        long chatId,
        long telegramId,
        string[] args,
        string? lang,
        CancellationToken ct)
    {
        if (args.Length == 0)
        {
            await _bot.SendMessage(
                chatId,
                _localizer.Get("ProvideWalletCheck", lang),
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var address = args[0];

        if (!IsValidWalletAddress(address))
        {
            await _bot.SendMessage(
                chatId,
                _localizer.Get("InvalidWalletAddress", lang),
                cancellationToken: ct);
            return;
        }

        // Detect chain and check if EVM
        var chain = DetectChain(address);
        if (chain == "solana")
        {
            await _bot.SendMessage(
                chatId,
                _localizer.GetFormatted("SolanaComingSoon", lang, ShortenAddress(address)),
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var statusMsg = await _bot.SendMessage(
            chatId,
            _localizer.Get("ScanningWallet", lang),
            cancellationToken: ct);

        try
        {
            // Fetch data from multiple chains in parallel
            var chains = new[] { "ethereum", "arbitrum", "optimism", "base", "polygon" };
            var balanceTasks = chains.Select(c => SafeGetBalanceAsync(address, c, ct)).ToArray();
            var activityTasks = chains.Select(c => SafeGetActivityAsync(address, c, ct)).ToArray();

            await Task.WhenAll(balanceTasks);
            await Task.WhenAll(activityTasks);

            var balances = balanceTasks.Select(t => t.Result).Where(b => b != null).ToList();
            var activities = activityTasks.Select(t => t.Result).Where(a => a != null).ToList();

            // Build response
            var response = new System.Text.StringBuilder();
            response.AppendLine(_localizer.GetFormatted("WalletScanHeader", lang, ShortenAddress(address)));

            // Show balances
            response.AppendLine(_localizer.Get("Balances", lang));
            var hasBalance = false;
            foreach (var balance in balances.Where(b => b!.BalanceInEth > 0.0001m))
            {
                hasBalance = true;
                var symbol = GetNativeSymbol(balance!.Chain);
                response.AppendLine($"  {balance.Chain}: {balance.BalanceInEth:F4} {symbol}");
            }
            if (!hasBalance)
            {
                response.AppendLine(_localizer.Get("NoBalancesFound", lang));
            }

            // Show activity
            response.AppendLine(_localizer.Get("OnChainActivity", lang));
            var hasActivity = false;
            foreach (var activity in activities.Where(a => a!.HasActivity))
            {
                hasActivity = true;
                response.AppendLine($"  {activity!.Chain}: {activity.TransactionCount} txns");
            }
            if (!hasActivity)
            {
                response.AppendLine(_localizer.Get("NoActivityFound", lang));
            }

            // Airdrop eligibility
            response.AppendLine(_localizer.Get("AirdropEligibility", lang));
            if (_airdropService != null)
            {
                try
                {
                    var eligibility = await _airdropService.CheckEligibilityAsync(address, ct);
                    if (eligibility.Count > 0)
                    {
                        var confirmed = eligibility.Where(e => e.IsEligible).ToList();
                        var claimable = eligibility.Where(e => !e.IsEligible && e.Status == "claimable").ToList();

                        if (confirmed.Count > 0)
                        {
                            response.AppendLine(_localizer.GetFormatted("AirdropsAvailable", lang, confirmed.Count));
                            foreach (var airdrop in confirmed.Take(5))
                            {
                                var statusEmoji = airdrop.HasClaimed ? "(claimed)" : "(unclaimed)";
                                var amount = airdrop.AllocationAmount.HasValue
                                    ? $" ~{airdrop.AllocationAmount:N0} {airdrop.TokenSymbol}"
                                    : "";
                                response.AppendLine($"    - {airdrop.AirdropName}{amount} {statusEmoji}");
                                if (airdrop.ClaimDeadline.HasValue && !airdrop.HasClaimed)
                                {
                                    var daysLeft = (airdrop.ClaimDeadline.Value - DateTime.UtcNow).Days;
                                    if (daysLeft <= 7)
                                    {
                                        response.AppendLine(_localizer.GetFormatted("DeadlineWarning", lang, daysLeft));
                                    }
                                }
                            }
                            if (confirmed.Count > 5)
                            {
                                response.AppendLine(_localizer.GetFormatted("AndMore", lang, confirmed.Count - 5));
                            }
                        }
                        else
                        {
                            response.AppendLine(_localizer.Get("NoEligibleAirdrops", lang));
                        }

                        // Show claimable airdrops the user should check manually
                        if (claimable.Count > 0)
                        {
                            response.AppendLine(_localizer.GetFormatted("ClaimableAirdropsHeader", lang, claimable.Count));
                            foreach (var airdrop in claimable.Take(5))
                            {
                                if (!string.IsNullOrEmpty(airdrop.ClaimUrl))
                                {
                                    response.AppendLine($"    - [{airdrop.AirdropName} ({airdrop.TokenSymbol})]({airdrop.ClaimUrl})");
                                }
                                else
                                {
                                    response.AppendLine($"    - {airdrop.AirdropName} ({airdrop.TokenSymbol})");
                                }
                                if (airdrop.ClaimDeadline.HasValue)
                                {
                                    var daysLeft = (airdrop.ClaimDeadline.Value - DateTime.UtcNow).Days;
                                    if (daysLeft <= 30)
                                    {
                                        response.AppendLine(_localizer.GetFormatted("DeadlineWarning", lang, daysLeft));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        response.AppendLine(_localizer.Get("NoActiveAirdrops", lang));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error checking eligibility for {Address}", address);
                    response.AppendLine(_localizer.Get("CouldNotCheckEligibility", lang));
                }
            }
            else
            {
                response.AppendLine(_localizer.Get("EligibilityNotConfigured", lang));
            }

            response.AppendLine(_localizer.Get("TrackWalletPrompt", lang));

            await _bot.EditMessageText(
                chatId,
                statusMsg.Id,
                response.ToString(),
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking wallet {Address}", address);
            await _bot.EditMessageText(
                chatId,
                statusMsg.Id,
                _localizer.GetFormatted("ErrorScanningWallet", lang, ex.Message),
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
        }
    }

    private async Task<WalletBalance?> SafeGetBalanceAsync(string address, string chain, CancellationToken ct)
    {
        try
        {
            return await _blockchainService.GetNativeBalanceAsync(address, chain, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get balance for {Address} on {Chain}", address, chain);
            return null;
        }
    }

    private async Task<WalletActivity?> SafeGetActivityAsync(string address, string chain, CancellationToken ct)
    {
        try
        {
            return await _blockchainService.GetWalletActivityAsync(address, chain, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get activity for {Address} on {Chain}", address, chain);
            return null;
        }
    }

    private static string GetNativeSymbol(string chain) => chain.ToLowerInvariant() switch
    {
        "polygon" => "MATIC",
        _ => "ETH"
    };

    private async Task HandlePointsCommandAsync(
        long chatId,
        long telegramId,
        string[] args,
        string? lang,
        CancellationToken ct)
    {
        if (args.Length == 0)
        {
            await _bot.SendMessage(
                chatId,
                _localizer.Get("ProvideWalletPoints", lang),
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var address = args[0];

        if (!IsValidWalletAddress(address))
        {
            await _bot.SendMessage(
                chatId,
                _localizer.Get("InvalidWalletAddress", lang),
                cancellationToken: ct);
            return;
        }

        var statusMsg = await _bot.SendMessage(
            chatId,
            _localizer.Get("FetchingPoints", lang),
            cancellationToken: ct);

        try
        {
            if (_pointsService == null)
            {
                await _bot.EditMessageText(
                    chatId,
                    statusMsg.Id,
                    _localizer.GetFormatted("PointsHeader", lang, ShortenAddress(address)) +
                    _localizer.Get("PointsNotConfigured", lang),
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct);
                return;
            }

            // Refresh points from APIs
            var pointsBalances = await _pointsService.RefreshPointsAsync(address, ct);

            var response = new System.Text.StringBuilder();
            response.AppendLine(_localizer.GetFormatted("PointsHeader", lang, ShortenAddress(address)));

            if (pointsBalances.Count > 0)
            {
                foreach (var balance in pointsBalances.OrderByDescending(b => b.Points))
                {
                    response.AppendLine($"*{balance.ProtocolName}*");
                    response.AppendLine($"  {balance.PointsName}: {balance.Points:N0}");

                    if (balance.Rank.HasValue)
                    {
                        response.AppendLine(_localizer.GetFormatted("Rank", lang, balance.Rank.Value));
                    }

                    if (balance.PointsChange24h.HasValue && balance.PointsChange24h != 0)
                    {
                        var changeSign = balance.PointsChange24h > 0 ? "+" : "";
                        response.AppendLine(_localizer.GetFormatted("Change24h", lang, changeSign, balance.PointsChange24h.Value));
                    }

                    if (balance.EstimatedValueUsd.HasValue)
                    {
                        response.AppendLine(_localizer.GetFormatted("EstimatedValue", lang, balance.EstimatedValueUsd.Value));
                    }

                    response.AppendLine();
                }

                response.AppendLine(_localizer.GetFormatted("PointsLastUpdated", lang, DateTime.UtcNow));
            }
            else
            {
                response.AppendLine(_localizer.Get("NoPointsFound", lang));
            }

            await _bot.EditMessageText(
                chatId,
                statusMsg.Id,
                response.ToString(),
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching points for {Address}", address);
            await _bot.EditMessageText(
                chatId,
                statusMsg.Id,
                _localizer.GetFormatted("ErrorFetchingPoints", lang, ShortenAddress(address)),
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
        }
    }

    private async Task HandleTrackCommandAsync(
        long chatId,
        long telegramId,
        string[] args,
        string? lang,
        CancellationToken ct)
    {
        if (args.Length == 0)
        {
            await _bot.SendMessage(
                chatId,
                _localizer.Get("ProvideWalletTrack", lang),
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var address = args[0];

        if (!IsValidWalletAddress(address))
        {
            await _bot.SendMessage(
                chatId,
                _localizer.Get("InvalidWalletAddress", lang),
                cancellationToken: ct);
            return;
        }

        var user = await _userService.GetUserByTelegramIdAsync(telegramId, ct);

        if (user.Wallets.Any(w => w.Address.Equals(address, StringComparison.OrdinalIgnoreCase)))
        {
            await _bot.SendMessage(
                chatId,
                _localizer.GetFormatted("AlreadyTrackingWallet", lang, ShortenAddress(address)),
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var chain = DetectChain(address);
        var wallet = new TrackedWallet
        {
            Address = address,
            Chain = chain,
            AddedAt = DateTime.UtcNow
        };

        await _userService.AddWalletAsync(user.Id, wallet, ct);

        await _bot.SendMessage(
            chatId,
            _localizer.GetFormatted("NowTrackingWallet", lang, ShortenAddress(address), chain),
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleWalletsCommandAsync(
        long chatId,
        long telegramId,
        string? lang,
        CancellationToken ct)
    {
        var user = await _userService.GetUserByTelegramIdAsync(telegramId, ct);

        if (user.Wallets.Count == 0)
        {
            await _bot.SendMessage(
                chatId,
                _localizer.Get("NoTrackedWallets", lang),
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var walletList = string.Join("\n", user.Wallets.Select((w, i) =>
            $"{i + 1}. `{ShortenAddress(w.Address)}` ({w.Chain})" +
            (w.Label != null ? $" - {w.Label}" : "")));

        await _bot.SendMessage(
            chatId,
            _localizer.GetFormatted("YourTrackedWallets", lang, user.Wallets.Count, walletList),
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleStatusCommandAsync(
        long chatId,
        long telegramId,
        string? lang,
        CancellationToken ct)
    {
        var user = await _userService.GetUserByTelegramIdAsync(telegramId, ct);

        var tier = user.SubscriptionTier;
        var tierDisplay = tier.ToUpperInvariant();
        var expiryInfo = user.SubscriptionExpiresAt.HasValue
            ? _localizer.GetFormatted("Expires", lang, user.SubscriptionExpiresAt.Value)
            : "";

        await _bot.SendMessage(
            chatId,
            _localizer.GetFormatted("YourStatus", lang, tierDisplay, expiryInfo, user.Wallets.Count, user.ReferralCode),
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleUpgradeCommandAsync(long chatId, string? lang, CancellationToken ct)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Tracker $9/mo", "subscribe:tracker"),
                InlineKeyboardButton.WithCallbackData("Architect $29/mo", "subscribe:architect")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("API $99/mo", "subscribe:api")
            }
        });

        await _bot.SendMessage(
            chatId,
            _localizer.Get("PricingInfo", lang),
            parseMode: ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    private async Task HandleCallbackAsync(CallbackQuery callback, CancellationToken ct)
    {
        var chatId = callback.Message!.Chat.Id;
        var telegramId = callback.From.Id;
        var data = callback.Data ?? "";
        var lang = callback.From.LanguageCode;

        if (data.StartsWith("subscribe:"))
        {
            var tier = data.Split(':')[1];
            await HandleSubscribeCallbackAsync(callback, chatId, telegramId, tier, lang, ct);
        }
        else if (data.StartsWith("reveal:"))
        {
            // reveal:<method>:<address> or reveal:<address> for legacy
            var parts = data.Split(':');
            if (parts.Length == 3)
            {
                var method = parts[1];
                var address = parts[2];
                await HandleRevealCallbackAsync(callback, chatId, telegramId, address, method, lang, ct);
            }
            else if (parts.Length == 2)
            {
                // Legacy format - show payment options
                var address = parts[1];
                await ShowRevealPaymentOptionsAsync(chatId, address, lang, ct);
            }
        }
    }

    private async Task HandleSubscribeCallbackAsync(
        CallbackQuery callback,
        long chatId,
        long telegramId,
        string tier,
        string? lang,
        CancellationToken ct)
    {
        try
        {
            await _bot.AnswerCallbackQuery(
                callback.Id,
                _localizer.Get("GeneratingCheckoutLink", lang),
                cancellationToken: ct);

            var user = await _userService.GetUserByTelegramIdAsync(telegramId, ct);

            // For Telegram, we use a simple success/cancel URL pattern
            // In production, this would be a dedicated web page
            var baseUrl = "https://t.me/GetAirdropArchitectBot";
            var successUrl = $"{baseUrl}?start=payment_success";
            var cancelUrl = $"{baseUrl}?start=payment_cancelled";

            var session = await _paymentService.CreateSubscriptionCheckoutAsync(
                user.Id,
                tier,
                successUrl,
                cancelUrl,
                ct);

            var tierName = tier.ToUpperInvariant();
            var price = tier switch
            {
                "tracker" => "$9",
                "architect" => "$29",
                "api" => "$99",
                _ => "?"
            };

            await _bot.SendMessage(
                chatId,
                _localizer.GetFormatted("UpgradeHeader", lang, tierName, price, session.Url),
                parseMode: ParseMode.Html,
                cancellationToken: ct);

            _logger.LogInformation(
                "Generated checkout link for user {UserId}, tier {Tier}",
                user.Id, tier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session for tier {Tier}", tier);
            await _bot.AnswerCallbackQuery(
                callback.Id,
                _localizer.Get("ErrorGeneratingCheckout", lang),
                showAlert: true,
                cancellationToken: ct);
        }
    }

    private async Task ShowRevealPaymentOptionsAsync(
        long chatId,
        string walletAddress,
        string? lang,
        CancellationToken ct)
    {
        var buttons = new List<InlineKeyboardButton[]>
        {
            new[] { InlineKeyboardButton.WithCallbackData(
                _localizer.Get("PayWithCard", lang),
                $"reveal:card:{walletAddress}") }
        };

        // Only show crypto option if service is configured
        if (_cryptoPaymentService != null)
        {
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(
                _localizer.Get("PayWithCrypto", lang),
                $"reveal:crypto:{walletAddress}") });
        }

        var keyboard = new InlineKeyboardMarkup(buttons);

        await _bot.SendMessage(
            chatId,
            _localizer.GetFormatted("UnlockWalletDetails", lang, ShortenAddress(walletAddress)),
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    private async Task HandleRevealCallbackAsync(
        CallbackQuery callback,
        long chatId,
        long telegramId,
        string walletAddress,
        string paymentMethod,
        string? lang,
        CancellationToken ct)
    {
        try
        {
            await _bot.AnswerCallbackQuery(
                callback.Id,
                _localizer.Get("GeneratingCheckoutLink", lang),
                cancellationToken: ct);

            var user = await _userService.GetUserByTelegramIdAsync(telegramId, ct);

            // Check if already revealed
            if (user.RevealedWallets.Contains(walletAddress, StringComparer.OrdinalIgnoreCase))
            {
                await _bot.SendMessage(
                    chatId,
                    _localizer.GetFormatted("AlreadyUnlockedWallet", lang, ShortenAddress(walletAddress)),
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct);
                return;
            }

            var baseUrl = "https://t.me/GetAirdropArchitectBot";
            var successUrl = $"{baseUrl}?start=reveal_success";
            var cancelUrl = $"{baseUrl}?start=reveal_cancelled";

            string checkoutUrl;
            string paymentMethodDisplay;

            if (paymentMethod == "crypto" && _cryptoPaymentService != null)
            {
                var metadata = new Dictionary<string, string>
                {
                    ["wallet_address"] = walletAddress
                };

                var charge = await _cryptoPaymentService.CreateChargeAsync(
                    user.Id,
                    "reveal",
                    metadata,
                    successUrl,
                    ct);

                checkoutUrl = charge.HostedUrl;
                paymentMethodDisplay = _localizer.Get("PaymentMethodCrypto", lang);

                _logger.LogInformation(
                    "Generated crypto reveal checkout for user {UserId}, wallet {Wallet}, charge {ChargeCode}",
                    user.Id, ShortenAddress(walletAddress), charge.ChargeCode);
            }
            else
            {
                var session = await _paymentService.CreateRevealCheckoutAsync(
                    user.Id,
                    walletAddress,
                    successUrl,
                    cancelUrl,
                    ct);

                checkoutUrl = session.Url;
                paymentMethodDisplay = _localizer.Get("PaymentMethodCard", lang);

                _logger.LogInformation(
                    "Generated card reveal checkout for user {UserId}, wallet {Wallet}",
                    user.Id, ShortenAddress(walletAddress));
            }

            await _bot.SendMessage(
                chatId,
                _localizer.GetFormatted("RevealCheckoutHeader", lang,
                    ShortenAddress(walletAddress), paymentMethodDisplay, checkoutUrl),
                parseMode: ParseMode.Html,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reveal checkout for wallet {Wallet}", walletAddress);
            await _bot.AnswerCallbackQuery(
                callback.Id,
                _localizer.Get("ErrorGeneratingCheckout", lang),
                showAlert: true,
                cancellationToken: ct);
        }
    }

    private static bool IsValidWalletAddress(string address)
    {
        // Ethereum-style (0x + 40 hex chars)
        if (address.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && address.Length == 42)
        {
            return address[2..].All(c => char.IsAsciiHexDigit(c));
        }

        // Solana (base58, 32-44 chars, no 0x prefix)
        if (!address.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            && address.Length >= 32
            && address.Length <= 44)
        {
            return address.All(c => char.IsLetterOrDigit(c));
        }

        return false;
    }

    private static string ShortenAddress(string address)
    {
        if (address.Length < 12) return address;
        return $"{address[..6]}...{address[^4..]}";
    }

    private static string DetectChain(string address)
    {
        if (address.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return "ethereum";
        }
        return "solana";
    }
}
