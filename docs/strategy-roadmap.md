# Airdrop Architect: Combined Strategy & Technical Roadmap
## Merging Claude + Gemini Analysis | C#/Azure Stack | Crypto + Fiat Payments

---

## Executive Summary

**Product Vision:** Airdrop Architect is an AI-driven strategy platform that goes beyond simple eligibility checking to provide proactive "path-to-profit" guidance for crypto users seeking airdrops.

**Key Differentiators:**
1. **Proactive, not reactive** - Tells users what to do NOW to qualify for FUTURE airdrops
2. **Points Meta Tracking** - Monitors off-chain points dashboards (the new airdrop standard)
3. **Sybil Protection** - Helps users avoid being flagged as bots
4. **Azure/C# Native** - Built on your 12 years of expertise

**Financial Targets:**
- Month 12: 2,500 MAU, $3,625 MRR, ~$2,800 net profit
- Month 24: 12,000 MAU, $17,400 MRR, ~$15,000 net profit
- Exit potential: $150K-$400K (2-3x annual revenue)

---

## Part 1: Product Strategy

### 1.1 Market Positioning

| Competitor | Approach | Gap We Fill |
|------------|----------|-------------|
| Drops.bot | Reactive checker | No strategy, no points tracking |
| AirdropScan | Manual, static | No AI, no path-to-eligibility |
| Lootbot | Automated farming | High Sybil detection risk |

**Our Position:** The "Bloomberg Terminal" for airdrop hunters - intelligence, not just data.

### 1.2 Product Tiers

| Tier | Price | Features |
|------|-------|----------|
| **Free** | $0 | Check 3 wallets/month, see IF eligible (not details) |
| **Tracker** | $9/mo | Track 10 wallets, Telegram alerts, claim reminders |
| **Architect** | $29/mo | Points dashboard, path-to-eligibility, Sybil analyzer, unlimited wallets |
| **API** | $99/mo | Bulk checks, webhooks, white-label data |

**Pricing Philosophy:** 
- Free tier drives viral growth (shareable "I found $X in airdrops" moments)
- $9 tier captures casual users who want alerts
- $29 tier is where serious farmers live (highest LTV)
- $99 tier captures B2B revenue from wallets/tools

### 1.3 Core Feature Set

**MVP Features (Months 1-4):**
1. ✅ Eligibility Checker - Check wallets against known airdrops
2. ✅ Points Dashboard - Track Hyperliquid, EigenLayer, Blur, etc.
3. ✅ Telegram Bot - Primary interface for crypto users
4. ✅ Stripe + Crypto Payments - Accept USD, BTC, ETH, USDC

**Phase 2 Features (Months 5-8):**
1. 🔄 Path-to-Eligibility Engine - "Do X, Y, Z to qualify for [Airdrop]"
2. 🔄 Sybil Protection Analyzer - "Your activity looks robotic, here's how to fix it"
3. 🔄 AI Protocol Parsing - Auto-extract criteria from whitepapers/docs
4. 🔄 Transaction Jitter Recommendations - Randomization guidance

**Phase 3 Features (Months 9-12):**
1. 📊 Portfolio Analytics - ROI tracking across farming activities
2. 📊 Historical Performance - "Users who did X earned $Y on average"
3. 📊 API + Webhook System - For developers and integrations
4. 📊 White-label licensing - For wallets wanting to embed

### 1.4 Points Meta: The New Airdrop Paradigm

**Why This Matters:**

The airdrop landscape shifted in 2023-2024:
- **Old model:** Use protocol → Random surprise airdrop
- **New model:** Use protocol → Earn visible points → Points convert to tokens

**Current Active Points Programs to Track:**

| Protocol | Chain | Points Name | Status |
|----------|-------|-------------|--------|
| Hyperliquid | Arbitrum | Points | Active, huge allocation expected |
| EigenLayer | Ethereum | Restaked Points | Active |
| Blast | L2 | Blast Points + Gold | Active |
| Scroll | L2 | Marks | Active |
| LayerZero | Multi | - | TGE complete, tracking claims |
| zkSync | L2 | - | TGE complete, tracking claims |
| Linea | L2 | LXP | Active |
| Mode | L2 | Mode Points | Active |

**Technical Approach:**
- Many points dashboards have unofficial APIs or scrapeable endpoints
- Store snapshots daily to show progress over time
- Calculate estimated token value based on comparable airdrops

---

## Part 2: Technical Architecture (C#/Azure)

### 2.1 System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                      CLIENT LAYER                               │
├─────────────────┬─────────────────┬─────────────────────────────┤
│   React Web App │  Telegram Bot   │      REST API               │
│   (Dashboard)   │  (Primary UX)   │   (Developers/B2B)          │
└────────┬────────┴────────┬────────┴────────────┬────────────────┘
         │                 │                     │
         ▼                 ▼                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                    AZURE API MANAGEMENT                         │
│         (Rate Limiting, Auth, Usage Tracking)                   │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    AZURE FUNCTIONS (.NET 8)                     │
├─────────────────┬─────────────────┬─────────────────────────────┤
│ EligibilityFunc │ PointsTrackerFn │ PaymentWebhookFn            │
│ AirdropScanFn   │ AlertEngineFn   │ UserManagementFn            │
│ ChainMonitorFn  │ SybilAnalyzerFn │ TelegramBotFn               │
└────────┬────────┴────────┬────────┴────────────┬────────────────┘
         │                 │                     │
         ▼                 ▼                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                      DATA & SERVICES                            │
