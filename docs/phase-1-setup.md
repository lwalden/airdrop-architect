# Phase 1: Foundation — Detailed Setup Guide

> Extracted from CLAUDE.md during AIAgentMinder v0.5.3 update.
> Reference this file on-demand for week-by-week tasks, shell commands, NuGet packages, and code scaffolding templates.

---

## Week 1: Project Setup

### Task 1.1: Repository Setup
```
STATUS: NOT STARTED
REQUIRES HUMAN: Yes - Create GitHub repo

PROMPT TO HUMAN:
"Please create a new GitHub repository named 'airdrop-architect' (private recommended).
Once created, provide me the repo URL and I'll initialize the project structure."
```

**After repo created, execute:**
```bash
# Clone and initialize
git clone [REPO_URL]
cd airdrop-architect

# Create solution structure
dotnet new sln -n AirdropArchitect
mkdir -p src/AirdropArchitect.Functions
mkdir -p src/AirdropArchitect.Core
mkdir -p src/AirdropArchitect.Infrastructure
mkdir -p tests/AirdropArchitect.Functions.Tests
mkdir -p tests/AirdropArchitect.Core.Tests
mkdir -p infrastructure
mkdir -p docs

# Create projects
cd src/AirdropArchitect.Core
dotnet new classlib -f net8.0
cd ../AirdropArchitect.Infrastructure
dotnet new classlib -f net8.0
cd ../AirdropArchitect.Functions
dotnet new func -f net8.0 --worker-runtime dotnet-isolated
cd ../../tests/AirdropArchitect.Core.Tests
dotnet new xunit -f net8.0
cd ../AirdropArchitect.Functions.Tests
dotnet new xunit -f net8.0

# Add to solution
cd ../..
dotnet sln add src/AirdropArchitect.Core/AirdropArchitect.Core.csproj
dotnet sln add src/AirdropArchitect.Infrastructure/AirdropArchitect.Infrastructure.csproj
dotnet sln add src/AirdropArchitect.Functions/AirdropArchitect.Functions.csproj
dotnet sln add tests/AirdropArchitect.Core.Tests/AirdropArchitect.Core.Tests.csproj
dotnet sln add tests/AirdropArchitect.Functions.Tests/AirdropArchitect.Functions.Tests.csproj

# Add project references
dotnet add src/AirdropArchitect.Functions/AirdropArchitect.Functions.csproj reference src/AirdropArchitect.Core/AirdropArchitect.Core.csproj
dotnet add src/AirdropArchitect.Functions/AirdropArchitect.Functions.csproj reference src/AirdropArchitect.Infrastructure/AirdropArchitect.Infrastructure.csproj
dotnet add src/AirdropArchitect.Infrastructure/AirdropArchitect.Infrastructure.csproj reference src/AirdropArchitect.Core/AirdropArchitect.Core.csproj
```

### Task 1.2: Core NuGet Packages
```bash
# Core project
cd src/AirdropArchitect.Core
dotnet add package Microsoft.Azure.Cosmos
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
dotnet add package Newtonsoft.Json

# Infrastructure project
cd ../AirdropArchitect.Infrastructure
dotnet add package Alchemy.Sdk  # or Nethereum if Alchemy SDK unavailable
dotnet add package Telegram.Bot
dotnet add package Stripe.net
dotnet add package Microsoft.Azure.Cosmos

# Functions project
cd ../AirdropArchitect.Functions
dotnet add package Microsoft.Azure.Functions.Worker
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.Http
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.Timer
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.Storage.Queues
dotnet add package Microsoft.Extensions.DependencyInjection
```

### Task 1.3: Environment Configuration

**Create `.env.example`:**
```env
# Azure
AZURE_COSMOS_CONNECTION_STRING=
AZURE_COSMOS_DATABASE_NAME=airdrop-db
AZURE_REDIS_CONNECTION_STRING=

# Blockchain APIs
ALCHEMY_API_KEY=
HELIUS_API_KEY=

# Telegram
TELEGRAM_BOT_TOKEN=

# Payments
STRIPE_SECRET_KEY=
STRIPE_WEBHOOK_SECRET=
COINBASE_COMMERCE_API_KEY=
COINBASE_COMMERCE_WEBHOOK_SECRET=
```

**Create `.gitignore`:**
```gitignore
# Environment
.env
local.settings.json
appsettings.Development.json

# Build
bin/
obj/
.vs/
*.user

# Dependencies
node_modules/

# IDE
.idea/
*.swp
```

