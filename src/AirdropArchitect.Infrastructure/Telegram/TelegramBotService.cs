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
        ILogger<TelegramBotService> logger)
    {
        _bot = bot;
        _userService = userService;
        _blockchainService = blockchainService;
        _paymentService = paymentService;
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

        await _userService.EnsureUserExistsAsync(telegramId, ct);

        var command = text.Split(' ')[0].ToLowerInvariant();
        var args = text.Split(' ').Skip(1).ToArray();

        switch (command)
        {
            case "/start":
                await SendWelcomeAsync(chatId, ct);
                break;
            case "/check":
                await HandleCheckCommandAsync(chatId, telegramId, args, ct);
                break;
            case "/points":
                await HandlePointsCommandAsync(chatId, telegramId, args, ct);
                break;
            case "/track":
                await HandleTrackCommandAsync(chatId, telegramId, args, ct);
                break;
            case "/wallets":
                await HandleWalletsCommandAsync(chatId, telegramId, ct);
                break;
            case "/status":
                await HandleStatusCommandAsync(chatId, telegramId, ct);
                break;
            case "/upgrade":
                await HandleUpgradeCommandAsync(chatId, ct);
                break;
            case "/help":
                await SendHelpAsync(chatId, ct);
                break;
            default:
                if (IsValidWalletAddress(text))
                {
                    await HandleCheckCommandAsync(chatId, telegramId, new[] { text }, ct);
                }
                break;
        }
    }

    private async Task SendWelcomeAsync(long chatId, CancellationToken ct)
    {
        var welcome = """
            *Welcome to Airdrop Architect!*

            I help you find unclaimed airdrops and track points across your wallets.

            *Quick Start:*
            Just paste any wallet address to check for airdrops!

            *Commands:*
            /check `<address>` - Check for airdrops
            /points `<address>` - Check points balances
            /track `<address>` - Track wallet for alerts
            /wallets - List your tracked wallets
            /status - Your subscription status
            /upgrade - Premium features
            /help - Show all commands
            """;

        await _bot.SendMessage(
            chatId,
            welcome,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task SendHelpAsync(long chatId, CancellationToken ct)
    {
        var help = """
            *Airdrop Architect Commands*

            *Checking Wallets:*
            /check `0x...` - Check wallet for unclaimed airdrops
            /points `0x...` - Check points balances (Hyperliquid, EigenLayer, etc.)

            *Tracking:*
            /track `0x...` - Add wallet to tracking
            /untrack `0x...` - Remove wallet from tracking
            /wallets - List your tracked wallets

            *Account:*
            /status - Your subscription status
            /upgrade - View premium options
            /alerts - Manage notification settings

            *Tips:*
            - You can just paste a wallet address directly
            - Supports Ethereum, Arbitrum, Optimism, Base, Solana
            """;

        await _bot.SendMessage(
            chatId,
            help,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleCheckCommandAsync(
        long chatId,
        long telegramId,
        string[] args,
        CancellationToken ct)
    {
        if (args.Length == 0)
        {
            await _bot.SendMessage(
                chatId,
                "Please provide a wallet address:\n`/check 0x123...abc`",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var address = args[0];

        if (!IsValidWalletAddress(address))
        {
            await _bot.SendMessage(
                chatId,
                "Invalid wallet address. Please check and try again.",
                cancellationToken: ct);
            return;
        }

        // Detect chain and check if EVM
        var chain = DetectChain(address);
        if (chain == "solana")
        {
            await _bot.SendMessage(
                chatId,
                $"Solana support coming soon! `{ShortenAddress(address)}`\n\n" +
                "Currently supporting: Ethereum, Arbitrum, Optimism, Base, Polygon",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var statusMsg = await _bot.SendMessage(
            chatId,
            "Scanning wallet across EVM chains...",
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
            response.AppendLine($"*Wallet Scan: `{ShortenAddress(address)}`*\n");

            // Show balances
            response.AppendLine("*Balances:*");
            var hasBalance = false;
            foreach (var balance in balances.Where(b => b!.BalanceInEth > 0.0001m))
            {
                hasBalance = true;
                var symbol = GetNativeSymbol(balance!.Chain);
                response.AppendLine($"  {balance.Chain}: {balance.BalanceInEth:F4} {symbol}");
            }
            if (!hasBalance)
            {
                response.AppendLine("  _No significant balances found_");
            }

            // Show activity
            response.AppendLine("\n*On-chain Activity:*");
            var hasActivity = false;
            foreach (var activity in activities.Where(a => a!.HasActivity))
            {
                hasActivity = true;
                response.AppendLine($"  {activity!.Chain}: {activity.TransactionCount} txns");
            }
            if (!hasActivity)
            {
                response.AppendLine("  _No on-chain activity found_");
            }

            // Eligibility hints (coming soon)
            response.AppendLine("\n*Airdrop Eligibility:*");
            response.AppendLine("  _Full eligibility checking coming soon!_");

            response.AppendLine($"\nTrack this wallet with /track to get alerts.");

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
                $"Error scanning wallet. Please try again later.\n\n`{ex.Message}`",
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
        CancellationToken ct)
    {
        if (args.Length == 0)
        {
            await _bot.SendMessage(
                chatId,
                "Please provide a wallet address:\n`/points 0x123...abc`",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var address = args[0];

        if (!IsValidWalletAddress(address))
        {
            await _bot.SendMessage(
                chatId,
                "Invalid wallet address. Please check and try again.",
                cancellationToken: ct);
            return;
        }

        var statusMsg = await _bot.SendMessage(
            chatId,
            "Fetching points balances...",
            cancellationToken: ct);

        // TODO: Implement actual points checking in Phase 2
        await _bot.EditMessageText(
            chatId,
            statusMsg.Id,
            $"Points for `{ShortenAddress(address)}`\n\n" +
            "_Points tracking coming soon!_\n\n" +
            "We'll support Hyperliquid, EigenLayer, Blast, and more.",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleTrackCommandAsync(
        long chatId,
        long telegramId,
        string[] args,
        CancellationToken ct)
    {
        if (args.Length == 0)
        {
            await _bot.SendMessage(
                chatId,
                "Please provide a wallet address:\n`/track 0x123...abc`",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var address = args[0];

        if (!IsValidWalletAddress(address))
        {
            await _bot.SendMessage(
                chatId,
                "Invalid wallet address.",
                cancellationToken: ct);
            return;
        }

        var user = await _userService.GetUserByTelegramIdAsync(telegramId, ct);

        if (user.Wallets.Any(w => w.Address.Equals(address, StringComparison.OrdinalIgnoreCase)))
        {
            await _bot.SendMessage(
                chatId,
                $"You're already tracking `{ShortenAddress(address)}`",
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
            $"Now tracking `{ShortenAddress(address)}` ({chain})\n\n" +
            "You'll receive alerts when:\n" +
            "- New airdrops become available\n" +
            "- Claim deadlines approach\n" +
            "- Points balances change significantly",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleWalletsCommandAsync(
        long chatId,
        long telegramId,
        CancellationToken ct)
    {
        var user = await _userService.GetUserByTelegramIdAsync(telegramId, ct);

        if (user.Wallets.Count == 0)
        {
            await _bot.SendMessage(
                chatId,
                "You're not tracking any wallets yet.\n\n" +
                "Use /track `<address>` to start tracking a wallet.",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var walletList = string.Join("\n", user.Wallets.Select((w, i) =>
            $"{i + 1}. `{ShortenAddress(w.Address)}` ({w.Chain})" +
            (w.Label != null ? $" - {w.Label}" : "")));

        await _bot.SendMessage(
            chatId,
            $"*Your Tracked Wallets ({user.Wallets.Count}):*\n\n{walletList}",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleStatusCommandAsync(
        long chatId,
        long telegramId,
        CancellationToken ct)
    {
        var user = await _userService.GetUserByTelegramIdAsync(telegramId, ct);

        var tier = user.SubscriptionTier;
        var emoji = tier switch
        {
            "architect" => "star",
            "tracker" => "satellite",
            "api" => "electric_plug",
            _ => "free"
        };

        var tierDisplay = tier.ToUpperInvariant();
        var expiryInfo = user.SubscriptionExpiresAt.HasValue
            ? $"\nExpires: {user.SubscriptionExpiresAt:yyyy-MM-dd}"
            : "";

        await _bot.SendMessage(
            chatId,
            $"*Your Status*\n\n" +
            $"Plan: *{tierDisplay}*{expiryInfo}\n" +
            $"Tracked wallets: {user.Wallets.Count}\n" +
            $"Referral code: `{user.ReferralCode}`\n\n" +
            "Use /upgrade to unlock more features!",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleUpgradeCommandAsync(long chatId, CancellationToken ct)
    {
        var pricing = """
            *Airdrop Architect Premium*

            *FREE*
            - Check 3 wallets/month
            - Basic eligibility info

            *TRACKER - $9/month*
            - Track up to 10 wallets
            - Telegram alerts
            - Claim deadline reminders

            *ARCHITECT - $29/month*
            - Unlimited wallets
            - Points dashboard
            - Path-to-eligibility tips
            - Sybil protection analyzer

            *API - $99/month*
            - Everything in Architect
            - REST API access
            - Webhooks

            Select a plan to upgrade:
            """;

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
            pricing,
            parseMode: ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    private async Task HandleCallbackAsync(CallbackQuery callback, CancellationToken ct)
    {
        var chatId = callback.Message!.Chat.Id;
        var telegramId = callback.From.Id;
        var data = callback.Data ?? "";

        if (data.StartsWith("subscribe:"))
        {
            var tier = data.Split(':')[1];
            await HandleSubscribeCallbackAsync(callback, chatId, telegramId, tier, ct);
        }
        else if (data.StartsWith("reveal:"))
        {
            var address = data.Split(':')[1];
            await HandleRevealCallbackAsync(callback, chatId, telegramId, address, ct);
        }
    }

    private async Task HandleSubscribeCallbackAsync(
        CallbackQuery callback,
        long chatId,
        long telegramId,
        string tier,
        CancellationToken ct)
    {
        try
        {
            await _bot.AnswerCallbackQuery(callback.Id, "Generating checkout link...", cancellationToken: ct);

            var user = await _userService.GetUserByTelegramIdAsync(telegramId, ct);

            // For Telegram, we use a simple success/cancel URL pattern
            // In production, this would be a dedicated web page
            var baseUrl = "https://t.me/AirdropArchitectBot";
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

            // Use HTML mode to avoid Markdown corrupting URLs with underscores
            await _bot.SendMessage(
                chatId,
                $"<b>Upgrade to {tierName}</b>\n\n" +
                $"Price: {price}/month\n\n" +
                $"Click the link below to complete your purchase:\n" +
                $"{session.Url}\n\n" +
                $"<i>Link expires in 24 hours</i>",
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
                "Error generating checkout link. Please try again.",
                showAlert: true,
                cancellationToken: ct);
        }
    }

    private async Task HandleRevealCallbackAsync(
        CallbackQuery callback,
        long chatId,
        long telegramId,
        string walletAddress,
        CancellationToken ct)
    {
        try
        {
            await _bot.AnswerCallbackQuery(callback.Id, "Generating checkout link...", cancellationToken: ct);

            var user = await _userService.GetUserByTelegramIdAsync(telegramId, ct);

            // Check if already revealed
            if (user.RevealedWallets.Contains(walletAddress, StringComparer.OrdinalIgnoreCase))
            {
                await _bot.SendMessage(
                    chatId,
                    $"You've already unlocked details for `{ShortenAddress(walletAddress)}`",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct);
                return;
            }

            var baseUrl = "https://t.me/AirdropArchitectBot";
            var successUrl = $"{baseUrl}?start=reveal_success";
            var cancelUrl = $"{baseUrl}?start=reveal_cancelled";

            var session = await _paymentService.CreateRevealCheckoutAsync(
                user.Id,
                walletAddress,
                successUrl,
                cancelUrl,
                ct);

            // Use HTML mode to avoid Markdown corrupting URLs with underscores
            await _bot.SendMessage(
                chatId,
                $"<b>Unlock Wallet Details</b>\n\n" +
                $"Wallet: <code>{ShortenAddress(walletAddress)}</code>\n" +
                $"Price: $5 one-time\n\n" +
                $"Click the link below to unlock:\n" +
                $"{session.Url}\n\n" +
                $"<i>Link expires in 24 hours</i>",
                parseMode: ParseMode.Html,
                cancellationToken: ct);

            _logger.LogInformation(
                "Generated reveal checkout link for user {UserId}, wallet {Wallet}",
                user.Id, ShortenAddress(walletAddress));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reveal checkout for wallet {Wallet}", walletAddress);
            await _bot.AnswerCallbackQuery(
                callback.Id,
                "Error generating checkout link. Please try again.",
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