├─────────────────┬─────────────────┬─────────────────────────────┤
│   Cosmos DB     │   Azure Redis   │   External APIs             │
│   (NoSQL)       │   (Cache)       │   (Alchemy, Helius)         │
├─────────────────┼─────────────────┼─────────────────────────────┤
│   Azure Blob    │   Azure OpenAI  │   Payment APIs              │
│   (Static/Logs) │   (Phase 2)     │   (Stripe, Coinbase)        │
└─────────────────┴─────────────────┴─────────────────────────────┘
```

### 2.2 Why Azure + C# (Leveraging Your Expertise)

| Advantage | Explanation |
|-----------|-------------|
| **Your 12 years of C# experience** | No learning curve, ship faster |
| **Azure Functions** | Event-driven, scales to zero, cost-effective |
| **Durable Functions** | Perfect for parallel chain scanning |
| **Cosmos DB** | Flexible schema for varying airdrop rules |
| **Azure OpenAI** | Semantic Kernel for AI features in Phase 2 |
| **Familiar tooling** | Visual Studio, Azure DevOps, etc. |

### 2.3 Database Schema (Cosmos DB)

**Container: Users**
```json
{
  "id": "user-uuid",
  "partitionKey": "users",
  "telegramId": 123456789,
  "email": "user@example.com",
  "subscriptionTier": "architect",
  "subscriptionExpiresAt": "2025-12-31T00:00:00Z",
  "stripeCustomerId": "cus_xxx",
  "coinbaseChargeIds": ["charge_xxx"],
  "wallets": [
    {
      "address": "0x123...",
      "chain": "ethereum",
      "label": "Main Wallet",
      "addedAt": "2025-01-15T00:00:00Z"
    }
  ],
  "referralCode": "ABC123",
  "referredBy": "user-uuid-2",
  "createdAt": "2025-01-01T00:00:00Z"
}
```

**Container: Airdrops**
```json
{
  "id": "airdrop-uuid",
  "partitionKey": "airdrops",
  "name": "Arbitrum",
  "tokenSymbol": "ARB",
  "chain": "arbitrum",
  "status": "claimable", // upcoming, claimable, expired
  "claimContract": "0x67a24CE4321aB3aF51c2D0a4801c3E111D88C9d9",
  "claimDeadline": "2024-09-23T12:00:00Z",
  "eligibilitySource": "https://github.com/...",
  "criteria": [
    "Used Arbitrum before Feb 6, 2023",
    "Made at least 2 transactions"
  ],
  "totalEligibleAddresses": 625000,
  "averageAllocationUsd": 1500,
  "createdAt": "2024-03-01T00:00:00Z"
}
```

**Container: PointsPrograms**
```json
{
  "id": "points-uuid",
  "partitionKey": "points",
  "protocolName": "Hyperliquid",
  "pointsName": "Points",
  "chain": "arbitrum",
  "status": "active",
  "dashboardUrl": "https://app.hyperliquid.xyz/points",
  "apiEndpoint": "https://api.hyperliquid.xyz/info", // if available
  "estimatedTgeDate": "2025-Q1",
  "trackingMethod": "api", // api, scrape, manual
  "lastUpdated": "2025-01-20T00:00:00Z"
}
```

**Container: Eligibility**
```json
{
  "id": "eligibility-uuid",
  "partitionKey": "elig-{airdropId}",
  "airdropId": "airdrop-uuid",
  "walletAddress": "0x123...",
  "allocationAmount": 1250.5,
  "hasClaimed": false,
  "merkleProof": ["0xabc...", "0xdef..."],
  "checkedAt": "2025-01-20T00:00:00Z"
}
```

**Container: PointsSnapshots**
```json
{
  "id": "snapshot-uuid",
  "partitionKey": "snapshot-{walletAddress}",
  "walletAddress": "0x123...",
  "protocolId": "points-uuid",
  "points": 12500,
  "rank": 4521,
  "percentile": 85.2,
  "snapshotDate": "2025-01-20",
  "previousPoints": 11200,
  "pointsChange": 1300
}
```

### 2.4 Core Services (C# Implementation)

**EligibilityService.cs**
```csharp
using Azure.Cosmos;
using Nethereum.Web3;
using System.Collections.Generic;
using System.Threading.Tasks;

public class EligibilityService
{
    private readonly CosmosContainer _eligibilityContainer;
    private readonly CosmosContainer _airdropContainer;
    private readonly IAlchemyClient _alchemy;
    private readonly IHeliusClient _helius;
    private readonly IDistributedCache _cache;

    public EligibilityService(
        CosmosClient cosmos,
        IAlchemyClient alchemy,
        IHeliusClient helius,
        IDistributedCache cache)
    {
        _eligibilityContainer = cosmos.GetContainer("airdrop-db", "eligibility");
        _airdropContainer = cosmos.GetContainer("airdrop-db", "airdrops");
        _alchemy = alchemy;
        _helius = helius;
        _cache = cache;
    }

    public async Task<List<EligibilityResult>> CheckWalletAsync(
        string address, 
        string[] chains = null)
    {
        chains ??= new[] { "ethereum", "arbitrum", "optimism", "base", "solana" };
        
        var results = new List<EligibilityResult>();
        var tasks = chains.Select(chain => CheckChainAsync(address, chain));
        
        var chainResults = await Task.WhenAll(tasks);
        
        foreach (var chainResult in chainResults)
        {
            results.AddRange(chainResult);
        }
        
        return results;
    }

    private async Task<List<EligibilityResult>> CheckChainAsync(
        string address, 
        string chain)
    {
        var results = new List<EligibilityResult>();
        
        // Get active airdrops for this chain
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.chain = @chain AND c.status IN ('claimable', 'upcoming')")
            .WithParameter("@chain", chain);
            
        var airdrops = await _airdropContainer.GetItemQueryIterator<Airdrop>(query)
            .ToListAsync();
        
        foreach (var airdrop in airdrops)
        {
            var eligibility = await CheckAirdropEligibilityAsync(address, airdrop);
            if (eligibility != null)
            {
                results.Add(eligibility);
            }
        }
        
        return results;
    }

    private async Task<EligibilityResult?> CheckAirdropEligibilityAsync(
        string address, 
        Airdrop airdrop)
    {
        // Check cache first
        var cacheKey = $"elig:{airdrop.Id}:{address}";
        var cached = await _cache.GetAsync<EligibilityResult>(cacheKey);
        if (cached != null) return cached;
        
        // Check our eligibility database
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.airdropId = @airdropId AND c.walletAddress = @address")
            .WithParameter("@airdropId", airdrop.Id)
            .WithParameter("@address", address.ToLower());
            
        var eligibilityData = await _eligibilityContainer
            .GetItemQueryIterator<EligibilityData>(query)
            .FirstOrDefaultAsync();
        
        if (eligibilityData == null) return null;
        if (eligibilityData.HasClaimed) return null;
        
        // Verify claim status on-chain
        if (!string.IsNullOrEmpty(airdrop.ClaimContract))
        {
            var hasClaimed = await CheckOnChainClaimStatusAsync(
                address, 
                airdrop.ClaimContract, 
                airdrop.Chain);
                
            if (hasClaimed)
            {
                // Update our database
                eligibilityData.HasClaimed = true;
                await _eligibilityContainer.UpsertItemAsync(eligibilityData);
                return null;
            }
        }
        
        var result = new EligibilityResult
        {
            AirdropName = airdrop.Name,
            TokenSymbol = airdrop.TokenSymbol,
            Chain = airdrop.Chain,
            AllocationAmount = eligibilityData.AllocationAmount,
            EstimatedValueUsd = await EstimateValueAsync(
                airdrop.TokenSymbol, 
                eligibilityData.AllocationAmount),
            HasClaimed = false,
            ClaimDeadline = airdrop.ClaimDeadline,
            ClaimUrl = airdrop.ClaimUrl
        };
        
        // Cache for 1 hour
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
        
        return result;
    }

