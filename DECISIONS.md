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

### ADR-006: Observability and Error Handling Strategy
**Date:** 2026-02-02
**Status:** Decided

**Context:** Need comprehensive monitoring and resilience for a system that depends on multiple external services (blockchain RPCs, Telegram API, payment providers).

**Options:**
1. Basic logging only
2. Application Insights without distributed tracing
3. Full observability stack with resilience patterns

**Decision:**
- **Observability:** Azure Application Insights with distributed tracing enabled
- **Error Handling:** Polly library for retry policies, circuit breakers, and rate-limit handling
- **Secret Management:** Azure Key Vault with 90-day rotation policy for API keys
- **Alerting:** Application Insights alerts for >5% error rate or P95 latency >2s

**Rationale:**
- Blockchain RPC calls are inherently unreliable (rate limits, node issues)
- Distributed tracing essential for debugging multi-function workflows
- Secret rotation reduces blast radius of compromised credentials
- Polly is the standard .NET resilience library

**Implementation Notes:**
- Add `Microsoft.Extensions.Http.Polly` NuGet package
- Configure Polly policies per external service (different retry logic for Alchemy vs Stripe)
- Enable W3C trace context propagation in Application Insights

---

### ADR-007: Cosmos DB Backup Strategy
**Date:** 2026-02-02
**Status:** Decided

**Context:** Need disaster recovery plan for database corruption or accidental deletion.