### Task 1.4: Azure Setup
```
STATUS: NOT STARTED
REQUIRES HUMAN: Yes - Azure account and resource creation

PROMPT TO HUMAN:
"We need to set up Azure resources. Please:

1. Log into Azure Portal (https://portal.azure.com)
   - If you don't have an account, create one (free tier available)

2. Create a Resource Group:
   - Name: 'airdrop-architect-rg'
   - Region: West US 2 (or your preferred region)

3. Create Cosmos DB account:
   - Name: 'airdrop-architect-cosmos'
   - API: Core (SQL)
   - Capacity mode: Serverless (cheapest for development)
   - Create a database named 'airdrop-db'
   - Create containers: 'users', 'airdrops', 'eligibility', 'points', 'snapshots'

4. Create Azure Redis Cache:
   - Name: 'airdrop-architect-redis'
   - Pricing tier: Basic C0 (cheapest, ~$15/mo)

5. Provide me with:
   - Cosmos DB connection string
   - Redis connection string

I'll store these in your local .env file (gitignored)."
```

---

## Week 2: Telegram Bot Foundation

### Task 2.1: Create Telegram Bot
```
STATUS: NOT STARTED
REQUIRES HUMAN: Yes - Telegram bot creation

PROMPT TO HUMAN:
"Please create a Telegram bot:

1. Open Telegram and search for @BotFather
2. Send /newbot
3. Choose a name: 'Airdrop Architect' (or similar)
4. Choose a username: 'AirdropArchitectBot' (must end in 'bot')
5. Copy the API token BotFather gives you

Please provide the bot token and I'll add it to your .env file."
```

### Task 2.2: Implement Telegram Bot Service
**File: `src/AirdropArchitect.Infrastructure/Telegram/TelegramBotService.cs`**