    private async Task<bool> CheckOnChainClaimStatusAsync(
        string address, 
        string contract, 
        string chain)
    {
        if (chain == "solana")
        {
            return await _helius.CheckClaimStatusAsync(address, contract);
        }
        
        // EVM chains via Alchemy
        var network = chain switch
        {
            "ethereum" => Network.EthMainnet,
            "arbitrum" => Network.ArbMainnet,
            "optimism" => Network.OptMainnet,
            "base" => Network.BaseMainnet,
            _ => Network.EthMainnet
        };
        
        return await _alchemy.CheckClaimStatusAsync(address, contract, network);
    }
}
```

**PointsTrackingService.cs**
```csharp
public class PointsTrackingService
{
    private readonly CosmosContainer _pointsContainer;
    private readonly CosmosContainer _snapshotContainer;
    private readonly HttpClient _httpClient;

    public async Task<List<PointsBalance>> GetWalletPointsAsync(string address)
    {
        var results = new List<PointsBalance>();
        
        // Get all active points programs
        var programs = await GetActivePointsProgramsAsync();
        
        var tasks = programs.Select(p => FetchPointsAsync(address, p));
        var balances = await Task.WhenAll(tasks);
        
        return balances.Where(b => b != null).ToList();
    }

    private async Task<PointsBalance?> FetchPointsAsync(
        string address, 
        PointsProgram program)
    {
        try
        {
            return program.TrackingMethod switch
            {
                "api" => await FetchFromApiAsync(address, program),
                "scrape" => await FetchFromDashboardAsync(address, program),
                _ => null
            };
        }
        catch (Exception ex)
        {
            // Log error, return null
            return null;
        }
    }

    private async Task<PointsBalance?> FetchFromApiAsync(
        string address, 
        PointsProgram program)
    {
        // Example: Hyperliquid API
        if (program.ProtocolName == "Hyperliquid")
        {
            var response = await _httpClient.PostAsJsonAsync(
                program.ApiEndpoint,
                new { type = "userPoints", user = address });
                
            var data = await response.Content.ReadFromJsonAsync<HyperliquidPointsResponse>();
            
            return new PointsBalance
            {
                Protocol = program.ProtocolName,
                PointsName = program.PointsName,
                Balance = data?.Points ?? 0,
                Rank = data?.Rank,
                LastUpdated = DateTime.UtcNow
            };
        }
        
        // Add other protocol handlers...
        return null;
    }

    public async Task TakeSnapshotAsync(string address)
    {
        var balances = await GetWalletPointsAsync(address);
        
        foreach (var balance in balances)
        {
            // Get previous snapshot
            var previousQuery = new QueryDefinition(
                @"SELECT TOP 1 * FROM c 
                  WHERE c.walletAddress = @address 
                  AND c.protocolId = @protocolId 
                  ORDER BY c.snapshotDate DESC")
                .WithParameter("@address", address)
                .WithParameter("@protocolId", balance.ProtocolId);
                
            var previous = await _snapshotContainer
                .GetItemQueryIterator<PointsSnapshot>(previousQuery)
                .FirstOrDefaultAsync();
            
            var snapshot = new PointsSnapshot
            {
                Id = Guid.NewGuid().ToString(),
                WalletAddress = address,
                ProtocolId = balance.ProtocolId,
                Points = balance.Balance,
                Rank = balance.Rank,
                SnapshotDate = DateTime.UtcNow.Date,
                PreviousPoints = previous?.Points ?? 0,
                PointsChange = balance.Balance - (previous?.Points ?? 0)
            };
            
            await _snapshotContainer.CreateItemAsync(snapshot);
        }
    }
}
```

### 2.5 Azure Functions Structure

```
/AirdropArchitect.Functions
├── /Eligibility
│   ├── CheckWalletFunction.cs      // HTTP trigger: POST /api/check
│   ├── BatchCheckFunction.cs       // HTTP trigger: POST /api/check/batch
│   └── RefreshClaimStatusFunction.cs // Timer trigger: every 6 hours
│
├── /Points
│   ├── GetPointsFunction.cs        // HTTP trigger: GET /api/points/{address}
│   ├── SnapshotFunction.cs         // Timer trigger: daily at midnight
│   └── PointsAlertFunction.cs      // Queue trigger: send alerts
│
├── /Payments
│   ├── StripeWebhookFunction.cs    // HTTP trigger: POST /api/webhooks/stripe
│   ├── CoinbaseWebhookFunction.cs  // HTTP trigger: POST /api/webhooks/coinbase
│   └── CreateCheckoutFunction.cs   // HTTP trigger: POST /api/checkout
│
├── /Telegram
│   ├── TelegramWebhookFunction.cs  // HTTP trigger: POST /api/telegram
│   └── SendAlertFunction.cs        // Queue trigger: send Telegram messages
│
├── /Users
│   ├── GetUserFunction.cs          // HTTP trigger: GET /api/user
│   ├── UpdateUserFunction.cs       // HTTP trigger: PUT /api/user
│   └── ReferralFunction.cs         // HTTP trigger: POST /api/referral
│
└── /Orchestrators (Durable Functions)
    ├── ParallelChainScanOrchestrator.cs  // Scan 50+ chains in parallel
    └── DailySnapshotOrchestrator.cs      // Coordinate daily snapshots
```

---

## Part 3: Payment Integration

### 3.1 Dual Payment Strategy

**Why Both Fiat + Crypto:**
- Crypto users expect to pay in crypto (it's your target market)
- Fiat (Stripe) catches users who don't want crypto complexity
- Lower fees with crypto (1% vs 2.9% + $0.30)
- Global reach - crypto works everywhere

### 3.2 Payment Options Comparison

| Provider | Currencies | Fees | Setup Complexity |
|----------|------------|------|------------------|
| **Stripe** | USD, EUR, etc. | 2.9% + $0.30 | Easy |
| **Coinbase Commerce** | BTC, ETH, USDC, + more | 1% | Easy |
| **BTCPay Server** | BTC, Lightning | 0% (self-hosted) | Medium |

**Recommended Approach:**
1. **Primary:** Stripe for fiat subscriptions
2. **Primary:** Coinbase Commerce for crypto (easiest integration)
3. **Optional (Phase 2):** BTCPay Server for 0% BTC fees

### 3.3 Coinbase Commerce Integration (C#)

```csharp
// CoinbaseCommerceService.cs
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

public class CoinbaseCommerceService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _webhookSecret;
    private readonly CosmosContainer _userContainer;

