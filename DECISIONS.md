# DECISIONS.md - Architectural Decision Record

> **Purpose:** Track architectural and design decisions to avoid re-debating across sessions.
> **Format:** Each decision has context, options considered, decision made, and rationale.

---

## How to Use This File

When a significant decision is made during development, add it here with:
1. **Context** - What problem/question arose
2. **Options** - What alternatives were considered
3. **Decision** - What was chosen
4. **Rationale** - Why this choice was made
5. **Date** - When the decision was made

---

## Decisions

### ADR-001: Technology Stack
**Date:** 2026-02-02
**Status:** Decided

**Context:** Choosing the core technology stack for the project.

**Decision:**
- Backend: C# / .NET 8 / Azure Functions
- Database: Azure Cosmos DB (NoSQL)
- Cache: Azure Redis Cache
- Frontend: React 18 + TypeScript + Tailwind CSS
- Bot: Telegram Bot API (via Telegram.Bot NuGet)
- Payments: Stripe + Coinbase Commerce
- Blockchain: Alchemy SDK (EVM) + Helius SDK (Solana)

**Rationale:**
- Leverages Laurance's 12 years of C# experience
- Azure ecosystem provides integrated services
- Cosmos DB offers flexible schema for varying airdrop rules
- Serverless (Functions) scales to zero, cost-effective for early stage

---

### ADR-002: Cosmos DB Capacity Mode
**Date:** 2026-02-02
**Status:** Decided

**Context:** Choosing between Provisioned vs Serverless capacity for Cosmos DB.

**Options:**
1. Provisioned throughput - predictable cost but requires capacity planning
2. Serverless - pay per request, scales to zero

**Decision:** Serverless

**Rationale:**
- Cheapest for development and early production
- No minimum cost when not in use
- Can migrate to provisioned later if needed for cost optimization at scale

---

### ADR-003: Primary User Interface
**Date:** 2026-02-02
**Status:** Decided

**Context:** Deciding the primary user interface for MVP.

**Options:**
1. Web app first
2. Telegram bot first
3. Both simultaneously

**Decision:** Telegram bot first, web dashboard in Phase 4

**Rationale:**
- Crypto users are highly active on Telegram
- Lower development overhead than web app
- Faster time to first users
- Web dashboard adds polish but not required for core value prop

---

### ADR-004: Payment Processing
**Date:** 2026-02-02
**Status:** Decided

**Context:** Choosing payment providers for fiat and crypto.

**Decision:**
- Fiat: Stripe (2.9% + $0.30 per transaction)
- Crypto: Coinbase Commerce (1% per transaction)

**Rationale:**
- Stripe is the standard for fiat subscriptions
- Coinbase Commerce is the easiest crypto payment integration
- Could add BTCPay Server later for 0% BTC fees (self-hosted)

---

### ADR-005: Session Continuity Approach
**Date:** 2026-02-02
**Status:** Decided

**Context:** How to maintain context across multiple development sessions with Claude.

**Options:**
1. Rely solely on git commit history
2. Use CLAUDE.md only with detailed instructions
3. Add dedicated progress tracking files

**Decision:** Add PROGRESS.md and DECISIONS.md alongside CLAUDE.md

**Rationale:**
- PROGRESS.md provides quick state snapshot for session resume
- DECISIONS.md prevents re-debating architectural choices
- Git history is too granular for quick context
- Structured files are faster to parse than free-form notes

---

## Pending Decisions

*Decisions that need to be made but haven't been finalized yet:*

### PDR-001: Authentication Strategy
**Context:** How users authenticate to the web dashboard
**Options to consider:**
1. Telegram Login Widget (links to bot account)
2. Magic link email auth
3. Wallet-based auth (Sign-In with Ethereum)
4. Traditional email/password

**Status:** Not yet decided - will address in Phase 4 with web dashboard

---

### PDR-002: Points Tracking Method Priority
**Context:** Which protocols to prioritize for points tracking
**Considerations:**
- Hyperliquid has API
- Some protocols only have scrapeable dashboards
- Need to balance effort vs user value

**Status:** Will decide at start of Phase 2

---
