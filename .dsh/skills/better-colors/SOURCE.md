# Skill source: better-colors

- **Repository:** [jakubkrehel/skills](https://github.com/jakubkrehel/skills)
- **Path in the repository:** `skills/better-colors`
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
| `SKILL.md` | 7647 |
| `color-formats.md` | 4515 |
| `color-usage.md` | 5833 |
| `contrast.md` | 4770 |
| `palette-generation.md` | 6022 |
| `palette-structure.md` | 5245 |
| `token-naming.md` | 5409 |
| `agents/openai.yaml` | 110 |

## Deviations

None: the folder is vendored as the source has it. `agents/openai.yaml` is the provider
companion the source ships next to `SKILL.md` and carries the Codex half of the invocation
switch (`policy.allow_implicit_invocation`); DSH does not read it.

## Notes

Model-invoked. Owns palette structure, token naming, colour notation and measuring a rendered
pair. `better-accessibility` decides when contrast is required and whether a pair fails; this
skill measures it and reports the value. The rule the skill insists on: never report a contrast
value you did not measure.

Local copy, no auto-update: refresh by hand when the source changes.
