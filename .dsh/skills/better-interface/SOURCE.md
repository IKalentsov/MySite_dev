# Skill source: better-interface

- **Repository:** [jakubkrehel/skills](https://github.com/jakubkrehel/skills)
- **Path in the repository:** `skills/better-interface`
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
| `SKILL.md` | 9532 |
| `review-format.md` | 2547 |
| `agents/openai.yaml` | 103 |

## Deviations

None: the folder is vendored as the source has it. `agents/openai.yaml` is the provider
companion the source ships next to `SKILL.md` and carries the Codex half of the invocation
switch (`policy.allow_implicit_invocation`); DSH does not read it.

## Notes

The orchestrator of the set: it routes an interface to each `better-*` skill, collects their
evidence and consolidates one ranked verdict, and it owns the shared severity ladder, the
finding cap and `review-format.md`. The rules themselves stay in the domain skills. It is
model-invocable, so the agent can start a review on its own.

Conversely, it cannot start `interface-review` (user-invoked) and asks the user to run it.

Local copy, no auto-update: refresh by hand when the source changes.