    public CoinbaseCommerceService(
        IConfiguration config,
        HttpClient httpClient,
        CosmosClient cosmos)
    {
        _apiKey = config["CoinbaseCommerce:ApiKey"];
        _webhookSecret = config["CoinbaseCommerce:WebhookSecret"];
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.commerce.coinbase.com/");
        _httpClient.DefaultRequestHeaders.Add("X-CC-Api-Key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("X-CC-Version", "2018-03-22");
        _userContainer = cosmos.GetContainer("airdrop-db", "users");
    }

    public async Task<CoinbaseCharge> CreateChargeAsync(
        string userId,
        string productType, // "reveal", "tracker", "architect"
        string? walletAddress = null)
    {
        var pricing = productType switch
        {
            "reveal" => new { amount = "5.00", currency = "USD" },
            "tracker" => new { amount = "9.00", currency = "USD" },
            "architect" => new { amount = "29.00", currency = "USD" },
            "api" => new { amount = "99.00", currency = "USD" },
            _ => throw new ArgumentException("Invalid product type")
        };

        var chargeRequest = new
        {
            name = $"Airdrop Architect - {productType}",
            description = GetProductDescription(productType),
            pricing_type = "fixed_price",
            local_price = pricing,
            metadata = new
            {
                user_id = userId,
                product_type = productType,
                wallet_address = walletAddress
            },
            redirect_url = "https://airdroparchitect.com/payment/success",
            cancel_url = "https://airdroparchitect.com/payment/cancel"
        };

        var response = await _httpClient.PostAsJsonAsync("charges", chargeRequest);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<CoinbaseChargeResponse>();
        return result.Data;
    }

    public async Task<bool> ValidateWebhookAsync(
        string payload, 
        string signature)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = Convert.ToHexString(computedHash).ToLower();
        
        return signature == computedSignature;
    }

    public async Task HandleWebhookAsync(CoinbaseWebhookEvent webhookEvent)
    {
        var eventType = webhookEvent.Event.Type;
        var charge = webhookEvent.Event.Data;
        var metadata = charge.Metadata;

        switch (eventType)
        {
            case "charge:confirmed":
                await HandlePaymentConfirmedAsync(metadata);
                break;
            case "charge:failed":
                // Log failure, potentially notify user
                break;
            case "charge:pending":
                // Payment initiated, waiting for confirmations
                break;
        }
    }

    private async Task HandlePaymentConfirmedAsync(ChargeMetadata metadata)
    {
        var userId = metadata.UserId;
        var productType = metadata.ProductType;

        var user = await _userContainer.ReadItemAsync<User>(
            userId, 
            new PartitionKey("users"));

        switch (productType)
        {
            case "reveal":
                // Grant access to reveal specific wallet
                user.Resource.RevealedWallets.Add(metadata.WalletAddress);
                break;
                
            case "tracker":
                user.Resource.SubscriptionTier = "tracker";
                user.Resource.SubscriptionExpiresAt = DateTime.UtcNow.AddMonths(1);
                break;
                
            case "architect":
                user.Resource.SubscriptionTier = "architect";
                user.Resource.SubscriptionExpiresAt = DateTime.UtcNow.AddMonths(1);
                break;
                
            case "api":
                user.Resource.SubscriptionTier = "api";
                user.Resource.SubscriptionExpiresAt = DateTime.UtcNow.AddMonths(1);
                break;
        }

        await _userContainer.ReplaceItemAsync(user.Resource, userId);
        
        // Send confirmation via Telegram
        await SendTelegramNotificationAsync(user.Resource.TelegramId, productType);
    }

    private string GetProductDescription(string productType)
    {
        return productType switch
        {
            "reveal" => "One-time reveal of airdrop eligibility details for a single wallet",
            "tracker" => "1 month of Tracker tier - Track 10 wallets with Telegram alerts",
            "architect" => "1 month of Architect tier - Full points dashboard and strategy tools",
            "api" => "1 month of API access - Bulk checks and webhooks",
            _ => ""
        };
    }
}
```

**Coinbase Webhook Azure Function:**
```csharp
// CoinbaseWebhookFunction.cs
public class CoinbaseWebhookFunction
{
    private readonly CoinbaseCommerceService _coinbase;
    private readonly ILogger<CoinbaseWebhookFunction> _logger;

    public CoinbaseWebhookFunction(
        CoinbaseCommerceService coinbase,
        ILogger<CoinbaseWebhookFunction> logger)
    {
        _coinbase = coinbase;
        _logger = logger;
    }