```csharp
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

    public TelegramBotService(
        string botToken,
        IUserService userService,
        ILogger<TelegramBotService> logger)
    {
        _bot = new TelegramBotClient(botToken);
        _userService = userService;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken ct = default)
    {
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
            _logger.LogError(ex, "Error handling Telegram update");
        }
    }

    private async Task HandleMessageAsync(Message message, CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        var text = message.Text!;
        var userId = message.From?.Id.ToString() ?? "";

        // Ensure user exists in our system
        await _userService.EnsureUserExistsAsync(userId, chatId);

        var command = text.Split(' ')[0].ToLowerInvariant();
        var args = text.Split(' ').Skip(1).ToArray();

        switch (command)
        {
            case "/start":
                await SendWelcomeAsync(chatId, ct);
                break;
            case "/check":
                await HandleCheckCommandAsync(chatId, userId, args, ct);
                break;
            case "/points":
                await HandlePointsCommandAsync(chatId, userId, args, ct);
                break;
            case "/track":
                await HandleTrackCommandAsync(chatId, userId, args, ct);
                break;
            case "/status":
                await HandleStatusCommandAsync(chatId, userId, ct);
                break;
            case "/upgrade":
                await HandleUpgradeCommandAsync(chatId, userId, ct);
                break;
            case "/help":
                await SendHelpAsync(chatId, ct);
                break;
            default:
                // Check if it's a wallet address
                if (IsValidWalletAddress(text))
                {
                    await HandleCheckCommandAsync(chatId, userId, new[] { text }, ct);
                }
                break;
        }
    }

    private async Task SendWelcomeAsync(long chatId, CancellationToken ct)
    {
        var welcome = """
            🏗️ *Welcome to Airdrop Architect!*

            I help you find unclaimed airdrops and track points across your wallets.

            *Quick Start:*
            Just paste any wallet address to check for airdrops!

            *Commands:*
            /check `<address>` - Check for airdrops
            /points `<address>` - Check points balances
            /track `<address>` - Track wallet for alerts
            /status - Your subscription status
            /upgrade - Premium features
            /help - Show all commands
            """;

        await _bot.SendTextMessageAsync(
            chatId,
            welcome,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task SendHelpAsync(long chatId, CancellationToken ct)
    {
        var help = """
            📖 *Airdrop Architect Commands*

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
            • You can just paste a wallet address directly
            • Supports Ethereum, Arbitrum, Optimism, Base, Solana
            """;

        await _bot.SendTextMessageAsync(
            chatId,
            help,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleCheckCommandAsync(
        long chatId,
        string userId,
        string[] args,
        CancellationToken ct)
    {
        if (args.Length == 0)
        {
            await _bot.SendTextMessageAsync(
                chatId,
                "Please provide a wallet address:\n`/check 0x123...abc`",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var address = args[0];

        if (!IsValidWalletAddress(address))
        {
            await _bot.SendTextMessageAsync(
                chatId,
                "❌ Invalid wallet address. Please check and try again.",
                cancellationToken: ct);
            return;
        }

        // Send "checking" message
        var statusMsg = await _bot.SendTextMessageAsync(
            chatId,
            "🔍 Scanning wallet for airdrops...",
            cancellationToken: ct);

        // TODO: Implement actual eligibility checking in Phase 2
        // For now, return placeholder
        await _bot.EditMessageTextAsync(
            chatId,
            statusMsg.MessageId,
            $"✅ Scanned `{ShortenAddress(address)}`\n\n" +
            "_Eligibility checking coming soon!_\n\n" +
            "Track this wallet with /track to get alerts.",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandlePointsCommandAsync(
        long chatId,
        string userId,
        string[] args,
        CancellationToken ct)
    {
        if (args.Length == 0)
        {
            await _bot.SendTextMessageAsync(
                chatId,
                "Please provide a wallet address:\n`/points 0x123...abc`",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var address = args[0];

        var statusMsg = await _bot.SendTextMessageAsync(
            chatId,
            "📊 Fetching points balances...",
            cancellationToken: ct);

        // TODO: Implement actual points checking in Phase 2
        await _bot.EditMessageTextAsync(
            chatId,
            statusMsg.MessageId,
            $"📊 Points for `{ShortenAddress(address)}`\n\n" +
            "_Points tracking coming soon!_\n\n" +
            "We'll support Hyperliquid, EigenLayer, Blast, and more.",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleTrackCommandAsync(
        long chatId,
        string userId,
        string[] args,
        CancellationToken ct)
    {
        if (args.Length == 0)
        {
            await _bot.SendTextMessageAsync(
                chatId,
                "Please provide a wallet address:\n`/track 0x123...abc`",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        var address = args[0];

        if (!IsValidWalletAddress(address))
        {
            await _bot.SendTextMessageAsync(
                chatId,
                "❌ Invalid wallet address.",
                cancellationToken: ct);
            return;
        }

        // TODO: Actually add to tracking
        await _bot.SendTextMessageAsync(
            chatId,
            $"✅ Now tracking `{ShortenAddress(address)}`\n\n" +
            "You'll receive alerts when:\n" +
            "• New airdrops become available\n" +
            "• Claim deadlines approach\n" +
            "• Points balances change significantly",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleStatusCommandAsync(
        long chatId,
        string userId,
        CancellationToken ct)
    {
        var user = await _userService.GetUserAsync(userId);

        var status = user?.SubscriptionTier ?? "free";
        var emoji = status switch
        {
            "architect" => "⭐",
            "tracker" => "📡",
            "api" => "🔌",
            _ => "🆓"
        };

        await _bot.SendTextMessageAsync(
            chatId,
            $"{emoji} *Your Status*\n\n" +
            $"Plan: *{status.ToUpperInvariant()}*\n" +
            $"Tracked wallets: {user?.Wallets?.Count ?? 0}\n\n" +
            "Use /upgrade to unlock more features!",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleUpgradeCommandAsync(
        long chatId,
        string userId,
        CancellationToken ct)
    {
        var pricing = """
            ⭐ *Airdrop Architect Premium*

            *🆓 Free*
            • Check 3 wallets/month
            • Basic eligibility info

            *📡 Tracker - $9/month*
            • Track up to 10 wallets
            • Telegram alerts
            • Claim deadline reminders

            *⭐ Architect - $29/month*
            • Unlimited wallets
            • Points dashboard
            • Path-to-eligibility tips
            • Sybil protection analyzer

            *🔌 API - $99/month*
            • Everything in Architect
            • REST API access
            • Webhooks
            """;

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("💳 Pay with Card", "upgrade:card"),
                InlineKeyboardButton.WithCallbackData("₿ Pay with Crypto", "upgrade:crypto")
            }
        });

        await _bot.SendTextMessageAsync(
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
            // TODO: Generate payment link and send to user
            await _bot.AnswerCallbackQueryAsync(
                callback.Id,
                "Payment integration coming soon!",
                cancellationToken: ct);
        }
        else if (data.StartsWith("reveal:"))
        {
            var address = data.Split(':')[1];
            // TODO: Handle reveal purchase
            await _bot.AnswerCallbackQueryAsync(
                callback.Id,
                "Reveal feature coming soon!",
                cancellationToken: ct);
        }
    }

    private static bool IsValidWalletAddress(string address)
    {
        // Ethereum-style (0x + 40 hex chars)
        if (address.StartsWith("0x") && address.Length == 42)
            return true;

        // Solana (base58, 32-44 chars, no 0x prefix)
        if (!address.StartsWith("0x") && address.Length >= 32 && address.Length <= 44)
            return true;

        return false;
    }

    private static string ShortenAddress(string address)
    {
        if (address.Length < 12) return address;
        return $"{address[..6]}...{address[^4..]}";
    }
}
```

