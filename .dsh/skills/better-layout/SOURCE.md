# Skill source: better-layout

- **Repository:** [jakubkrehel/skills](https://github.com/jakubkrehel/skills)
- **Path in the repository:** `skills/better-layout`
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
| `SKILL.md` | 5644 |
| `grouping-and-alignment.md` | 5423 |
| `spacing-and-adaptivity.md` | 6609 |
| `agents/openai.yaml` | 117 |

## Deviations

None: the folder is vendored as the source has it. `agents/openai.yaml` is the provider
companion the source ships next to `SKILL.md` and carries the Codex half of the invocation
switch (`policy.allow_implicit_invocation`); DSH does not read it.

## Notes

Model-invoked. Owns spatial grouping, alignment, spacing, responsive structure and logical CSS
properties. Hit areas and focus belong to `better-accessibility`, radius, shadow and animation
to `better-ui`, line length to `better-typography`.

Local copy, no auto-update: refresh by hand when the source changes.
