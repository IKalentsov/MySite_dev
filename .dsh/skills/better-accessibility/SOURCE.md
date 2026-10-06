# Skill source: better-accessibility

- **Repository:** [jakubkrehel/skills](https://github.com/jakubkrehel/skills)
- **Path in the repository:** `skills/better-accessibility`
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
| `SKILL.md` | 8254 |
| `focus-and-keyboard.md` | 6319 |
| `forms.md` | 3718 |
| `hit-areas.md` | 3957 |
| `motion-and-zoom.md` | 3939 |
| `screen-readers.md` | 5072 |
| `semantics-and-aria.md` | 5368 |
| `agents/openai.yaml` | 121 |

## Deviations

None: the folder is vendored as the source has it. `agents/openai.yaml` is the provider
companion the source ships next to `SKILL.md` and carries the Codex half of the invocation
switch (`policy.allow_implicit_invocation`); DSH does not read it.

## Notes

Model-invoked. Contrast measurement is owned by `better-colors`, text sizing by
`better-typography`, spatial RTL by `better-layout`; the skill names those hand-offs instead of
restating their rules.

Pairs with `frontend/principles/ui-and-accessibility.md`, which states the project-level
requirement (WCAG 2.2 AA, the mandatory screen states). This skill is the recipe layer.

Local copy, no auto-update: refresh by hand when the source changes.
