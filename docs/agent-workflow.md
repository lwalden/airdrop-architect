# Agent Workflow — Sub-Agent Delegation & Checkpoint Protocol

> Extracted from CLAUDE.md during AIAgentMinder v0.5.3 update.
> Reference on-demand for delegation patterns and checkpoint reporting format.

---

## Sub-Agent Delegation

When appropriate, spawn sub-agents to parallelize work. Use this format:

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
