# RAG Source Registry

## Purpose
Track approved protocol sources used by the RAG pipeline for AI Protocol Parsing, Path-to-Eligibility responses, and criteria change detection.

## Source Acceptance Rules
1. Prefer official protocol documentation, governance forums, and official announcement channels.
2. Record source ownership and trust level.
3. Snapshot each source on a fixed cadence for reproducibility.
4. Exclude unverifiable social posts unless linked from official channels.

## Source Inventory
| Source ID | Protocol | Type | URL | Owner | Trust Level | Refresh Cadence | Notes |
|-----------|----------|------|-----|-------|-------------|-----------------|-------|
| src-hyperliquid-docs | Hyperliquid | Docs | https://docs.hyperliquid.xyz/ | Protocol | High | Daily | |
| src-eigenlayer-docs | EigenLayer | Docs | https://docs.eigenlayer.xyz/ | Protocol | High | Daily | |
| src-scroll-docs | Scroll | Docs | https://docs.scroll.io/ | Protocol | High | Daily | |
| src-linea-docs | Linea | Docs | https://docs.linea.build/ | Protocol | High | Daily | |

## Snapshot Metadata Contract
Every ingested chunk should preserve:
- `sourceId`
- `sourceUrl`
- `sourceTitle`
- `snapshotDate`
- `chunkId`
- `protocol`
- `contentHash`

## Ownership and Review
- Technical owner: Platform engineering
- Domain owner: Product/strategy owner
- Review cadence: Weekly
- SLA: New high-priority source changes triaged within 24 hours

## Open Items
1. Implement Azure AI Search index provisioning and chunk ingestion pipeline.
2. Add all initial protocol announcement feeds.
3. Define source deprecation policy for stale or low-trust feeds.
