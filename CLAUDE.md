# CLAUDE.md - Airdrop Architect Development Orchestration

## Reference Documents

- **Strategy & Roadmap:** See `/docs/strategy-roadmap.md` for full business context,
  revenue model, competitive analysis, and technical architecture decisions.
- **Progress Tracking:** See `/PROGRESS.md` for current development state and session continuity
- **Decision Log:** See `/DECISIONS.md` for architectural decisions made

---

## Session Resume Protocol

**IMPORTANT: At the start of EVERY new session, follow these steps:**

1. **Read `/PROGRESS.md` first** - This tells you current phase, completed tasks, blockers, and what to do next
2. **Check `/DECISIONS.md`** - Review any architectural decisions so you don't re-ask resolved questions
3. **Run `git status`** - Check for any uncommitted work from previous sessions
4. **Run `gh pr list`** - Check for any open PRs awaiting review
5. **Check the "Blocked / Awaiting Human Action" section** - Ask the user about any items pending their input
6. **Resume from "Next Session Should" in PROGRESS.md**

**At the END of every session:**
1. Update `/PROGRESS.md` with:
   - Any completed tasks (add to Completed Tasks table)
   - Any new blockers (add to Blocked section)
   - What the next session should focus on
   - A brief session summary
2. Update `/DECISIONS.md` if any architectural decisions were made
3. Commit progress tracking changes if meaningful work was done

**When resuming and the user says "continue where we left off":**
- Read PROGRESS.md
- Summarize the current state briefly
- Ask about any blocked items
- Propose next steps based on "Next Session Should"

**Mid-Session Token Cap Handling:**

Sessions may end unexpectedly when Claude Pro token limits are reached. To minimize lost work:

1. **Write code to files immediately** - Don't accumulate large changes in memory; write each file as it's completed
2. **Commit at natural checkpoints:**
   - After solution/project compiles successfully
   - After tests pass
   - After completing a logical unit of work (one task/feature)
3. **Update PROGRESS.md before multi-step operations** - If starting a complex task, note what you're about to do
4. **Prefer smaller, frequent commits** over one large commit at the end

**If session ends mid-task:**
- Files already written to disk are preserved
- Uncommitted changes in staged files are preserved
- Conversation context is NOT preserved (but can be scrolled back in the UI)
- Next session: run `git status` and `git diff` to see partial work, then continue
  

## Project Overview

You are orchestrating the development of **Airdrop Architect**, a crypto airdrop eligibility checker and points tracking SaaS platform. This document provides the master plan, phase breakdowns, and specific instructions for working with the human developer (Laurance).

**Human Developer Profile:**
- 12 years C# backend experience
- Azure cloud services expertise
- Currently employed (limited time availability)
- Prefers collaboration over autonomous execution of major decisions
- Will create GitHub repos and merge PRs manually
- Needs prompting for external service signups and credentials

**Communication Preferences:**
- Provide TL;DR summary first, then detailed explanation
- Assume always available when working (no need to batch questions)
- Medium risk tolerance: avoid bleeding edge/pre-release packages when possible

---

## Your Capabilities & Boundaries

### You CAN:
- Create files and directory structures
- Search the internet for documentation, APIs, tutorials
- Install packages (npm, NuGet, pip, etc.)
- Use CLI and bash commands
- Create branches and pull requests in GitHub
- Create sub-agents to parallelize work
- Scaffold code, write implementations, create tests

### You SHOULD ASK the human to:
- Create new GitHub repositories
- Merge pull requests
- Sign up for external services (Azure, Stripe, Coinbase, etc.)
- Provide API keys and credentials
- Make billing/payment decisions
- Approve major architectural changes
- Review and approve PRs before merge

### Credential Handling:
- NEVER store credentials in code files
- Use environment variables or Azure Key Vault
- When you need a credential, ask: "Please provide your [SERVICE] API key. I'll store it in .env (gitignored) for local development."
- Create `.env.example` files showing required variables WITHOUT values

### Git Workflow:
- **NEVER commit directly to main** - Always use feature branches
- Branch naming: `feature/short-description` or `fix/short-description`
- Claude CAN: create branches, commit, push to remote, open PRs via `gh pr create`
- Claude CANNOT: merge PRs to main (user does this manually)
- After user merges PR: switch back to main, pull, then start next feature branch

**Typical flow:**
```bash
git checkout -b feature/telegram-bot-skeleton
# ... make changes, commit frequently ...
git push -u origin feature/telegram-bot-skeleton
gh pr create --title "Add Telegram bot skeleton" --body "..."
# Wait for user to review and merge
# User says "merged" → git checkout main && git pull
```

### Pull Request Format:
- PRs should include:
  - Clear title describing the change
  - Summary of what was changed and why
  - Test plan (manual testing steps or automated test references)
  - Any follow-up items or known limitations
- Keep PRs reasonably sized - prefer multiple smaller PRs over one massive PR
- After PR is merged, update PROGRESS.md with completed task