**Options:**
1. No backup (rely on Cosmos DB's built-in replication)
2. Periodic backup only (daily/weekly exports)
3. Continuous backup with point-in-time restore

**Decision:**
- Enable Continuous Backup (7-day point-in-time restore)
- Weekly export to Azure Blob Storage for long-term retention
- Document restore procedure in runbook

**Rationale:**
- Continuous backup adds ~$0.20/GB/month but provides granular recovery
- Serverless mode supports continuous backup
- Export provides additional safety net and audit trail
- Recovery documentation ensures we can act quickly in emergency

**Implementation Notes:**
- Enable continuous backup when creating Cosmos DB account
- Create Azure Function on timer trigger for weekly blob export
- Store exports in separate storage account for isolation

---

### ADR-008: Software Licensing
**Date:** 2026-02-02
**Status:** Decided

**Context:** Need to decide on intellectual property approach for codebase.

**Options:**
1. MIT License (permissive open source)
2. Apache 2.0 (permissive with patent grant)
3. AGPL (copyleft, requires source disclosure)
4. Proprietary / All Rights Reserved

**Decision:** Proprietary / All Rights Reserved

**Rationale:**
- Core eligibility logic and airdrop criteria database are competitive advantages
- SaaS business model doesn't benefit from open source
- May open-source non-core utilities later if beneficial for marketing
- No LICENSE file in repo = proprietary by default

**Implementation Notes:**
- Add copyright header to source files
- If ever contributing open-source utilities, create separate repos with explicit license

---

### ADR-009: Data Ownership and Multi-Tenancy
**Date:** 2026-02-02
**Status:** Decided

**Context:** Need to clarify data ownership for potential white-label/B2B licensing in Phase 3.

**Decision:**
- **Aggregated airdrop criteria:** Owned by Airdrop Architect
- **User wallet data:** Owned by users, we are data processors
- **B2B tenants:** Logical isolation via `tenantId` partition key
- Database schema must support tenant isolation from Day 1

**Rationale:**
- Clear ownership enables white-label licensing without legal ambiguity
- Partition key strategy enables data isolation without separate databases
- GDPR requires clear processor/controller distinction
- Forward-compatible schema avoids costly migration later

**Implementation Notes:**
- Add optional `tenantId` field to User model (null for direct users)
- Partition key strategy: `tenantId` or `userId` depending on query patterns
- Document data processing agreement template for B2B customers

---

## Pending Decisions

*Decisions that need to be made but haven't been finalized yet:*

### PDR-001: Authentication Strategy
**Context:** How users authenticate to the web dashboard

**Options to consider:**
1. Telegram Login Widget (links to bot account)
2. Magic link email auth
3. Wallet-based auth (Sign-In with Ethereum / SIWE)
4. Traditional email/password
5. Hybrid: Telegram primary + optional wallet linking

**Considerations:**
- SIWE adds trust/security for crypto-native users
- SIWE could enable wallet verification without manual address entry
- Telegram Login provides seamless bot↔web account linking
- Hybrid approach offers flexibility but adds implementation complexity

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

### ADR-010: Multi-Environment Strategy
**Date:** 2026-02-03
**Status:** Decided

**Context:** Need to support multiple deployment environments (dev, uat, prod) with consistent naming and isolation.

**Decision:**
- **Naming Convention:** `airdrop-architect-{env}-{resource}` (e.g., `airdrop-architect-dev-cosmos`)
- **Resource Groups:** One per environment (`airdrop-architect-dev-rg`, `airdrop-architect-uat-rg`, `airdrop-architect-prod-rg`)
- **Region:** West US 2 for all environments (simplifies networking, reduces latency between services)
- **Isolation:** Complete resource isolation between environments (no shared databases)

**Environments:**
| Environment | Purpose | Cost Tier |
|-------------|---------|-----------|
| DEV | Development and testing | Serverless/Basic |
| UAT | User acceptance testing, pre-prod validation | Serverless/Basic |
| PROD | Production traffic | Serverless initially, upgrade as needed |

**Resource Naming:**
| Resource | DEV | UAT | PROD |
|----------|-----|-----|------|
| Resource Group | airdrop-architect-dev-rg | airdrop-architect-uat-rg | airdrop-architect-prod-rg |
| Cosmos DB | airdrop-architect-dev-cosmos | airdrop-architect-uat-cosmos | airdrop-architect-prod-cosmos |
| Redis Cache | airdrop-architect-dev-redis | airdrop-architect-uat-redis | airdrop-architect-prod-redis |
| App Insights | airdrop-architect-dev-insights | airdrop-architect-uat-insights | airdrop-architect-prod-insights |
| Function App | airdrop-architect-dev-func | airdrop-architect-uat-func | airdrop-architect-prod-func |

**Rationale:**
- Clear naming prevents accidental cross-environment operations
- Resource group isolation enables easy cost tracking per environment
- Same region reduces complexity and cross-region data transfer costs
- Consistent pattern scales to additional environments if needed

**Implementation Notes:**
- Use Azure CLI scripts or Bicep templates for repeatable environment creation
- Store environment-specific connection strings in `.env.{environment}` files locally
- Azure Functions will use App Configuration or environment variables for runtime config
- DEV environment created 2026-02-03

---

### PDR-002: Transactional Email Service
**Date:** 2026-02-03
**Status:** Pending Decision

**Context:** Users completing payment have no email confirmation - they only see an in-app Telegram message. For a professional payment experience, we need:
- Payment receipt emails
- Welcome emails explaining tier features
- Subscription renewal reminders
- Payment failure notifications

**Options:**
1. **SendGrid** - Industry standard, generous free tier (100 emails/day), excellent deliverability
2. **Azure Communication Services** - Native Azure integration, pay-per-email pricing
3. **Stripe-native emails** - Stripe can send receipts automatically, limited customization
4. **Postmark** - Great for transactional email, excellent templates

**Considerations:**
- Need user email address (currently optional in User model)
- Email collection UX: Stripe checkout can collect email
- GDPR compliance: Need unsubscribe option for non-transactional emails
- Integration effort vs. feature value

**Recommendation:** Start with **Stripe's native receipt emails** (free, automatic) and add SendGrid for welcome/feature emails in Phase 2.

**Action Required:** User to decide on email provider preference

---

### ADR-011: Internationalization and Geographic Restrictions
**Date:** 2026-02-05
**Status:** Decided

**Context:** Need to define geographic scope for the service and whether to support multiple languages. Key considerations:
- OFAC sanctions compliance is mandatory for US-based businesses
- Telegram Bot API does NOT auto-translate; we must implement i18n ourselves
- Many crypto airdrops exclude US users, but we're an information service (lower risk)
- Localization adds development overhead but expands addressable market

**Options Considered:**
1. **US-only (English)** - Simplest, clearest legal position, no i18n work
2. **US + EU + English-speaking (English-only)** - Good market, favorable MiCA exemptions
3. **Global (minus OFAC)** - Maximum market, requires full i18n infrastructure

**Decision:** Hybrid approach - **Option 2 for MVP, architected for Option 3**

**MVP Scope (Phase 1-3):**
- English-only UI
- Target markets: US, EU, UK, Canada, Australia, Singapore
- Block OFAC-sanctioned countries: Iran, North Korea, Syria, Cuba, Russia, Belarus, Venezuela, Afghanistan
- Block Algeria (crypto banned July 2025)
- Geo-restriction via IP detection + ToS self-declaration

**Future Expansion (Phase 4+):**
- Add localization when user demand justifies it
- Priority languages: Spanish, Portuguese, Chinese, Russian (if sanctions lifted)
- Localization infrastructure built into code from Day 1

**Rationale:**
- English covers majority of crypto users globally
- EU's MiCA regulation exempts information services
- OFAC compliance is non-negotiable for US business
- Building i18n-ready infrastructure now avoids costly refactoring later
- Crypto is global; limiting to US-only unnecessarily constrains growth

**Implementation Requirements:**

1. **Geographic Restriction Service:**
   - `IGeoRestrictionService` interface with `IsAllowedAsync(string countryCode)`
   - Configurable blocked country list (not hardcoded)
   - Store user's country in User model for analytics

2. **i18n-Ready Code Patterns:**
   - All user-facing strings externalized (no hardcoded text in code)
   - Use resource files or JSON locale files from Day 1
   - Telegram: Read `language_code` from user, store preference
   - Default to English when locale unavailable

3. **String Externalization Structure:**
   ```
   /locales
     /en
       telegram.json      # Bot messages
       errors.json        # Error messages
       notifications.json # Alert templates
     /es (future)
     /pt (future)
   ```

4. **ToS/Legal Localization:**
   - English ToS/Privacy Policy for MVP
   - Plan for translated legal documents when expanding
   - Include jurisdiction clause in ToS

**Blocked Countries List (MVP):**
| Country | Reason | Code |
|---------|--------|------|
| Iran | OFAC Comprehensive | IR |
| North Korea | OFAC Comprehensive | KP |
| Syria | OFAC Comprehensive | SY |
| Cuba | OFAC Comprehensive | CU |
| Russia | OFAC Significant | RU |
| Belarus | OFAC Significant | BY |
| Venezuela | OFAC Significant | VE |
| Afghanistan | OFAC Partial | AF |
| Algeria | Crypto banned (July 2025) | DZ |
| Crimea Region | OFAC | UA-43 |

**Phase 4 Localization Checklist (Future):**
- [ ] Implement i18n library integration (e.g., custom or Microsoft.Extensions.Localization)
- [ ] Translate Telegram bot messages (priority: ES, PT, ZH)
- [ ] Translate React dashboard
- [ ] Translate ToS and Privacy Policy
- [ ] Add language selector to settings
- [ ] RTL support consideration (Arabic, Hebrew)

---