### Task 2.3: Telegram Webhook Function
**File: `src/AirdropArchitect.Functions/Telegram/TelegramWebhookFunction.cs`**

```csharp
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using AirdropArchitect.Core.Interfaces;

namespace AirdropArchitect.Functions.Telegram;

public class TelegramWebhookFunction
{
    private readonly ITelegramBotService _telegramService;
    private readonly ILogger<TelegramWebhookFunction> _logger;

    public TelegramWebhookFunction(
        ITelegramBotService telegramService,
        ILogger<TelegramWebhookFunction> logger)
    {
        _telegramService = telegramService;
        _logger = logger;
    }

    [Function("TelegramWebhook")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "telegram/webhook")]
        HttpRequestData req,
        CancellationToken cancellationToken)
    {
        try
        {
            var body = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(body))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var update = JsonConvert.DeserializeObject<Update>(body);

            if (update != null)
            {
                await _telegramService.HandleUpdateAsync(update, cancellationToken);
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Telegram webhook");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
```

---

## Week 3: External Service Setup

### Task 3.1: Alchemy API Setup
```
STATUS: NOT STARTED
REQUIRES HUMAN: Yes - Alchemy account creation

PROMPT TO HUMAN:
"Please set up an Alchemy account for blockchain API access:

1. Go to https://www.alchemy.com/
2. Sign up for a free account
3. Create a new app:
   - Name: 'Airdrop Architect'
   - Chain: Ethereum Mainnet (we'll add more later)
4. Copy your API key from the dashboard

The free tier gives us 300M compute units/month - plenty for development.

Please provide the API key and I'll add it to your .env file."
```

### Task 3.2: Stripe Account Setup
```
STATUS: NOT STARTED
REQUIRES HUMAN: Yes - Stripe account creation

PROMPT TO HUMAN:
"Please set up a Stripe account for card payments:

1. Go to https://stripe.com/
2. Sign up for an account
3. Complete basic business verification (can use your name/address for now)
4. From the Developer Dashboard:
   - Copy your Secret Key (starts with sk_test_)
   - Set up a webhook endpoint (we'll configure the URL later)

5. Create these Products in Stripe:
   - 'Tracker Monthly' - $9/month recurring
   - 'Architect Monthly' - $29/month recurring
   - 'API Monthly' - $99/month recurring
   - 'Wallet Reveal' - $5 one-time

Please provide:
- Secret Key (sk_test_...)
- The Price IDs for each product (price_...)"
```

### Task 3.3: Coinbase Commerce Setup
```
STATUS: NOT STARTED
REQUIRES HUMAN: Yes - Coinbase Commerce account creation

PROMPT TO HUMAN:
"Please set up Coinbase Commerce for crypto payments:

1. Go to https://commerce.coinbase.com/
2. Sign up (you can use your existing Coinbase account)
3. Complete merchant verification
4. From Settings > Security:
   - Create an API key
   - Set up webhook notifications

5. Configure accepted currencies:
   - Enable: BTC, ETH, USDC, DAI

Please provide:
- API Key
- Webhook Shared Secret

Note: Coinbase Commerce charges 1% per transaction (vs Stripe's 2.9%+$0.30)"
```

---

## Week 4: Core Models & Database

### Task 4.1: Create Core Models

**File: `src/AirdropArchitect.Core/Models/User.cs`**

