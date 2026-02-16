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
- Month 14: 2,500 MAU, $3,625 MRR, ~$2,800 net profit
- Month 26: 12,000 MAU, $17,400 MRR, ~$15,000 net profit
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
1. Eligibility Checker - Check wallets against known airdrops
2. Points Tracking - Track Hyperliquid, EigenLayer, Blur, etc. via Telegram/API
3. Telegram Bot - Primary interface for crypto users
4. Stripe + Crypto Payments - Accept USD, BTC, ETH, USDC

**Phase 2 Features (Months 5-10):**
1. Path-to-Eligibility Engine - "Do X, Y, Z to qualify for [Airdrop]" with citations
2. Sybil Protection Analyzer - "Your activity looks robotic, here's how to fix it"
3. AI Protocol Parsing (RAG) - Extract criteria from whitepapers/docs with source links
4. Criteria Change Detection - Alert users when protocol rules or announcements shift
5. Transaction Jitter Recommendations - Randomization guidance
6. Transactional Email System - Payment receipts, welcome emails, subscription notifications

**Phase 3 Features (Months 11-14):**
1. Portfolio Analytics - ROI tracking across farming activities
2. Historical Performance - "Users who did X earned $Y on average"
3. API + Webhook System - For developers and integrations
4. White-label licensing - For wallets wanting to embed
5. RAG quality and cost optimization - Retrieval tuning, eval harness, guardrails

**Phase 4 Features (Months 15+):**
1. React Web Dashboard - Secondary UX for power users and B2B admins
2. Localization - Spanish, Portuguese, Chinese, Russian (if sanctions are lifted)
3. Regional Expansion - Evaluate lifting geo-restrictions as regulations evolve
4. Multi-language ToS/Privacy Policy

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
│   (Phase 4+)    │  (Primary UX)   │   (Developers/B2B)          │
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

**Partitioning Strategy (ADR-009):**
- User and tenant-owned records use `tenantId` (B2B) or `userId` (direct users) as the partition dimension.
- Shared reference data (airdrops/points programs) remains globally readable.
- All user-personalized outputs (eligibility, snapshots, recommendations) carry `tenantId` for logical isolation.

