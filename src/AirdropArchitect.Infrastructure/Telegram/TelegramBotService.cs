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
    private readonly ILogger<TelegramBotService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TelegramBotService(
        ITelegramBotClient bot,
        IUserService userService,
        ILogger<TelegramBotService> logger)
    {
        _bot = bot;
        _userService = userService;
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

        var statusMsg = await _bot.SendMessage(
            chatId,
            "Scanning wallet for airdrops...",
            cancellationToken: ct);

        // TODO: Implement actual eligibility checking in Phase 2
        await _bot.EditMessageText(
            chatId,
            statusMsg.Id,
            $"Scanned `{ShortenAddress(address)}`\n\n" +
            "_Eligibility checking coming soon!_\n\n" +
            "Track this wallet with /track to get alerts.",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

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
            """;

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Pay with Card", "upgrade:card"),
                InlineKeyboardButton.WithCallbackData("Pay with Crypto", "upgrade:crypto")
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
        var data = callback.Data ?? "";

        if (data.StartsWith("upgrade:"))
        {
            var method = data.Split(':')[1];
            // TODO: Generate payment link and send to user in Phase 3
            await _bot.AnswerCallbackQuery(
                callback.Id,
                "Payment integration coming soon!",
                cancellationToken: ct);
        }
        else if (data.StartsWith("reveal:"))
        {
            var address = data.Split(':')[1];
            // TODO: Handle reveal purchase in Phase 3
            await _bot.AnswerCallbackQuery(
                callback.Id,
                "Reveal feature coming soon!",
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