### Test Plan Format:
```markdown
## Test Plan
- [ ] Step 1: Description of what to test
- [ ] Step 2: Expected outcome
- [ ] Verify: Specific verification steps

**Prerequisites:** What needs to be set up before testing
**Environment:** Local / Azure / Both
```

---

## Technology Stack

```
Backend:        C# / .NET 8 / Azure Functions
Database:       Azure Cosmos DB (NoSQL)
Cache:          Azure Redis Cache
Frontend:       React 18 + TypeScript + Tailwind CSS
Bot:            Telegram Bot API (via Telegram.Bot NuGet package)
Payments:       Stripe + Coinbase Commerce
Blockchain:     Alchemy SDK (EVM) + Helius SDK (Solana)
AI (Phase 2):   Azure OpenAI + Semantic Kernel
```

---

## Project Structure

```
/AirdropArchitect
├── /src
│   ├── /AirdropArchitect.Functions      # Azure Functions (.NET 8)
│   │   ├── /Eligibility
│   │   ├── /Points
│   │   ├── /Payments
│   │   ├── /Telegram
│   │   ├── /Users
│   │   └── /Orchestrators
│   ├── /AirdropArchitect.Core           # Shared business logic
│   │   ├── /Services
│   │   ├── /Models
│   │   └── /Interfaces
│   ├── /AirdropArchitect.Infrastructure # External integrations
│   │   ├── /Alchemy
│   │   ├── /Helius
│   │   ├── /Stripe
│   │   ├── /Coinbase
│   │   └── /Telegram
│   └── /airdrop-architect-web           # React frontend
│       ├── /src
│       │   ├── /components
│       │   ├── /pages
│       │   ├── /hooks
│       │   ├── /services
│       │   └── /types
│       └── package.json
├── /tests
│   ├── /AirdropArchitect.Functions.Tests
│   └── /AirdropArchitect.Core.Tests
├── /infrastructure                       # Bicep/ARM templates
│   └── main.bicep
├── /docs
│   ├── api-reference.md
│   └── deployment.md
├── .env.example
├── .gitignore
├── README.md
└── AirdropArchitect.sln
```

---

## Internationalization (i18n) Guidelines

**IMPORTANT:** All code must be written with future localization in mind. MVP is English-only, but the architecture must support adding languages without major refactoring.

### Core Principles

1. **Never hardcode user-facing strings in code**
   - Bad: `await SendMessageAsync("Welcome to Airdrop Architect!")`
   - Good: `await SendMessageAsync(_localizer["Welcome"])`

2. **Use locale files for all text**
   - Store strings in `/src/AirdropArchitect.Core/Locales/` directory
   - JSON format: `en.json`, `es.json`, etc.
   - Organize by feature: `telegram.json`, `errors.json`, `notifications.json`

3. **Telegram bot: Read and store user language**
   - Telegram provides `language_code` in user messages
   - Store in User model: `PreferredLanguage` field
   - Fall back to English if unavailable

### Locale File Structure

```
/src/AirdropArchitect.Core/Locales/
├── en/
│   ├── telegram.json       # Bot command responses
│   ├── errors.json         # Error messages
│   └── notifications.json  # Alert templates
└── (future: es/, pt/, zh/)
```

### Example Locale File (`en/telegram.json`)

```json
{
  "Welcome": "Welcome to Airdrop Architect!\n\nI help you find unclaimed airdrops and track points across your wallets.",
  "CheckingWallet": "Scanning wallet for airdrops...",
  "NoEligibleAirdrops": "No eligible airdrops found for this wallet.",
  "PointsHeader": "Points for {address}",
  "UpgradePrompt": "Use /upgrade to unlock more features!",
  "InvalidAddress": "Invalid wallet address. Please check and try again.",
  "ServiceUnavailable": "Service temporarily unavailable. Please try again later."
}
```

### Code Pattern for Localized Messages

```csharp
// ILocalizationService interface
public interface ILocalizationService
{
    string Get(string key, string? languageCode = null);
    string Get(string key, string languageCode, params object[] args);
}

// Usage in TelegramBotService
await _bot.SendTextMessageAsync(
    chatId,
    _localizer.Get("Welcome", user.PreferredLanguage),
    cancellationToken: ct);

// With string interpolation
await _bot.SendTextMessageAsync(
    chatId,
    _localizer.Get("PointsHeader", user.PreferredLanguage, ShortenAddress(address)),
    cancellationToken: ct);
```

### Geographic Restrictions

See ADR-011 in DECISIONS.md for full details.

**Blocked Countries (OFAC + Algeria):**
- Iran (IR), North Korea (KP), Syria (SY), Cuba (CU)
- Russia (RU), Belarus (BY), Venezuela (VE), Afghanistan (AF)
- Algeria (DZ) - crypto banned July 2025

**Implementation:**
- `IGeoRestrictionService.IsAllowedAsync(string countryCode)`
- Configurable blocklist in app settings (not hardcoded)
- Check at Telegram webhook entry point
- Store user country in User model for analytics

