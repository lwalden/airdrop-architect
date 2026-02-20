# CLAUDE.md — Airdrop Architect

> Claude reads this file automatically at the start of every session.
> Keep it concise — every line costs context tokens.

## Reference Documents

- **Strategy & Roadmap:** `docs/strategy-roadmap.md` — business context, revenue model, competitive analysis
- **Progress Tracking:** `PROGRESS.md` — current development state and session continuity
- **Decision Log:** `DECISIONS.md` — architectural decisions (do not re-debate recorded decisions)

---

## Session Protocol

### Starting a Session
1. Read `PROGRESS.md` — understand current state, active tasks, and priorities
2. Check `git status` for uncommitted work
3. Resume from "Next Priorities" in PROGRESS.md

> If no session context was injected above (you don't see PROGRESS.md content), read PROGRESS.md and DECISIONS.md manually before proceeding.

### During a Session
- Write code to files immediately — don't accumulate changes in memory
- Commit at natural checkpoints (compiles, tests pass, logical unit complete)
- Prefer smaller, frequent commits over one large commit
- Use Claude's native Tasks for complex multi-step work; keep PROGRESS.md as the durable record

### Ending a Session
Run `/handoff` to write a clear briefing for the next session. Hooks handle timestamp and auto-commit automatically.

---

## Project Identity

**Project:** Airdrop Architect
**Description:** Crypto airdrop eligibility checker and points tracking SaaS platform
**Type:** SaaS / Telegram Bot / Web App
**Stack:** C# / .NET 8 / Azure Functions / Cosmos DB / Redis / React 18 + TypeScript / Telegram Bot API / Stripe / Coinbase Commerce / Alchemy SDK (EVM) / Helius SDK (Solana)

**Developer Profile (Laurance):**
- 12 years C# backend experience; Azure cloud services expertise
- Currently employed — limited time availability
- Prefers collaboration over autonomous execution of major decisions
- Will create GitHub repos and merge PRs manually; needs prompting for external service signups
- Risk tolerance: medium (avoid bleeding edge/pre-release packages)

**Communication:**
- TL;DR summary first, then detailed explanation
- Be proactive about dependencies, explain decisions, offer alternatives, flag risks early

---

## Development Phases

- **Phase 1: Foundation (Weeks 1–4)** — infrastructure, auth, Telegram bot skeleton. See `docs/phase-1-setup.md`.
- **Phase 2: Core Features (Weeks 5–8)** — eligibility checking and points tracking
- **Phase 3: Payments (Weeks 9–12)** — Stripe and Coinbase Commerce
- **Phase 4: Polish & Launch (Weeks 13–16)** — React dashboard, beta launch

See `docs/phase-definitions.md` for per-phase success criteria checklists.

---

## Behavioral Rules

### Autonomy Boundaries
**You CAN autonomously:** Create files, install packages, run builds/tests, create branches and PRs, scaffold code
**Ask the human first:** Create GitHub repos, merge PRs, sign up for services, provide API keys, approve major architectural changes

### Credentials
- Never store credentials in code. Use `.env` files (gitignored) or Azure Key Vault.
- When you need a credential: "Please provide your [SERVICE] API key. I'll store it in .env (gitignored)."
- Always create `.env.example` showing required variables WITHOUT values.

### Git Workflow
- **Never commit directly to main** — always use feature branches
- Branch naming: `feature/short-description`, `fix/short-description`, `chore/short-description`
- All changes via PR. Claude creates PRs via `gh pr create`; human reviews and merges.
- After user merges PR: `git checkout main && git pull`, then start next feature branch.

### Pull Requests & Test Plans
See `docs/pr-guidelines.md` for PR format and test plan template.

### Internationalization & Geo-Restrictions
See `docs/i18n-localization.md` for locale patterns and OFAC/country blocklist rules (required from day 1).

---

## Context Budget

| File | Target Size | Action if Exceeded |
|------|------------|-------------------|
| CLAUDE.md | ~120 lines | Don't add without removing something |
| PROGRESS.md | ~20 lines active | Self-trimming: only 3 session notes kept |
| DECISIONS.md | Grows over time | Delete superseded entries (git history preserves them) |

**Reading Strategy:**
- PROGRESS.md: Every session (auto-injected by hook)
- DECISIONS.md: Auto-injected if decisions exist; always check before architectural choices
- strategy-roadmap.md: On-demand
- docs/phase-1-setup.md: On-demand (scaffolding, shell commands, model templates)
- docs/i18n-localization.md: On-demand (i18n/geo-restriction implementation)

---

## External Services

| Service | Purpose | Free Tier | When Needed |
|---------|---------|-----------|-------------|
| Azure | Hosting, DB, Functions | $200 credit | Week 1 |
| Alchemy | EVM blockchain data | 300M CU/mo | Week 3 |
| Helius | Solana blockchain data | 1M credits/mo | Phase 2 |
| Telegram | Bot hosting | Free | Week 2 |
| Stripe | Card payments | Free (2.9% fee) | Week 3 |
| Coinbase Commerce | Crypto payments | Free (1% fee) | Week 3 |
| GitHub | Code hosting | Free | Week 1 |