    [Function("CoinbaseWebhook")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "webhooks/coinbase")] 
        HttpRequestData req)
    {
        var payload = await req.ReadAsStringAsync();
        var signature = req.Headers.GetValues("X-CC-Webhook-Signature").FirstOrDefault();

        if (string.IsNullOrEmpty(signature))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        // Validate webhook signature
        var isValid = await _coinbase.ValidateWebhookAsync(payload, signature);
        if (!isValid)
        {
            _logger.LogWarning("Invalid Coinbase webhook signature");
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        try
        {
            var webhookEvent = JsonSerializer.Deserialize<CoinbaseWebhookEvent>(payload);
            await _coinbase.HandleWebhookAsync(webhookEvent);
            
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Coinbase webhook");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
```

### 3.4 Stripe Integration (C#)

```csharp
// StripeService.cs
using Stripe;
using Stripe.Checkout;

public class StripeService
{
    private readonly CosmosContainer _userContainer;

    public StripeService(IConfiguration config, CosmosClient cosmos)
    {
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        _userContainer = cosmos.GetContainer("airdrop-db", "users");
    }

    public async Task<Session> CreateCheckoutSessionAsync(
        string userId,
        string priceId,  // Stripe Price ID
        string successUrl,
        string cancelUrl)
    {
        var user = await _userContainer.ReadItemAsync<User>(
            userId, 
            new PartitionKey("users"));

        var options = new SessionCreateOptions
        {
            Customer = user.Resource.StripeCustomerId,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1,
                }
            },
            Mode = "subscription",
            SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "user_id", userId }
            }
        };

        // Create customer if doesn't exist
        if (string.IsNullOrEmpty(user.Resource.StripeCustomerId))
        {
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Metadata = new Dictionary<string, string> { { "user_id", userId } }
            });
            
            user.Resource.StripeCustomerId = customer.Id;
            await _userContainer.ReplaceItemAsync(user.Resource, userId);
            
            options.Customer = customer.Id;
        }

        var sessionService = new SessionService();
        return await sessionService.CreateAsync(options);
    }

    public async Task HandleWebhookAsync(Event stripeEvent)
    {
        switch (stripeEvent.Type)
        {
            case Events.CheckoutSessionCompleted:
                var session = stripeEvent.Data.Object as Session;
                await HandleCheckoutCompleteAsync(session);
                break;
                
            case Events.CustomerSubscriptionUpdated:
                var subscription = stripeEvent.Data.Object as Subscription;
                await HandleSubscriptionUpdateAsync(subscription);
                break;
                
            case Events.CustomerSubscriptionDeleted:
                var cancelledSub = stripeEvent.Data.Object as Subscription;
                await HandleSubscriptionCancelledAsync(cancelledSub);
                break;
        }
    }

    private async Task HandleCheckoutCompleteAsync(Session session)
    {
        var userId = session.Metadata["user_id"];
        var subscriptionId = session.SubscriptionId;

        // Get subscription details to determine tier
        var subService = new SubscriptionService();
        var subscription = await subService.GetAsync(subscriptionId);
        var priceId = subscription.Items.Data[0].Price.Id;

        var tier = GetTierFromPriceId(priceId);

        var user = await _userContainer.ReadItemAsync<User>(
            userId, 
            new PartitionKey("users"));
            
        user.Resource.SubscriptionTier = tier;
        user.Resource.StripeSubscriptionId = subscriptionId;
        user.Resource.SubscriptionExpiresAt = subscription.CurrentPeriodEnd;
        
        await _userContainer.ReplaceItemAsync(user.Resource, userId);
    }

    private string GetTierFromPriceId(string priceId)
    {
        // Map your Stripe Price IDs to tiers
        return priceId switch
        {
            "price_tracker_monthly" => "tracker",
            "price_architect_monthly" => "architect",
            "price_api_monthly" => "api",
            _ => "free"
        };
    }
}
```

### 3.5 Unified Payment Flow

```csharp
// CreateCheckoutFunction.cs
[Function("CreateCheckout")]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "checkout")] 
    HttpRequestData req)
{
    var request = await req.ReadFromJsonAsync<CheckoutRequest>();
    
    // Validate user
    var userId = GetUserIdFromToken(req);
    if (string.IsNullOrEmpty(userId))
    {
        return req.CreateResponse(HttpStatusCode.Unauthorized);
    }

    string checkoutUrl;

    if (request.PaymentMethod == "crypto")
    {
        // Coinbase Commerce
        var charge = await _coinbase.CreateChargeAsync(
            userId,
            request.ProductType,
            request.WalletAddress);
            
        checkoutUrl = charge.HostedUrl;
    }
    else
    {
        // Stripe
        var priceId = GetStripePriceId(request.ProductType);
        var session = await _stripe.CreateCheckoutSessionAsync(
            userId,
            priceId,
            request.SuccessUrl,
            request.CancelUrl);
            
        checkoutUrl = session.Url;
    }

    var response = req.CreateResponse(HttpStatusCode.OK);
    await response.WriteAsJsonAsync(new { checkoutUrl });
    return response;
}

public class CheckoutRequest
{
    public string ProductType { get; set; }  // reveal, tracker, architect, api
    public string PaymentMethod { get; set; } // crypto, card
    public string? WalletAddress { get; set; } // For reveals
    public string SuccessUrl { get; set; }
    public string CancelUrl { get; set; }
}
```

---

## Part 4: React Frontend

### 4.1 Tech Stack

```
/airdrop-architect-web
├── /src
│   ├── /components
│   │   ├── /dashboard
│   │   │   ├── WalletCard.tsx
│   │   │   ├── PointsChart.tsx
│   │   │   ├── EligibilityList.tsx
│   │   │   └── SybilScore.tsx
│   │   ├── /payment
│   │   │   ├── PaymentModal.tsx
│   │   │   ├── CryptoPayment.tsx
│   │   │   └── SubscriptionStatus.tsx
│   │   └── /common
│   │       ├── WalletInput.tsx
│   │       └── ChainSelector.tsx
│   ├── /pages
│   │   ├── Home.tsx
│   │   ├── Dashboard.tsx
│   │   ├── Check.tsx
│   │   └── Pricing.tsx
│   ├── /hooks
│   │   ├── useWalletCheck.ts
│   │   ├── usePoints.ts
│   │   └── useAuth.ts
│   ├── /services
│   │   ├── api.ts
│   │   └── telegram.ts
│   └── /types
│       └── index.ts
├── package.json
└── tailwind.config.js
```

**Key Dependencies:**
```json
{
  "dependencies": {
    "react": "^18.2.0",
    "react-router-dom": "^6.x",
    "tailwindcss": "^3.x",
    "@tanstack/react-query": "^5.x",
    "recharts": "^2.x",
    "framer-motion": "^10.x",
    "@stripe/stripe-js": "^2.x"
  }
}
```

### 4.2 Key Components

**PaymentModal.tsx** (Supports both Stripe and Crypto)
```tsx
import { useState } from 'react';
import { loadStripe } from '@stripe/stripe-js';

interface PaymentModalProps {
  productType: 'reveal' | 'tracker' | 'architect' | 'api';
  walletAddress?: string;
  onClose: () => void;
}

