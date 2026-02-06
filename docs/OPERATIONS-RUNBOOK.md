# OPERATIONS-RUNBOOK.md - Airdrop Architect Post-Launch Operations

> **Purpose:** Detailed operational procedures for maintaining Airdrop Architect after launch.
> **Audience:** Laurance (solo operator) and any future team members
> **Last Updated:** February 2026

---

## Table of Contents

1. [Daily Operations](#1-daily-operations)
2. [Weekly Operations](#2-weekly-operations)
3. [Monthly Operations](#3-monthly-operations)
4. [Quarterly Operations](#4-quarterly-operations)
5. [Airdrop Database Management](#5-airdrop-database-management)
6. [Points Program Maintenance](#6-points-program-maintenance)
7. [Integration Health Monitoring](#7-integration-health-monitoring)
8. [Incident Response](#8-incident-response)
9. [Competitive Intelligence](#9-competitive-intelligence)
10. [Scaling Triggers](#10-scaling-triggers)
11. [Exit Preparation](#11-exit-preparation)

---

## 1. Daily Operations

**Estimated Time:** 15-30 minutes

### 1.1 Morning Health Check (5 min)

```
□ Check Application Insights dashboard for overnight errors
  URL: https://portal.azure.com → airdrop-architect-{env}-insights
  
□ Review error rate (should be <1%)
  Query: exceptions | where timestamp > ago(24h) | summarize count() by type

□ Check Telegram bot responsiveness
  - Send /start to @GetAirdropArchitectBot
  - Verify response within 2 seconds

□ Glance at payment dashboard for failed transactions
  - Stripe: https://dashboard.stripe.com/payments
  - Coinbase: https://commerce.coinbase.com/dashboard
```

### 1.2 Airdrop News Scan (10 min)

**Sources to check daily:**

| Source | URL | What to Look For |
|--------|-----|------------------|
| Crypto Twitter Lists | Your curated list | "airdrop", "TGE", "token launch" |
| r/CryptoCurrency | reddit.com/r/CryptoCurrency | Airdrop megathreads |
| DeFiLlama Airdrops | defillama.com/airdrops | New listings |
| LayerZero/zkSync Discord | Protocol Discords | Claim announcements |

**Quick Twitter Search Queries:**
```
"airdrop" min_faves:100 -is:retweet
"claim now" crypto -scam min_faves:50
"eligibility" "snapshot" crypto
```

### 1.3 Error Log Review (5 min)

```kusto
// Application Insights query for critical errors
exceptions
| where timestamp > ago(24h)
| where severityLevel >= 3
| summarize count() by outerMessage, problemId
| order by count_ desc
| take 10
```

**Action triggers:**
- >10 errors from same source → Investigate immediately
- Payment webhook failures → Check Stripe/Coinbase status pages
- Alchemy/blockchain errors → Check RPC status, consider failover

---

## 2. Weekly Operations

**Estimated Time:** 2-4 hours (can be split across days)

### 2.1 Airdrop Database Update (60-90 min)

**Every Monday:**

#### Step 1: Discover New Airdrops
```
Sources to check:
□ airdrops.io - New listings
□ earni.fi - Upcoming airdrops  
□ DeFiLlama - Unlocks calendar
□ Twitter/X advanced search for past 7 days
□ Protocol-specific Discords (LayerZero, zkSync, Scroll, etc.)
```

#### Step 2: Add New Airdrops to Database

For each new airdrop discovered:

```json
// Template for new airdrop entry
{
  "name": "[Protocol Name]",
  "tokenSymbol": "[SYMBOL]",
  "chain": "[ethereum|arbitrum|optimism|base|solana]",
  "status": "[upcoming|claimable|expired]",
  "claimContract": "[0x... or null if not yet deployed]",
  "claimUrl": "[https://claim.protocol.xyz]",
  "claimDeadline": "[ISO 8601 date or null]",
  "snapshotDate": "[ISO 8601 date or null]",
  "eligibilitySource": "[URL to eligibility list/GitHub/IPFS]",
  "criteria": [
    "Criterion 1",
    "Criterion 2"
  ],
  "totalEligibleAddresses": null,
  "averageAllocationUsd": null
}
```

#### Step 3: Acquire Eligibility Data

**Priority Order:**
1. **GitHub releases** - Many projects publish CSVs or JSONs
2. **IPFS hashes** - Often linked in claim contracts
3. **Official announcements** - Sometimes include merkle roots
4. **Community aggregators** - Last resort, verify accuracy

**Eligibility Data Acquisition Script Pattern:**
```bash
# Example: Downloading eligibility list from GitHub
curl -L "https://github.com/project/airdrop/releases/download/v1/eligibility.json" \
  -o ./data/raw/[project]-eligibility.json

# Parse and import to Cosmos DB (via admin tool or script)
```

#### Step 4: Update Claim Statuses

```sql
-- Pseudo-query: Mark expired airdrops
UPDATE airdrops 
SET status = 'expired' 
WHERE claimDeadline < NOW() AND status = 'claimable'
```

### 2.2 Points Integration Health Check (30 min)

**Every Wednesday:**

For each active points provider:

| Protocol | Health Check |
|----------|--------------|
| Hyperliquid | Call API with test wallet, verify response format unchanged |
| EigenLayer | Check dashboard structure, verify scraping still works |
| Blast | Verify API endpoint, check for rate limit changes |
| Scroll | Check Marks dashboard, verify data extraction |

**Test Script Pattern:**
```bash
# Run points provider tests
dotnet test --filter "Category=PointsIntegration"

# Or manual check via curl
curl -X POST "https://api.hyperliquid.xyz/info" \
  -H "Content-Type: application/json" \
  -d '{"type":"userPoints","user":"0x[test-address]"}' | jq .
```

**If integration breaks:**
1. Check protocol's Discord/Twitter for API changes
2. Review network tab in browser to find new endpoints
3. Update provider code and test
4. Create PR with fix

### 2.3 Metrics Review (30 min)

**Every Friday:**

```
□ Weekly Active Users (WAU)
□ New user signups
□ Conversion rate (free → paid)
□ Revenue by tier
□ Churn (cancelled subscriptions)
□ Top checked wallets (identify power users)
□ Error rate trend
□ API response time P95
```

**Application Insights Dashboard Queries:**

```kusto
// User activity
customEvents
| where timestamp > ago(7d)
| where name == "WalletChecked"
| summarize checks = count() by bin(timestamp, 1d)
| render timechart

// New users
customEvents
| where timestamp > ago(7d)
| where name == "UserCreated"
| summarize count() by bin(timestamp, 1d)
```

### 2.4 Security Scan (15 min)

```bash
# Check for vulnerable NuGet packages
dotnet list package --vulnerable --include-transitive

# Review any GitHub Dependabot alerts
gh api repos/{owner}/{repo}/dependabot/alerts --jq '.[] | select(.state=="open")'
```

---

## 3. Monthly Operations

**Estimated Time:** 4-6 hours

### 3.1 First Monday: Financial Review

```
□ Export Stripe revenue report
□ Export Coinbase Commerce revenue report
□ Calculate MRR, growth rate, churn
□ Compare to projections (see strategy-roadmap.md Part 5)
□ Review Azure costs vs budget
□ Identify cost optimization opportunities
```

**Azure Cost Analysis:**
```bash
# View current month costs by resource
az consumption usage list \
  --start-date $(date -d "first day of this month" +%Y-%m-%d) \
  --end-date $(date +%Y-%m-%d) \
  --query "[].{Resource:instanceName, Cost:pretaxCost}" \
  --output table
```

### 3.2 Second Monday: Infrastructure Review

```
□ Cosmos DB performance metrics
  - RU consumption patterns
  - Storage growth rate
  - Query performance (slow queries)
  
□ Redis cache hit rate
  - Should be >90% for eligibility checks
  
□ Azure Functions execution times
  - Identify slow functions
  - Cold start frequency
  
□ Alchemy/Helius usage vs limits
  - Track % of free tier consumed
  - Plan upgrades if approaching limits
```

**Cosmos DB RU Analysis:**
```kusto
// Top RU-consuming queries
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.DOCUMENTDB"
| where Category == "QueryRuntimeStatistics"
| summarize avg(requestCharge_s) by querytext_s
| top 10 by avg_requestCharge_s desc
```

### 3.3 Third Monday: Content & Community

```
□ Review user feedback (Telegram messages, support requests)
□ Identify common feature requests
□ Update FAQ/help documentation if needed
□ Plan any blog posts or Twitter content
□ Engage with crypto Twitter (build presence)
```

### 3.4 Fourth Monday: Backup & DR Verification

```
□ Verify Cosmos DB continuous backup is working
  - Check backup status in Azure Portal
  - Document latest restore point
  
□ Test restore procedure (quarterly, but check monthly)
  - Restore to test container
  - Verify data integrity
  
□ Verify all credentials are in Key Vault
  - No hardcoded secrets in code
  - Rotation dates documented
  
□ Review OFAC blocked countries list
  - Check for sanctions changes
  - Update IGeoRestrictionService if needed
```

---

## 4. Quarterly Operations

**Estimated Time:** 1-2 days

### 4.1 API Key Rotation

**All external service API keys should be rotated:**

| Service | Key Location | Rotation Procedure |
|---------|--------------|-------------------|
| Alchemy | Azure Key Vault | Generate new key in Alchemy dashboard, update Key Vault, verify, delete old |
| Helius | Azure Key Vault | Same process |
| Stripe | Azure Key Vault | Roll keys in Stripe dashboard (supports key rolling) |
| Coinbase Commerce | Azure Key Vault | Generate new API key, update webhook secret |
| Telegram Bot | Azure Key Vault | Revoke via BotFather, create new token |

**Rotation Checklist:**
```
□ Generate new key in service dashboard
□ Add new key to Azure Key Vault as new version
□ Deploy application (will pick up new key)
□ Verify application works with new key
□ Disable/delete old key in service dashboard
□ Update DECISIONS.md with rotation date
```

### 4.2 Competitive Analysis Deep Dive

**Quarterly competitor review:**

| Competitor | Check For |
|------------|-----------|
| Drops.bot | New features, pricing changes, UI updates |
| Earndrop | New protocols supported, marketing tactics |
| AirdropScan | Data coverage, user experience |
| DeBank/Zerion | Airdrop features (are they entering our space?) |

**Analysis Template:**
```markdown
## Q[X] 2026 Competitive Analysis

### Drops.bot
- New features: 
- Pricing changes:
- Strengths we should copy:
- Weaknesses we can exploit:

### Market Trends
- Emerging competitors:
- Protocol changes affecting airdrops:
- New chains gaining traction:
```

### 4.3 Security Audit (Self-Assessment)

```
□ Review all API endpoints for authentication requirements
□ Check rate limiting is properly enforced
□ Verify webhook signature validation
□ Test OFAC geo-blocking is working
□ Review error messages (no sensitive data exposure)
□ Check for SQL/NoSQL injection vulnerabilities
□ Verify HTTPS everywhere
□ Review access logs for suspicious patterns
```

### 4.4 Performance Optimization

```
□ Identify slowest API endpoints (P95 latency)
□ Review Cosmos DB query patterns
□ Optimize cache usage
□ Consider adding/removing indexes
□ Review cold start mitigation strategies
□ Load test critical paths
```

---

## 5. Airdrop Database Management

### 5.1 Airdrop Lifecycle

```
UPCOMING → CLAIMABLE → EXPIRED
   │           │
   │           └── User can claim, we check eligibility
   │
   └── Snapshot hasn't happened or claim not open yet
```

### 5.2 Data Sources by Priority

| Priority | Source | Reliability | Effort |
|----------|--------|-------------|--------|
| 1 | Official GitHub releases | High | Low |
| 2 | Protocol documentation | High | Medium |
| 3 | IPFS eligibility lists | High | Medium |
| 4 | On-chain contract events | High | High |
| 5 | Community aggregators | Medium | Low |
| 6 | Social media announcements | Low | Low |

### 5.3 Eligibility Data Formats

**Common formats you'll encounter:**

```json
// Format 1: Simple address list
["0x123...", "0x456...", "0x789..."]

// Format 2: Address + Amount (CSV-like)
[
  {"address": "0x123...", "amount": "1000000000000000000"},
  {"address": "0x456...", "amount": "2000000000000000000"}
]

// Format 3: Merkle tree format
{
  "merkleRoot": "0xabc...",
  "claims": {
    "0x123...": {
      "index": 0,
      "amount": "1000000000000000000",
      "proof": ["0xdef...", "0xghi..."]
    }
  }
}
```

### 5.4 Airdrop Data Quality Checklist

Before adding an airdrop to production:

```
□ Verified claim contract address on block explorer
□ Eligibility list source is authoritative
□ Claim deadline confirmed (or marked as null)
□ Token symbol matches official project
□ Chain is correctly identified
□ At least one test wallet verified for eligibility match
□ Claim URL is official project domain
```

---

## 6. Points Program Maintenance

### 6.1 Provider Status Matrix

Update this weekly:

| Protocol | Method | Status | Last Verified | Notes |
|----------|--------|--------|---------------|-------|
| Hyperliquid | API | ✅ Active | YYYY-MM-DD | api.hyperliquid.xyz/info |
| EigenLayer | Scrape | ✅ Active | YYYY-MM-DD | Dashboard updated quarterly |
| Blast | API | ✅ Active | YYYY-MM-DD | blast.io/api |
| Scroll | Scrape | ⚠️ Flaky | YYYY-MM-DD | Rate limited aggressively |
| Linea | Manual | 🔄 Planned | - | Need to implement |

### 6.2 Adding a New Points Provider

**Checklist:**

```
1. Research Phase
   □ Find API documentation (if exists)
   □ Inspect network traffic on official dashboard
   □ Identify rate limits and authentication requirements
   □ Check if wallet signature is required

2. Implementation Phase
   □ Create IPointsProvider implementation
   □ Follow HyperliquidPointsProvider pattern
   □ Implement retry logic with Polly
   □ Add caching (1-hour minimum for points data)
   □ Add to DI container in Program.cs

3. Testing Phase
   □ Unit tests with mocked responses
   □ Integration tests with real API (rate-limited)
   □ Test with 5+ real wallets for accuracy
   □ Verify error handling for invalid addresses

4. Deployment Phase
   □ Feature flag for gradual rollout
   □ Monitor error rates for 24 hours
   □ Enable for all users if stable
```

### 6.3 Handling Provider Failures

**When a points API breaks:**

| Severity | Condition | Response |
|----------|-----------|----------|
| Low | Intermittent timeouts | Rely on retry logic, monitor |
| Medium | >10% error rate | Increase cache TTL, reduce call frequency |
| High | Complete failure | Disable provider, show "temporarily unavailable" |
| Critical | Data returning wrong | Disable immediately, investigate |

**Incident Response Template:**
```markdown
## Points Provider Incident: [Protocol]

**Detected:** YYYY-MM-DD HH:MM
**Severity:** [Low/Medium/High/Critical]
**Impact:** [X users affected, Y requests failed]

### Timeline
- HH:MM - Issue detected via [monitoring/user report]
- HH:MM - Investigation started
- HH:MM - Root cause identified: [cause]
- HH:MM - Mitigation applied: [action]
- HH:MM - Service restored

### Root Cause
[Detailed explanation]

### Prevention
[Steps to prevent recurrence]
```

---

## 7. Integration Health Monitoring

### 7.1 External Dependency Status Pages

Bookmark these for quick reference during incidents:

| Service | Status Page |
|---------|-------------|
| Azure | status.azure.com |
| Alchemy | status.alchemy.com |
| Helius | status.helius.dev |
| Stripe | status.stripe.com |
| Coinbase | status.coinbase.com |
| Telegram | (No official status page, check @BotNews) |

### 7.2 Application Insights Alerts

**Configure these alerts:**

| Alert | Condition | Action |
|-------|-----------|--------|
| High Error Rate | >5% requests failing | Email + SMS |
| Slow Response | P95 latency >3s | Email |
| Function Failures | >10 failures in 5 min | Email + SMS |
| Payment Webhook Failures | Any failure | Email (high priority) |
| Low Disk/Memory | <10% available | Email |

**Alert Configuration (Bicep/ARM):**
```bicep
resource errorRateAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: 'high-error-rate'
  properties: {
    severity: 1
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'ErrorRate'
          metricName: 'requests/failed'
          operator: 'GreaterThan'
          threshold: 5
          timeAggregation: 'Average'
        }
      ]
    }
  }
}
```

### 7.3 Synthetic Monitoring

**Implement health check endpoint:**

```csharp
// HealthCheckFunction.cs
[Function("HealthCheck")]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] 
    HttpRequestData req)
{
    var checks = new Dictionary<string, bool>();
    
    // Check Cosmos DB
    checks["cosmosdb"] = await _cosmosHealth.CheckAsync();
    
    // Check Redis
    checks["redis"] = await _redisHealth.CheckAsync();
    
    // Check Alchemy (light check)
    checks["alchemy"] = await _alchemyHealth.CheckAsync();
    
    var allHealthy = checks.Values.All(v => v);
    
    var response = req.CreateResponse(
        allHealthy ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable);
    await response.WriteAsJsonAsync(checks);
    return response;
}
```

**External monitoring (recommended):**
- Use UptimeRobot or Pingdom (free tier available)
- Check /health endpoint every 5 minutes
- Alert on 2 consecutive failures

---

## 8. Incident Response

### 8.1 Severity Definitions

| Severity | Definition | Response Time | Examples |
|----------|------------|---------------|----------|
| P1 - Critical | Service completely down | 15 min | All APIs returning 500, DB unreachable |
| P2 - High | Major feature broken | 1 hour | Payments failing, Telegram bot unresponsive |
| P3 - Medium | Feature degraded | 4 hours | Slow responses, partial data |
| P4 - Low | Minor issue | 24 hours | UI glitch, non-critical error in logs |

### 8.2 Incident Response Checklist

```
□ Acknowledge the incident
□ Assess severity and impact
□ Communicate status (if user-facing)
□ Identify root cause
□ Implement fix or mitigation
□ Verify resolution
□ Document in incident log
□ Conduct post-mortem (P1/P2 only)
```

### 8.3 Common Issues and Fixes

**Issue: Telegram bot not responding**
```
1. Check Azure Functions are running
   az functionapp show --name airdrop-architect-{env}-func --query state
   
2. Verify webhook URL is set correctly
   curl https://api.telegram.org/bot{TOKEN}/getWebhookInfo
   
3. Check function logs
   az functionapp log tail --name airdrop-architect-{env}-func
   
4. If webhook dropped, re-register
   curl -X POST "https://api.telegram.org/bot{TOKEN}/setWebhook?url={FUNCTION_URL}"
```

**Issue: Eligibility checks returning stale data**
```
1. Check Redis cache status
   az redis show --name airdrop-architect-{env}-redis --query provisioningState
   
2. Flush cache if needed (last resort)
   redis-cli -h {host} -p 6380 -a {password} --tls FLUSHDB
   
3. Verify Cosmos DB has latest data
```

**Issue: Stripe webhooks failing**
```
1. Check Stripe webhook logs
   https://dashboard.stripe.com/webhooks
   
2. Verify webhook endpoint is reachable
   curl -X POST https://{function-url}/api/payments/stripe/webhook
   
3. Check webhook secret matches
   Compare STRIPE_WEBHOOK_SECRET in Key Vault vs Stripe dashboard
```

---

## 9. Competitive Intelligence

### 9.1 Monitoring Competitors

**Weekly checks:**

| Competitor | What to Monitor |
|------------|-----------------|
| Drops.bot | New protocols added, UI changes, pricing |
| Earndrop | Marketing tactics, feature launches |
| DeFi dashboards | Are they adding airdrop features? |

**Tools:**
- Visualping.io - Monitor competitor homepage changes
- Twitter Lists - Follow competitor accounts
- SimilarWeb - Traffic estimates

### 9.2 Market Trend Signals

**Watch for:**

| Signal | Implication |
|--------|-------------|
| New L2 launches | New airdrop opportunities |
| Major protocol TGEs | Rush of eligibility checks |
| Sybil crackdowns | Sybil analyzer feature more valuable |
| Points program announcements | Add to points tracking |
| Regulatory news | May affect geo-restrictions |

### 9.3 Feature Request Tracking

**Template for user feedback:**

```markdown
## Feature Request: [Title]

**Source:** [Telegram user / Twitter / Email]
**Date:** YYYY-MM-DD
**Request:** [What they asked for]
**Use Case:** [Why they want it]
**Frequency:** [First time / Multiple requests]
**Effort Estimate:** [Low/Medium/High]
**Priority:** [P1/P2/P3/P4]
**Status:** [Backlog / Planned / In Progress / Done / Won't Do]
```

---

## 10. Scaling Triggers

### 10.1 When to Upgrade Infrastructure

| Metric | Threshold | Action |
|--------|-----------|--------|
| Cosmos RU consumption | >80% of limit | Increase RU or optimize queries |
| Redis memory | >80% used | Upgrade tier or review TTLs |
| Function execution time | P95 >5s | Optimize or scale out |
| Alchemy CU usage | >70% of tier | Plan upgrade or add caching |
| User count | >1,000 MAU | Consider UAT/Prod split |
| Revenue | >$1,000 MRR | Invest in monitoring/redundancy |

### 10.2 Cost Optimization Opportunities

| Opportunity | When | Savings |
|-------------|------|---------|
| Cosmos: Provisioned vs Serverless | >$100/month Cosmos cost | 20-40% |
| Redis: Review cache TTLs | Memory >50% | Prevent tier upgrade |
| Functions: Premium plan | Cold starts problematic | Better performance/cost |
| Alchemy: Annual contract | Stable usage pattern | 10-20% |

### 10.3 Feature Scaling

| User Milestone | Features to Prioritize |
|----------------|------------------------|
| 100 users | Core reliability, bug fixes |
| 500 users | Performance optimization |
| 1,000 users | Analytics, user feedback collection |
| 2,500 users | Referral program, self-service features |
| 5,000 users | API for developers, white-label |
| 10,000+ users | Team features, enterprise tier |

---

## 11. Exit Preparation

### 11.1 Ongoing Documentation for Sale

Maintain these for potential acquirers:

```
□ Monthly financial statements
□ User growth metrics (MRR, MAU, churn)
□ Technical architecture diagram (keep updated)
□ API documentation
□ Operational runbooks (this document)
□ Vendor/contract list
□ IP documentation (what you own)
```

### 11.2 Code Quality Standards

To maintain "sellable" codebase:

```
□ Consistent code style (use .editorconfig)
□ Meaningful commit messages
□ No hardcoded secrets (all in Key Vault)
□ README.md with setup instructions
□ Architecture decision records (DECISIONS.md)
□ Test coverage for critical paths
□ No TODO comments in production code
```

### 11.3 12-Month Pre-Sale Checklist

If planning to sell within 12 months:

```
□ Clean up any technical debt
□ Document all operational procedures
□ Ensure all data is exportable
□ Remove owner dependencies (automate everything)
□ Build SOPs for common tasks
□ Prepare data room structure
□ Get financials audited (if revenue >$50K)
□ Document all third-party dependencies
□ Ensure contracts are transferable
□ Build transition plan template
```

---

## Appendix A: Quick Reference Commands

### Azure CLI

```bash
# Check function app status
az functionapp show --name airdrop-architect-{env}-func --resource-group airdrop-architect-{env}-rg --query state

# View function logs
az functionapp log tail --name airdrop-architect-{env}-func --resource-group airdrop-architect-{env}-rg

# Restart function app
az functionapp restart --name airdrop-architect-{env}-func --resource-group airdrop-architect-{env}-rg

# Check Cosmos DB metrics
az cosmosdb show --name airdrop-architect-{env}-cosmos --resource-group airdrop-architect-{env}-rg
```

### Redis CLI

```bash
# Connect to Azure Redis
redis-cli -h airdrop-architect-{env}-redis.redis.cache.windows.net -p 6380 -a {password} --tls

# Check memory usage
INFO memory

# List keys matching pattern
KEYS eligibility:*

# Check TTL on key
TTL eligibility:0x123...
```

### Application Insights Queries

```kusto
// Requests by endpoint (last 24h)
requests
| where timestamp > ago(24h)
| summarize count(), avg(duration) by name
| order by count_ desc

// Errors by type
exceptions
| where timestamp > ago(24h)
| summarize count() by type, outerMessage
| order by count_ desc

// User activity
customEvents
| where timestamp > ago(7d)
| where name in ("WalletChecked", "PointsChecked", "UserCreated")
| summarize count() by name, bin(timestamp, 1d)
| render timechart
```

---

## Appendix B: Contact List

| Service | Support Channel | SLA |
|---------|-----------------|-----|
| Azure | https://azure.microsoft.com/support | Based on plan |
| Alchemy | Discord: discord.gg/alchemy | Community / Priority for paid |
| Helius | Discord: discord.gg/helius | Community / Priority for paid |
| Stripe | https://support.stripe.com | Email / Chat for paid |
| Coinbase Commerce | https://help.coinbase.com | Email |
| Telegram | @BotSupport | Slow |

---

## Appendix C: Estimated Time Summary

| Frequency | Tasks | Time |
|-----------|-------|------|
| Daily | Health check, news scan, error review | 15-30 min |
| Weekly | Airdrop updates, points check, metrics | 2-4 hours |
| Monthly | Financial, infrastructure, backup verify | 4-6 hours |
| Quarterly | Key rotation, security, competitive analysis | 1-2 days |

**Total Monthly Operational Time:** ~20-30 hours

This is compatible with a full-time job, especially after initial stabilization period (first 2-3 months post-launch).

---

*Document Version: 1.0*
*Created: February 2026*
*Next Review: After beta launch*