**Container: Users**
```json
{
  "id": "user-uuid",
  "partitionKey": "tenant-{tenantId}", // or user-{id} for direct users
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
  "partitionKey": "airdrop-{id}",
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
  "partitionKey": "protocol-{protocolName}",
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
  "partitionKey": "tenant-{tenantId}",
  "tenantId": "tenant-uuid",
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
  "partitionKey": "tenant-{tenantId}",
  "tenantId": "tenant-uuid",
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
| Crimea Region | OFAC restrictions |

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
- Phase 4+: Add translations based on user demand (Spanish, Portuguese, Chinese, Russian if sanctions are lifted)
- Legal docs require translation when expanding to non-English markets

### 3.5 Technical Debt Checkpoints

- **End of Phase 1:** Review and refactor any shortcuts taken during rapid development
- **End of Phase 2:** Load test eligibility checking at 10x expected volume, security review
- **End of Phase 3:** Security audit before B2B API launch, penetration testing
- **Quarterly:** Dependency vulnerability scan, Polly policy tuning based on error rates

---

## Part 4: Development Timeline & Costs

**Quality Gate for All Coding Work:** Every implementation task must include adding or updating unit tests in the corresponding test project before completion.

### 4.1 Phase 1: MVP Development (Months 1-4)
| Month | Focus | Deliverables |
|-------|-------|--------------|
| 1 | Infrastructure | Azure setup, Cosmos DB, basic API, auth |
| 2 | Core Checker | Eligibility service, Alchemy/Helius integration |
| 3 | Telegram Bot | Full bot functionality, Stripe/Coinbase payments |
| 4 | Points + Hardening | Points tracking, reliability testing, launch readiness |

**Estimated Monthly Costs - Phase 1:**
| Item | Cost |
|------|------|
| Azure Functions (Consumption) | $5-15 |
| Cosmos DB (Free tier -> Serverless) | $0-25 |
| Azure Redis (Basic) | $15 |
| Alchemy (Free tier) | $0 |
| Helius (Free tier) | $0 |
| Domain + Cloudflare | $15 |
| **Total** | **$40-70** |

### 4.2 Phase 2: AI + RAG Integration (Months 5-10)
| Month | Focus | Deliverables |
|-------|-------|--------------|
| 5 | RAG Foundations | Source registry, ingestion pipeline, chunking + metadata strategy |
| 6 | Protocol Parsing V1 | Criteria extraction from docs/announcements into structured schema |
| 7 | Path Engine V1 | Citation-backed path-to-eligibility responses in Telegram/API |
| 8 | Change Detection | Snapshot diffing, criteria-shift alerts, review workflow |
| 9 | Sybil Analyzer | Transaction pattern analysis + jitter recommendation engine |
| 10 | Beta Launch | Quality gates, eval metrics, cost tuning, public beta |

**Estimated Monthly Costs - Phase 2:**
| Item | Cost |
|------|------|
| Azure (scaled up) | $50-80 |
| Azure OpenAI | $40-120 |
| Retrieval index (Azure AI Search - selected) | $50-150 |
| Alchemy + Helius (growth usage) | $49-99 |
| Marketing (initial) | $0-50 |
| **Total** | **$189-499** |

### 4.3 Phase 3: Launch & Growth (Months 11-14)
| Month | Focus | Deliverables |
|-------|-------|--------------|
| 11 | Marketing Push | Content, Twitter, Discord outreach |
| 12 | Referrals | Referral program, affiliate system |
| 13 | API Launch | Developer API, documentation, webhook onboarding |
| 14 | Optimization | Performance, cost optimization, RAG quality hardening |

**Estimated Monthly Costs - Phase 3:**
| Item | Cost |
|------|------|
| Azure (Production) | $100-200 |
| API Providers + Retrieval Infrastructure | $150-300 |
| Marketing/Ads | $250-550 |
| Tools (analytics, etc.) | $50-100 |
| **Total** | **$550-1,150** |

### 4.4 Two-Year Summary
| Period | Avg Monthly Cost | Cumulative |
|--------|------------------|------------|
| Months 1-4 | $55 | $220 |
| Months 5-10 | $325 | $2,170 |
| Months 11-14 | $850 | $5,570 |
| Months 15-26 | $1,400 | $22,370 |
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
| 10 | 500 | 0 | $0 | $250 | -$250 |
| 12 | 1,500 | 30 | $600 | $550 | +$50 |
| 14 | 3,000 | 60 | $1,200 | $800 | +$400 |
| 20 | 8,000 | 160 | $3,200 | $1,100 | +$2,100 |
| 26 | 15,000 | 300 | $6,000 | $1,600 | +$4,400 |

**Year 2 Annual Net: ~$48,000**

### 5.2 Optimistic Model (5% Paid Conversion)
| Month | MAU | Paid Users | MRR | Expenses | Net |
|-------|-----|------------|-----|----------|-----|
| 10 | 500 | 0 | $0 | $250 | -$250 |
| 12 | 1,500 | 75 | $1,500 | $550 | +$950 |
| 14 | 3,000 | 150 | $3,000 | $800 | +$2,200 |
| 20 | 8,000 | 400 | $8,000 | $1,100 | +$6,900 |
| 26 | 15,000 | 750 | $15,000 | $1,600 | +$13,400 |

**Year 2 Annual Net: ~$115,000**

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

1. Sync roadmap and ADRs to the Telegram-first scope and tenant-aware partitioning model
2. Define approved RAG source corpus (official docs, governance posts, protocol announcements)
3. Provision Azure AI Search index and finalize retrieval schema/filters
4. Build a gold evaluation set (minimum 30 protocol-criteria Q&A pairs with expected citations)
5. Add or update unit tests for each implementation task completed this week

### Months 1-4 Calendar (MVP - Telegram First)

1. Infrastructure and auth foundations
2. Eligibility service + Alchemy/Helius integrations
3. Telegram bot and payment flows (Stripe + Coinbase)
4. Points tracking and reliability hardening
5. React dashboard explicitly deferred to Phase 4 (ADR-003)

### Months 5-10 Calendar (Phase 2: AI + RAG Delivery)

| Month | Focus | Deliverables |
|-------|-------|--------------|
| 5 | Corpus + Ingestion | Source registry, crawler/parsers, source snapshot storage |
| 6 | Retrieval Layer | Chunking policy, embeddings, vector index, metadata filters |
| 7 | Answer Layer | Citation-backed protocol parsing + path-to-eligibility responses |
| 8 | Change Detection | Criteria diff engine and alert routing (Telegram + API webhooks) |
| 9 | Guardrails + QA | Confidence thresholds, human review queue, hallucination checks |
| 10 | Beta Readiness | Load testing, prompt/index tuning, observability dashboards |

### Months 11-14 Calendar (Phase 3: Commercialization)

| Month | Focus | Deliverables |
|-------|-------|--------------|
| 11 | Distribution | Content and community growth |
| 12 | Monetization | Referral and partner programs |
| 13 | Developer Surface | Public API + webhook docs + usage plans |
| 14 | Optimization | Cost controls, SLO tuning, RAG quality hardening |

### RAG Backlog Additions

1. Citation renderer for Telegram/API output (`title`, `url`, `retrievedAt`)
2. Criteria change detector with severity scoring and subscription alerts
3. Human-review queue for low-confidence or conflicting source evidence
4. Offline eval harness: extraction precision/recall + grounded answer score
5. Ops metrics: retrieval hit rate, citation coverage, token/RU spend, stale-source ratio

### Documents and Files to Add or Update for RAG

1. Update `docs/strategy-roadmap.md` with phased RAG delivery and revised timeline
2. Update `DECISIONS.md` with RAG adoption and retrieval backend ADRs
3. Add `docs/rag-source-registry.md` (approved sources, ownership, refresh cadence)
4. Add `docs/rag-evaluation-plan.md` (quality gates and acceptance thresholds)
5. Update `docs/OPERATIONS-RUNBOOK.md` with RAG rollback + bad-source incident playbooks

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

*Document Version: 2.3*  
*Last Updated: February 16, 2026*  
*Combined Analysis: Claude + Gemini + Enterprise Review*

