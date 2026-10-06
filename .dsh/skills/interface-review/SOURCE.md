# Skill source: interface-review

- **Repository:** [jakubkrehel/skills](https://github.com/jakubkrehel/skills)
- **Path in the repository:** `skills/interface-review`
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
| `SKILL.md` | 10690 |
| `removed-signals.md` | 2907 |
| `scope-resolution.md` | 8155 |
| `agents/openai.yaml` | 157 |

## Deviations

None: the folder is vendored as the source has it. `agents/openai.yaml` is the provider
companion the source ships next to `SKILL.md` and carries the Codex half of the invocation
switch (`policy.allow_implicit_invocation`); DSH does not read it.

## Notes

**User-invoked** (`disable-model-invocation: true`): the source makes it user-only because a
review of a change is something a person asks for. It resolves the scope — a diff, a branch, a
pull request — classifies each finding as `Introduced`, `Regression` or `Pre-existing`, and
hands the review up to `better-interface`, which owns severity, the cap and the verdict.

Complements `shared/skills/code-review` (two axes, Standards and Spec, no roles attached):
that one reviews a change as engineering, this one reviews it as interface.

Local copy, no auto-update: refresh by hand when the source changes.
