# RAG Evaluation Plan

## Goal
Establish quality gates for citation-backed protocol parsing and path-to-eligibility recommendations before public beta.

## Evaluation Scope
1. Criteria extraction quality from protocol docs/announcements.
2. Groundedness and citation coverage of user-facing responses.
3. Change-detection accuracy when source criteria evolve.

## Test Set Requirements
- Minimum 30 protocol criteria Q&A pairs for initial gate.
- Include at least 5 adversarial prompts designed to induce hallucination.
- Include at least 5 change-detection cases (old vs updated criteria).

## Metrics
| Metric | Target | Description |
|--------|--------|-------------|
| Citation Coverage | >= 95% | Responses include at least one valid source citation |
| Groundedness Score | >= 0.90 | Answer content supported by retrieved chunks |
| Criteria Extraction Precision | >= 0.90 | Extracted criteria are correct |
| Criteria Extraction Recall | >= 0.85 | Important criteria are not missed |
| Change Detection Precision | >= 0.90 | Reported changes are real |
| Change Detection Recall | >= 0.85 | Real changes are not missed |

## Release Gates
1. Do not enable RAG responses for users unless citation coverage and groundedness thresholds pass.
2. Route low-confidence outputs to deterministic fallback messaging.
3. Require human review for conflicting evidence across sources.

## Observability Requirements
Track and alert on:
- Retrieval hit rate
- Stale-source ratio
- Average citations per response
- Token and RU cost per request
- Low-confidence fallback rate

## Iteration Cadence
- Weekly evaluation run during Months 5-10.
- Monthly regression run after launch.
- Re-run full suite after model, chunking, or retrieval strategy changes.

## Open Items
1. Finalize automated groundedness scorer implementation.
2. Define acceptance threshold adjustments for beta vs GA.
3. Integrate evaluation reports into CI/CD quality checks.