export function PaymentModal({ productType, walletAddress, onClose }: PaymentModalProps) {
  const [paymentMethod, setPaymentMethod] = useState<'card' | 'crypto'>('card');
  const [loading, setLoading] = useState(false);

  const prices = {
    reveal: { amount: 5, label: 'One-time Reveal' },
    tracker: { amount: 9, label: 'Tracker Monthly' },
    architect: { amount: 29, label: 'Architect Monthly' },
    api: { amount: 99, label: 'API Monthly' },
  };

  const handleCheckout = async () => {
    setLoading(true);
    
    try {
      const response = await fetch('/api/checkout', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          productType,
          paymentMethod,
          walletAddress,
          successUrl: `${window.location.origin}/payment/success`,
          cancelUrl: `${window.location.origin}/payment/cancel`,
        }),
      });

      const { checkoutUrl } = await response.json();
      window.location.href = checkoutUrl;
    } catch (error) {
      console.error('Checkout error:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-gray-900 rounded-2xl p-6 max-w-md w-full mx-4">
        <h2 className="text-2xl font-bold text-white mb-4">
          {prices[productType].label}
        </h2>
        
        <div className="text-4xl font-bold text-green-400 mb-6">
          ${prices[productType].amount}
          {productType !== 'reveal' && <span className="text-lg text-gray-400">/mo</span>}
        </div>

        {/* Payment Method Selection */}
        <div className="mb-6">
          <label className="text-gray-400 text-sm mb-2 block">Payment Method</label>
          <div className="grid grid-cols-2 gap-3">
            <button
              onClick={() => setPaymentMethod('card')}
              className={`p-4 rounded-xl border-2 transition ${
                paymentMethod === 'card'
                  ? 'border-blue-500 bg-blue-500/10'
                  : 'border-gray-700 hover:border-gray-600'
              }`}
            >
              <span className="text-2xl mb-2 block">💳</span>
              <span className="text-white font-medium">Card</span>
              <span className="text-gray-500 text-xs block">via Stripe</span>
            </button>
            
            <button
              onClick={() => setPaymentMethod('crypto')}
              className={`p-4 rounded-xl border-2 transition ${
                paymentMethod === 'crypto'
                  ? 'border-orange-500 bg-orange-500/10'
                  : 'border-gray-700 hover:border-gray-600'
              }`}
            >
              <span className="text-2xl mb-2 block">₿</span>
              <span className="text-white font-medium">Crypto</span>
              <span className="text-gray-500 text-xs block">BTC, ETH, USDC</span>
            </button>
          </div>
        </div>

        {/* Crypto Payment Info */}
        {paymentMethod === 'crypto' && (
          <div className="bg-gray-800 rounded-lg p-3 mb-4 text-sm text-gray-400">
            <p>✓ 1% fee (vs 2.9% for cards)</p>
            <p>✓ Pay with BTC, ETH, USDC, and more</p>
            <p>✓ Powered by Coinbase Commerce</p>
          </div>
        )}

        {/* Actions */}
        <div className="flex gap-3">
          <button
            onClick={onClose}
            className="flex-1 py-3 rounded-xl border border-gray-700 text-gray-400 hover:bg-gray-800"
          >
            Cancel
          </button>
          <button
            onClick={handleCheckout}
            disabled={loading}
            className="flex-1 py-3 rounded-xl bg-gradient-to-r from-blue-500 to-purple-500 text-white font-medium hover:opacity-90 disabled:opacity-50"
          >
            {loading ? 'Loading...' : 'Continue'}
          </button>
        </div>
      </div>
    </div>
  );
}
```

---

## Part 5: Telegram Bot

### 5.1 Bot Commands

| Command | Description |
|---------|-------------|
| `/start` | Welcome message, create account |
| `/check <address>` | Check wallet for airdrops |
| `/points <address>` | Check points balances |
| `/track <address>` | Add wallet to tracking |
| `/untrack <address>` | Remove wallet from tracking |
| `/status` | Show subscription status |
| `/upgrade` | Show pricing, upgrade options |
| `/alerts` | Manage alert preferences |

### 5.2 Telegram Bot Implementation (C#)

```csharp
// TelegramBotService.cs
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public class TelegramBotService
{
    private readonly ITelegramBotClient _bot;
    private readonly EligibilityService _eligibility;
    private readonly PointsTrackingService _points;
    private readonly CosmosContainer _userContainer;

    public TelegramBotService(
        IConfiguration config,
        EligibilityService eligibility,
        PointsTrackingService points,
        CosmosClient cosmos)
    {
        _bot = new TelegramBotClient(config["Telegram:BotToken"]);
        _eligibility = eligibility;
        _points = points;
        _userContainer = cosmos.GetContainer("airdrop-db", "users");
    }

    public async Task HandleUpdateAsync(Update update)
    {
        if (update.Message is not { Text: { } text } message)
            return;

        var chatId = message.Chat.Id;
        var userId = message.From?.Id.ToString();

        // Ensure user exists
        await EnsureUserExistsAsync(userId, chatId);

        // Parse command
        var command = text.Split(' ')[0].ToLower();
        var args = text.Split(' ').Skip(1).ToArray();

        switch (command)
        {
            case "/start":
                await HandleStartAsync(chatId);
                break;
            case "/check":
                await HandleCheckAsync(chatId, userId, args);
                break;
            case "/points":
                await HandlePointsAsync(chatId, userId, args);
                break;
            case "/track":
                await HandleTrackAsync(chatId, userId, args);
                break;
            case "/upgrade":
                await HandleUpgradeAsync(chatId, userId);
                break;
            default:
                // Check if it's a wallet address
                if (IsValidAddress(text))
                {
                    await HandleCheckAsync(chatId, userId, new[] { text });
                }
                break;
        }
    }

    private async Task HandleStartAsync(long chatId)
    {
        var welcomeText = @"
🏗️ *Welcome to Airdrop Architect!*

I help you find and track crypto airdrops across all your wallets.

*Commands:*
/check `<address>` - Check wallet for airdrops
/points `<address>` - Check points balances
/track `<address>` - Track a wallet for alerts
/upgrade - View premium features

Or just paste any wallet address to check it instantly!
";

        await _bot.SendTextMessageAsync(
            chatId,
            welcomeText,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
    }

    private async Task HandleCheckAsync(long chatId, string userId, string[] args)
    {
        if (args.Length == 0)
        {
            await _bot.SendTextMessageAsync(chatId, 
                "Please provide a wallet address:\n`/check 0x123...abc`",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            return;
        }

        var address = args[0];
        
        if (!IsValidAddress(address))
        {
            await _bot.SendTextMessageAsync(chatId,
                "❌ Invalid wallet address. Please check and try again.");
            return;
        }

        // Send "checking" message
        var checkingMsg = await _bot.SendTextMessageAsync(chatId,
            "🔍 Scanning wallet for airdrops...");

        // Check eligibility
        var results = await _eligibility.CheckWalletAsync(address);

        if (results.Count == 0)
        {
            await _bot.EditMessageTextAsync(chatId, checkingMsg.MessageId,
                $"No unclaimed airdrops found for:\n`{ShortenAddress(address)}`\n\n" +
                "Track this wallet with /track to get alerts for new airdrops!",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            return;
        }

        // Check user subscription
        var user = await GetUserAsync(userId);
        var isPremium = user?.SubscriptionTier != "free";

        string response;
        InlineKeyboardMarkup? keyboard = null;

        if (isPremium)
        {
            response = FormatFullResults(address, results);
        }
        else
        {
            response = FormatTeaserResults(address, results);
            keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        "🔓 Reveal Details ($5)", 
                        $"reveal:{address}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        "⭐ Upgrade to Premium", 
                        "upgrade")
                }
            });
        }

        await _bot.EditMessageTextAsync(
            chatId, 
            checkingMsg.MessageId,
            response,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
            replyMarkup: keyboard);
    }

    private async Task HandlePointsAsync(long chatId, string userId, string[] args)
    {
        if (args.Length == 0)
        {
            await _bot.SendTextMessageAsync(chatId,
                "Please provide a wallet address:\n`/points 0x123...abc`",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            return;
        }

        var address = args[0];
        
        var checkingMsg = await _bot.SendTextMessageAsync(chatId,
            "📊 Fetching points balances...");

        var balances = await _points.GetWalletPointsAsync(address);

        if (balances.Count == 0)
        {
            await _bot.EditMessageTextAsync(chatId, checkingMsg.MessageId,
                $"No points found for `{ShortenAddress(address)}`\n\n" +
                "Start farming points by interacting with protocols like Hyperliquid, EigenLayer, Blast, etc.",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            return;
        }

        var response = FormatPointsResults(address, balances);
        await _bot.EditMessageTextAsync(
            chatId,
            checkingMsg.MessageId,
            response,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
    }

    private async Task HandleUpgradeAsync(long chatId, string userId)
    {
        var pricingText = @"
⭐ *Airdrop Architect Premium*

*Tracker - $9/month*
• Track up to 10 wallets
• Telegram alerts for new eligibility
• Claim deadline reminders

*Architect - $29/month*
• Unlimited wallet tracking
• Points dashboard across all protocols
• Path-to-eligibility recommendations
• Sybil protection analyzer
• Priority support

*API - $99/month*
• Everything in Architect
• REST API access
• Bulk eligibility checks
• Webhooks for integrations
";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("💳 Pay with Card", "pay:card"),
                InlineKeyboardButton.WithCallbackData("₿ Pay with Crypto", "pay:crypto")
            }
        });

        await _bot.SendTextMessageAsync(
            chatId,
            pricingText,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
            replyMarkup: keyboard);
    }

    private string FormatTeaserResults(string address, List<EligibilityResult> results)
    {
        var totalValue = results.Sum(r => r.EstimatedValueUsd);
        var chains = string.Join(", ", results.Select(r => r.Chain).Distinct());

        return $@"
🎉 *Airdrops Found!*

Wallet: `{ShortenAddress(address)}`

*{results.Count} unclaimed airdrop(s) detected!*
Estimated value: *${totalValue:N2}*

Chains: {chains}

🔓 Reveal full details to see:
• Token names and amounts
• Claim deadlines
• Direct claim links
";
    }

    private string FormatFullResults(string address, List<EligibilityResult> results)
    {
        var lines = new List<string>
        {
            $"🎉 *Airdrops for* `{ShortenAddress(address)}`\n"
        };

        foreach (var r in results)
        {
            var deadline = r.ClaimDeadline?.ToString("MMM dd, yyyy") ?? "No deadline";
            lines.Add(
                $"• *{r.AirdropName}* ({r.TokenSymbol})\n" +
                $"  Amount: {r.AllocationAmount:N2} (~${r.EstimatedValueUsd:N2})\n" +
                $"  Chain: {r.Chain}\n" +
                $"  Deadline: {deadline}\n");
        }

        var total = results.Sum(r => r.EstimatedValueUsd);
        lines.Add($"\n*Total Estimated Value: ${total:N2}*");

        return string.Join("\n", lines);
    }

    private string FormatPointsResults(string address, List<PointsBalance> balances)
    {
        var lines = new List<string>
        {
            $"📊 *Points for* `{ShortenAddress(address)}`\n"
        };

        foreach (var b in balances)
        {
            var rankStr = b.Rank.HasValue ? $" (Rank #{b.Rank:N0})" : "";
            lines.Add($"• *{b.Protocol}*: {b.Balance:N0} {b.PointsName}{rankStr}");
        }

        return string.Join("\n", lines);
    }

    private string ShortenAddress(string address)
    {
        if (address.Length < 12) return address;
        return $"{address[..6]}...{address[^4..]}";
    }

    private bool IsValidAddress(string address)
    {
        // Ethereum-style
        if (address.StartsWith("0x") && address.Length == 42)
            return true;
        // Solana (base58, 32-44 chars)
        if (address.Length >= 32 && address.Length <= 44)
            return true;
        return false;
    }
}
```

---

## Part 6: Development Timeline & Costs

### 6.1 Phase 1: MVP Development (Months 1-4)

| Month | Focus | Deliverables |
|-------|-------|--------------|
| 1 | Infrastructure | Azure setup, Cosmos DB, basic API, auth |
| 2 | Core Checker | Eligibility service, Alchemy/Helius integration |
| 3 | Telegram Bot | Full bot functionality, Stripe/Coinbase payments |
| 4 | Points + Polish | Points tracking, React dashboard, testing |

**Estimated Monthly Costs - Phase 1:**
| Item | Cost |
|------|------|
| Azure Functions (Consumption) | $5-15 |
| Cosmos DB (Free tier → Serverless) | $0-25 |
| Azure Redis (Basic) | $15 |
| Alchemy (Free tier) | $0 |
| Helius (Free tier) | $0 |
| Domain + Cloudflare | $15 |
| **Total** | **$40-70** |

### 6.2 Phase 2: AI Integration (Months 5-8)

| Month | Focus | Deliverables |
|-------|-------|--------------|
| 5 | Path Engine | "How to qualify" recommendations |
| 6 | Sybil Analyzer | Transaction pattern analysis |
| 7 | AI Parsing | Azure OpenAI for protocol docs |
| 8 | Beta Launch | Public launch, initial users |

**Estimated Monthly Costs - Phase 2:**
| Item | Cost |
|------|------|
| Azure (Scaled up) | $30-50 |
| Azure OpenAI | $20-50 |
| Alchemy (Growth usage) | $0-49 |
| Helius (Developer tier) | $49 |
| Marketing (initial) | $0-50 |
| **Total** | **$100-250** |

### 6.3 Phase 3: Launch & Growth (Months 9-12)

| Month | Focus | Deliverables |
|-------|-------|--------------|
| 9 | Marketing Push | Content, Twitter, Discord outreach |
| 10 | Referrals | Referral program, affiliate system |
| 11 | API Launch | Developer API, documentation |
| 12 | Optimization | Performance, cost optimization |

**Estimated Monthly Costs - Phase 3:**
| Item | Cost |
|------|------|
| Azure (Production) | $75-150 |
| API Providers | $100-200 |
| Marketing/Ads | $200-500 |
| Tools (analytics, etc.) | $50-100 |
| **Total** | **$400-950** |

### 6.4 Two-Year Summary

| Period | Avg Monthly Cost | Cumulative |
|--------|------------------|------------|
| Months 1-4 | $55 | $220 |
| Months 5-8 | $175 | $920 |
| Months 9-12 | $675 | $3,620 |
| Year 2 | $1,200 | $18,020 |

**One-Time Costs (Optional):**
- WA LLC formation: ~$200
- Legal/TOS consultation: ~$500-1,000
- Logo/Branding: ~$200-500

---

## Part 7: Revenue Projections

### 7.1 Conservative Model (2% Paid Conversion)

| Month | MAU | Paid Users | MRR | Expenses | Net |
|-------|-----|------------|-----|----------|-----|
| 8 | 500 | 0 | $0 | $200 | -$200 |
| 10 | 1,500 | 30 | $600 | $500 | +$100 |
| 12 | 3,000 | 60 | $1,200 | $700 | +$500 |
| 18 | 8,000 | 160 | $3,200 | $1,000 | +$2,200 |
| 24 | 15,000 | 300 | $6,000 | $1,500 | +$4,500 |

**Year 2 Annual Net: ~$50,000**

### 7.2 Optimistic Model (5% Paid Conversion)

| Month | MAU | Paid Users | MRR | Expenses | Net |
|-------|-----|------------|-----|----------|-----|
| 8 | 500 | 0 | $0 | $200 | -$200 |
| 10 | 1,500 | 75 | $1,500 | $500 | +$1,000 |
| 12 | 3,000 | 150 | $3,000 | $700 | +$2,300 |
| 18 | 8,000 | 400 | $8,000 | $1,000 | +$7,000 |
| 24 | 15,000 | 750 | $15,000 | $1,500 | +$13,500 |

**Year 2 Annual Net: ~$120,000**

### 7.3 Revenue Mix Assumptions

| Tier | % of Paid | ARPU | Revenue Share |
|------|-----------|------|---------------|
| Reveals ($5) | 40% | $5 | 10% |
| Tracker ($9) | 35% | $9 | 18% |
| Architect ($29) | 20% | $29 | 42% |
| API ($99) | 5% | $99 | 30% |

**Blended ARPU: ~$20/paid user**

---

## Part 8: Critical Success Factors

### 8.1 From Gemini Analysis

1. **Sybil Protection** 
   - Implement "Transaction Jitter" recommendations
   - Analyze user patterns, flag robotic behavior
   - This is a major differentiator

2. **Points Aggregation**
   - Move beyond on-chain to off-chain points
   - This is the current meta
   - First-mover advantage possible

3. **Non-Custodial Focus**
   - Stay as dashboard/strategy tool
   - Never hold user funds
   - Avoids regulatory complexity

### 8.2 Key Moats

1. **Data Quality** - Your curated airdrop database
2. **Points API Access** - Relationships with protocols
3. **AI Intelligence** - Path-to-eligibility recommendations
4. **User Trust** - Non-custodial, privacy-focused

### 8.3 Risk Factors

| Risk | Mitigation |
|------|------------|
| Airdrops decline | Diversify into points, DeFi analytics |
| Competition | Focus on UX, speed, AI features |
| Regulatory changes | Non-custodial, no financial advice |
| API cost spikes | Caching, optimize queries, multiple providers |

---

## Part 9: Next Steps

### Immediate Actions (This Week)

1. ✅ Set up Azure account and initial resources
2. ✅ Create GitHub repository
3. ✅ Set up local development environment
4. ✅ Get Alchemy API key (free tier)
5. ✅ Create Telegram bot via BotFather
6. ✅ Create Stripe account
7. ✅ Create Coinbase Commerce account

### Week 1-2 Development

1. Basic Azure Functions project structure
2. Cosmos DB containers and models
3. User authentication flow
4. Telegram bot skeleton with /start command

### Week 3-4 Development

1. Eligibility checking service
2. Alchemy integration for EVM chains
3. First 10 airdrops seeded in database
4. /check command working

### Month 2 Goals

1. Points tracking for top 5 protocols
2. Stripe + Coinbase Commerce payments
3. React dashboard MVP
4. Full Telegram bot functionality

---

## Appendix: Resources

### APIs
- Alchemy: https://docs.alchemy.com
- Helius: https://docs.helius.dev
- Coinbase Commerce: https://docs.cdp.coinbase.com/commerce
- Stripe: https://stripe.com/docs
- Telegram Bot: https://core.telegram.org/bots/api

### Azure
- Azure Functions (.NET): https://learn.microsoft.com/en-us/azure/azure-functions/
- Cosmos DB: https://learn.microsoft.com/en-us/azure/cosmos-db/
- Durable Functions: https://learn.microsoft.com/en-us/azure/azure-functions/durable/

### Open Source References
- BTCPay Server (C#): https://github.com/btcpayserver/btcpayserver
- Uniswap Merkle Distributor: https://github.com/Uniswap/merkle-distributor

### Points Dashboards to Track
- Hyperliquid: https://app.hyperliquid.xyz/points
- EigenLayer: https://app.eigenlayer.xyz/
- Blast: https://blast.io/en/airdrop
- Scroll: https://scroll.io/sessions
- Linea: https://linea.build/

---

*Document Version: 2.0*  
*Last Updated: February 2026*  
*Combined Analysis: Claude + Gemini*