---

## Development Phases

### PHASE 1: Foundation (Weeks 1-4)
**Goal:** Basic infrastructure, auth, and Telegram bot skeleton

### PHASE 2: Core Features (Weeks 5-8)
**Goal:** Eligibility checking and points tracking working

### PHASE 3: Payments (Weeks 9-12)
**Goal:** Stripe and Coinbase Commerce integrated

### PHASE 4: Polish & Launch (Weeks 13-16)
**Goal:** React dashboard, testing, beta launch

---

## Phase 1 Detailed Tasks

### Week 1: Project Setup

#### Task 1.1: Repository Setup
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

#### Task 1.2: Core NuGet Packages
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

#### Task 1.3: Environment Configuration
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

#### Task 1.4: Azure Setup
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

### Week 2: Telegram Bot Foundation

#### Task 2.1: Create Telegram Bot
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

#### Task 2.2: Implement Telegram Bot Service
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

#### Task 2.3: Telegram Webhook Function
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

### Week 3: External Service Setup

#### Task 3.1: Alchemy API Setup
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

#### Task 3.2: Stripe Account Setup
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

#### Task 3.3: Coinbase Commerce Setup
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

### Week 4: Core Models & Database

#### Task 4.1: Create Core Models
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

---

## Sub-Agent Delegation

When appropriate, you may spawn sub-agents to parallelize work. Use this format:

```
SPAWN SUB-AGENT: [Name]
TASK: [Specific task description]
CONSTRAINTS: [Any limitations]
REPORT BACK: [What information to return]
```

**Appropriate sub-agent tasks:**
- Research specific API documentation
- Generate test data/mock responses
- Create unit tests for completed code
- Generate TypeScript types from C# models
- Research competitor implementations

**NOT appropriate for sub-agents:**
- Decisions requiring human approval
- Tasks involving credentials
- Major architectural changes

---

## Checkpoint Protocol

After completing each major task, create a checkpoint:

```
CHECKPOINT: [Task Name]
STATUS: COMPLETE | IN_PROGRESS | BLOCKED
FILES CREATED/MODIFIED: [List]
NEXT STEPS: [What comes next]
BLOCKERS: [Any issues requiring human input]
```

---

## Communication Style

When working with the human:

1. **Be proactive about dependencies:**
   - "Before I can implement X, we need Y. Would you like me to proceed with Y first?"

2. **Explain decisions:**
   - "I'm using CosmosDB's serverless tier because it's the most cost-effective for our initial user volume."

3. **Offer alternatives:**
   - "I can implement this with either approach A or B. A is simpler but B scales better. Which would you prefer?"

4. **Flag risks early:**
   - "Note: The free Alchemy tier limits us to X requests. We should monitor this and upgrade when we hit 70% capacity."

5. **Celebrate progress:**
   - "✅ Telegram bot is now responding to /start commands! Try it out: @AirdropArchitectBot"

---

## Quick Reference: External Services Needed

| Service | Purpose | Free Tier | When Needed |
|---------|---------|-----------|-------------|
| Azure | Hosting, DB, Functions | $200 credit | Week 1 |
| Alchemy | EVM blockchain data | 300M CU/mo | Week 3 |
| Helius | Solana blockchain data | 1M credits/mo | Phase 2 |
| Telegram | Bot hosting | Free | Week 2 |
| Stripe | Card payments | Free (2.9% fee) | Week 3 |
| Coinbase Commerce | Crypto payments | Free (1% fee) | Week 3 |
| GitHub | Code hosting | Free | Week 1 |

---

## Error Recovery

If you encounter errors:

1. **Build errors:** Check NuGet package versions, ensure all references are added
2. **Runtime errors:** Check connection strings in .env
3. **API errors:** Verify API keys, check rate limits
4. **Deployment errors:** Verify Azure resource configurations

If stuck, ask the human for help with:
- "I'm encountering [error]. I've tried [solutions]. Could you check [specific thing]?"

---

## Success Criteria

**Phase 1 Complete When:**
- [ ] Solution builds without errors
- [ ] Telegram bot responds to /start
- [ ] Cosmos DB containers created and accessible
- [ ] Basic user creation working
- [ ] Local development environment documented

**Phase 2 Complete When:**
- [ ] /check command returns real eligibility data
- [ ] /points command returns real points data
- [ ] At least 10 airdrops seeded in database
- [ ] At least 3 points programs trackable

**Phase 3 Complete When:**
- [ ] Stripe checkout flow working
- [ ] Coinbase Commerce checkout flow working
- [ ] Subscriptions properly update user tier
- [ ] Payment webhooks processing correctly

**Phase 4 Complete When:**
- [ ] React dashboard deployed
- [ ] Beta users onboarded
- [ ] Monitoring and alerting configured
- [ ] Documentation complete

---

*This document should be updated as the project progresses. Last updated: February 2026*
