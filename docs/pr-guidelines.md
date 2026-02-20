# Pull Request & Test Plan Guidelines

> Extracted from CLAUDE.md during AIAgentMinder v0.5.3 update.

---

## Pull Request Format

PRs should include:
- Clear title describing the change
- Summary of what was changed and why
- Test plan (manual testing steps or automated test references)
- Any follow-up items or known limitations

Keep PRs reasonably sized — prefer multiple smaller PRs over one massive PR.
After PR is merged, update PROGRESS.md with completed task.

---

## Test Plan Format

```markdown
## Test Plan
- [ ] Step 1: Description of what to test
- [ ] Step 2: Expected outcome
- [ ] Verify: Specific verification steps

**Prerequisites:** What needs to be set up before testing
**Environment:** Local / Azure / Both
```
