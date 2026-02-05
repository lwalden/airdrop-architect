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
5. 🔄 Transactional Email System - Payment receipts, welcome emails, subscription notifications

**Phase 3 Features (Months 9-12):**
1. 📊 Portfolio Analytics - ROI tracking across farming activities
2. 📊 Historical Performance - "Users who did X earned $Y on average"
3. 📊 API + Webhook System - For developers and integrations
4. 📊 White-label licensing - For wallets wanting to embed

**Phase 4 Features (Months 13+):**
1. 🌍 Localization - Spanish, Portuguese, Chinese translations
2. 🌍 Regional Expansion - Evaluate lifting geo-restrictions as regulations evolve
3. 🌍 Multi-language ToS/Privacy Policy

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
  "tenantId": null,
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
using Polly;
using Polly.CircuitBreaker;
using Microsoft.Extensions.Logging;

namespace AirdropArchitect.Core.Services;

public class EligibilityService
{
    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<EligibilityService> _logger;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly IAsyncPolicy _circuitBreakerPolicy;

    public EligibilityService(
        CosmosClient cosmosClient,
        ILogger<EligibilityService> logger)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
        
        // Polly retry policy for transient failures
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Retry {RetryCount} after {Delay}s",
                        retryCount,
                        timeSpan.TotalSeconds);
                });
        
        // Circuit breaker for sustained failures
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (ex, duration) =>
                {
                    _logger.LogError(ex, "Circuit breaker opened for {Duration}", duration);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset");
                });
    }

    public async Task<EligibilityResult> CheckEligibility(
        string walletAddress, 
        string airdropId)
    {
        // Implementation with Polly policies wrapping external calls
        return await Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy)
            .ExecuteAsync(async () =>
            {
                // Check cached eligibility first
                var cached = await GetCachedEligibility(walletAddress, airdropId);
                if (cached != null) return cached;
                
                // Fetch from blockchain/external source
                var result = await FetchEligibilityFromSource(walletAddress, airdropId);
                
                // Cache the result
                await CacheEligibility(result);
                
                return result;
            });
    }
    
    // ... rest of implementation
}
```

**PointsTracker.cs** (See full implementation in strategy-roadmap.md appendix)

**TelegramBotService.cs** (See full implementation in strategy-roadmap.md appendix)

---

## Part 3: Risk Management

### 3.1 Operational Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| RPC provider downtime | Medium | High | Multi-provider fallback (Alchemy + public RPCs), Polly circuit breakers |
| Airdrop criteria changes | High | Medium | Weekly human verification of AI-parsed rules, user feedback loop |
| Sybil detection evolution | High | High | Monitor protocol announcements, abstract eligibility rules for quick updates |
| Cosmos DB corruption | Low | Critical | Continuous backup + weekly blob export, documented restore procedure |
| API key compromise | Low | High | Key Vault with 90-day rotation, least-privilege access, secret scanning in CI |

### 3.2 Regulatory Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| "Financial advice" liability | Medium | Critical | ToS disclaimers, no personalized recommendations, educational framing only |
| GDPR/CCPA violations | Low | High | Privacy Policy, data minimization, user deletion workflow, DPA for B2B |
| MSB classification (Success Fees) | Low | High | Legal consult before launch, consider removing Success Fees if problematic |
| Crypto regulatory changes | Medium | Medium | Non-custodial design, no token handling, geographic restrictions if needed |
| Wallet data as PII | Medium | Medium | Treat wallet addresses as pseudonymous PII, document in privacy policy |

### 3.3 Competitive Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Protocol-native tracking tools | High | Medium | Focus on multi-protocol aggregation value, not single-protocol depth |
| Larger competitor entry | Medium | High | Speed to market, community building, niche focus on power users |
| Free alternatives | High | Low | Differentiate on accuracy, UX, and premium features |
| Data scraping of our criteria DB | Medium | Medium | Rate limiting, watermarking data, legal ToS restrictions |

### 3.4 Geographic Scope and Compliance

**MVP Target Markets:** US, EU, UK, Canada, Australia, Singapore (English-only)

**OFAC-Blocked Countries (Mandatory):**
| Country | Sanctions Type |
|---------|---------------|
| Iran | Comprehensive |
| North Korea | Comprehensive |
| Syria | Comprehensive |
| Cuba | Comprehensive |
| Russia | Significant restrictions |
| Belarus | Significant restrictions |
| Venezuela | Significant restrictions |
| Afghanistan | Partial |
| Algeria | Crypto banned (July 2025) |

**Implementation:**
- IP-based geo-detection at API gateway level
- ToS self-declaration ("I am not located in a restricted jurisdiction")
- Store user country code for compliance audit trail
- Configurable blocklist (not hardcoded) for easy updates

**Why Not US-Only:**
- Crypto is inherently global; US-only limits growth unnecessarily
- EU MiCA regulation explicitly exempts information services
- Our service is non-custodial (no token handling) = lower regulatory risk
- English covers vast majority of crypto users worldwide

**Internationalization Strategy:**
- MVP: English-only, but code architected for i18n from Day 1
- All user-facing strings externalized to locale files
- Phase 4+: Add translations based on user demand (Spanish, Portuguese, Chinese priority)
- Legal docs require translation when expanding to non-English markets

### 3.5 Technical Debt Checkpoints

- **End of Phase 1:** Review and refactor any shortcuts taken during rapid development
- **End of Phase 2:** Load test eligibility checking at 10x expected volume, security review
- **End of Phase 3:** Security audit before B2B API launch, penetration testing
- **Quarterly:** Dependency vulnerability scan, Polly policy tuning based on error rates

---

## Part 4: Development Timeline & Costs

### 4.1 Phase 1: MVP Development (Months 1-4)

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

### 4.2 Phase 2: AI Integration (Months 5-8)

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

### 4.3 Phase 3: Launch & Growth (Months 9-12)

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

### 4.4 Two-Year Summary

| Period | Avg Monthly Cost | Cumulative |
|--------|------------------|------------|
| Months 1-4 | $55 | $220 |
| Months 5-8 | $175 | $920 |
| Months 9-12 | $675 | $3,620 |
| Year 2 | $1,200 | $18,020 |

**One-Time Costs (Optional but Recommended):**
- WA LLC formation: ~$200
- Legal/TOS consultation: ~$500-1,000
- Logo/Branding: ~$200-500
- Security audit (Phase 3): ~$2,000-5,000

---

## Part 5: Revenue Projections

### 5.1 Conservative Model (2% Paid Conversion)

| Month | MAU | Paid Users | MRR | Expenses | Net |
|-------|-----|------------|-----|----------|-----|
| 8 | 500 | 0 | $0 | $200 | -$200 |
| 10 | 1,500 | 30 | $600 | $500 | +$100 |
| 12 | 3,000 | 60 | $1,200 | $700 | +$500 |
| 18 | 8,000 | 160 | $3,200 | $1,000 | +$2,200 |
| 24 | 15,000 | 300 | $6,000 | $1,500 | +$4,500 |

**Year 2 Annual Net: ~$50,000**

### 5.2 Optimistic Model (5% Paid Conversion)

| Month | MAU | Paid Users | MRR | Expenses | Net |
|-------|-----|------------|-----|----------|-----|
| 8 | 500 | 0 | $0 | $200 | -$200 |
| 10 | 1,500 | 75 | $1,500 | $500 | +$1,000 |
| 12 | 3,000 | 150 | $3,000 | $700 | +$2,300 |
| 18 | 8,000 | 400 | $8,000 | $1,000 | +$7,000 |
| 24 | 15,000 | 750 | $15,000 | $1,500 | +$13,500 |

**Year 2 Annual Net: ~$120,000**

### 5.3 Revenue Mix Assumptions

| Tier | % of Paid | ARPU | Revenue Share |
|------|-----------|------|---------------|
| Reveals ($5) | 40% | $5 | 10% |
| Tracker ($9) | 35% | $9 | 18% |
| Architect ($29) | 20% | $29 | 42% |
| API ($99) | 5% | $99 | 30% |

**Blended ARPU: ~$20/paid user**

---

## Part 6: Critical Success Factors

### 6.1 From Gemini Analysis

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

### 6.2 Key Moats

1. **Data Quality** - Your curated airdrop database
2. **Points API Access** - Relationships with protocols
3. **AI Intelligence** - Path-to-eligibility recommendations
4. **User Trust** - Non-custodial, privacy-focused

### 6.3 Risk Factors

| Risk | Mitigation |
|------|------------|
| Airdrops decline | Diversify into points, DeFi analytics |
| Competition | Focus on UX, speed, AI features |
| Regulatory changes | Non-custodial, no financial advice |
| API cost spikes | Caching, optimize queries, multiple providers |

---

## Part 7: Next Steps

### Immediate Actions (This Week)

1. ✅ Set up Azure account and initial resources
2. ✅ Create GitHub repository
3. ✅ Set up local development environment
4. ✅ Get Alchemy API key (free tier)
5. ✅ Create Telegram bot via BotFather
6. ✅ Create Stripe account
7. ✅ Create Coinbase Commerce account
8. ✅ Draft Terms of Service (boilerplate created)
9. ✅ Draft Privacy Policy (boilerplate created, GDPR/CCPA compliant)
10. ✅ MiCA regulatory analysis (does NOT apply - informational service only)
11. ✅ Cookie Policy created (EU requirement for web dashboard)
12. ✅ Data Subject Rights page created (GDPR/CCPA compliance)
13. ⚠️ Replace placeholders in legal docs and attorney review
14. 🔄 Implement geo-restriction service (OFAC compliance)
15. 🔄 Set up i18n infrastructure (locale files, string externalization)
16. 🔄 Implement cookie consent banner (web dashboard)

### Week 1-2 Development

1. Basic Azure Functions project structure
2. Cosmos DB containers and models
3. User authentication flow
4. Telegram bot skeleton with /start command
5. Application Insights with distributed tracing

### Week 3-4 Development

1. Eligibility checking service
2. Alchemy integration for EVM chains
3. First 10 airdrops seeded in database
4. /check command working
5. Polly resilience patterns implemented

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
- Application Insights: https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview

### .NET Libraries
- Polly (Resilience): https://github.com/App-vNext/Polly
- Telegram.Bot: https://github.com/TelegramBots/Telegram.Bot

### Open Source References
- BTCPay Server (C#): https://github.com/btcpayserver/btcpayserver
- Uniswap Merkle Distributor: https://github.com/Uniswap/merkle-distributor

### Points Dashboards to Track
- Hyperliquid: https://app.hyperliquid.xyz/points
- EigenLayer: https://app.eigenlayer.xyz/
- Blast: https://blast.io/en/airdrop
- Scroll: https://scroll.io/sessions
- Linea: https://linea.build/

### Legal Resources
- GDPR for SaaS: https://gdpr.eu/
- CCPA Overview: https://oag.ca.gov/privacy/ccpa
- FinCEN MSB Registration: https://www.fincen.gov/msb-registrant-search

---

*Document Version: 2.1*  
*Last Updated: February 2026*  
*Combined Analysis: Claude + Gemini + Enterprise Review*
