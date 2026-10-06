# Skill source: break

- **Repository:** [jakubkrehel/skills](https://github.com/jakubkrehel/skills)
- **Path in the repository:** `skills/break`
- **Branch:** `main`, commit `267330e1adfc66a718fb65fa6918c1f06d0a689e` (2026-08-29, "feat: new skill descriptions")
- **Retrieved:** 2026-09-21
- **Licence:** MIT (the `LICENSE` file in the repository root, "Copyright (c) 2026 Jakub Krehel")
- **Retrieval method:** the repository tarball. All 65 files of that commit were compared against
  the git blob SHA-1 from the GitHub API: no mismatch, no extra file, nothing missing. The
  folder was then copied here unchanged.
- **Set manifest:** `../SOURCES.json`

## Files

| File | Bytes |
|---|---|
| `SKILL.md` | 6681 |
| `scenarios.md` | 4320 |
| `agents/openai.yaml` | 166 |

## Deviations

None: the folder is vendored as the source has it. `agents/openai.yaml` is the provider
companion the source ships next to `SKILL.md` and carries the Codex half of the invocation
switch (`policy.allow_implicit_invocation`); DSH does not read it.

## Notes

**User-invoked** (`disable-model-invocation: true`): it writes a throwaway harness page and
stress-tests one component — long text, empty values, RTL, narrow containers — and the source
does not want an agent littering a repository with those pages unprompted.

It owns no domain rules and issues no verdict: each break names the `better-*` skill that owns
the fix, which keeps the finding actionable when the review is picked up later.

Local copy, no auto-update: refresh by hand when the source changes.