```csharp
using Newtonsoft.Json;

namespace AirdropArchitect.Core.Models;

public class User
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("partitionKey")]
    public string PartitionKey { get; set; } = "users";

    [JsonProperty("telegramId")]
    public long? TelegramId { get; set; }

    [JsonProperty("email")]
    public string? Email { get; set; }

    [JsonProperty("subscriptionTier")]
    public string SubscriptionTier { get; set; } = "free";

    [JsonProperty("subscriptionExpiresAt")]
    public DateTime? SubscriptionExpiresAt { get; set; }

    [JsonProperty("stripeCustomerId")]
    public string? StripeCustomerId { get; set; }

    [JsonProperty("stripeSubscriptionId")]
    public string? StripeSubscriptionId { get; set; }

    [JsonProperty("wallets")]
    public List<TrackedWallet> Wallets { get; set; } = new();

    [JsonProperty("revealedWallets")]
    public List<string> RevealedWallets { get; set; } = new();

    [JsonProperty("referralCode")]
    public string ReferralCode { get; set; } = GenerateReferralCode();

    [JsonProperty("referredBy")]
    public string? ReferredBy { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private static string GenerateReferralCode()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "")
            .Replace("+", "")
            .Replace("=", "")
            [..8]
            .ToUpper();
    }
}

public class TrackedWallet
{
    [JsonProperty("address")]
    public string Address { get; set; } = "";

    [JsonProperty("chain")]
    public string Chain { get; set; } = "ethereum";

    [JsonProperty("label")]
    public string? Label { get; set; }

    [JsonProperty("addedAt")]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
```

**File: `src/AirdropArchitect.Core/Models/Airdrop.cs`**

```csharp
using Newtonsoft.Json;

namespace AirdropArchitect.Core.Models;

public class Airdrop
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("partitionKey")]
    public string PartitionKey { get; set; } = "airdrops";

    [JsonProperty("name")]
    public string Name { get; set; } = "";

    [JsonProperty("tokenSymbol")]
    public string TokenSymbol { get; set; } = "";

    [JsonProperty("chain")]
    public string Chain { get; set; } = "ethereum";

    [JsonProperty("status")]
    public string Status { get; set; } = "upcoming"; // upcoming, claimable, expired

    [JsonProperty("claimContract")]
    public string? ClaimContract { get; set; }

    [JsonProperty("claimUrl")]
    public string? ClaimUrl { get; set; }

    [JsonProperty("claimDeadline")]
    public DateTime? ClaimDeadline { get; set; }

    [JsonProperty("snapshotDate")]
    public DateTime? SnapshotDate { get; set; }

    [JsonProperty("eligibilitySource")]
    public string? EligibilitySource { get; set; } // URL to eligibility list

    [JsonProperty("criteria")]
    public List<string> Criteria { get; set; } = new();

    [JsonProperty("totalEligibleAddresses")]
    public int? TotalEligibleAddresses { get; set; }

    [JsonProperty("averageAllocationUsd")]
    public decimal? AverageAllocationUsd { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

**File: `src/AirdropArchitect.Core/Models/PointsProgram.cs`**

```csharp
using Newtonsoft.Json;

namespace AirdropArchitect.Core.Models;

public class PointsProgram
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("partitionKey")]
    public string PartitionKey { get; set; } = "points";

    [JsonProperty("protocolName")]
    public string ProtocolName { get; set; } = "";

    [JsonProperty("pointsName")]
    public string PointsName { get; set; } = "Points";

    [JsonProperty("chain")]
    public string Chain { get; set; } = "ethereum";

    [JsonProperty("status")]
    public string Status { get; set; } = "active"; // active, ended, upcoming

    [JsonProperty("dashboardUrl")]
    public string? DashboardUrl { get; set; }

    [JsonProperty("apiEndpoint")]
    public string? ApiEndpoint { get; set; }

    [JsonProperty("trackingMethod")]
    public string TrackingMethod { get; set; } = "manual"; // api, scrape, manual

    [JsonProperty("estimatedTgeDate")]
    public string? EstimatedTgeDate { get; set; }

    [JsonProperty("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class PointsSnapshot
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("partitionKey")]
    public string PartitionKey { get; set; } // "snapshot-{walletAddress}"

    [JsonProperty("walletAddress")]
    public string WalletAddress { get; set; } = "";

    [JsonProperty("protocolId")]
    public string ProtocolId { get; set; } = "";

    [JsonProperty("points")]
    public decimal Points { get; set; }

    [JsonProperty("rank")]
    public int? Rank { get; set; }

    [JsonProperty("percentile")]
    public decimal? Percentile { get; set; }

    [JsonProperty("snapshotDate")]
    public DateTime SnapshotDate { get; set; }

    [JsonProperty("previousPoints")]
    public decimal? PreviousPoints { get; set; }

    [JsonProperty("pointsChange")]
    public decimal? PointsChange { get; set; }
}
```
