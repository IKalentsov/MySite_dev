# Skill source: better-typography

- **Repository:** [jakubkrehel/skills](https://github.com/jakubkrehel/skills)
- **Path in the repository:** `skills/better-typography`
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
| `SKILL.md` | 10846 |
| `choosing-fonts.md` | 3179 |
| `css-cheat-sheet.md` | 3952 |
| `details-and-accessibility.md` | 4211 |
| `spacing-and-sizing.md` | 4294 |
| `variable-fonts-and-opentype.md` | 4308 |
| `wrapping-and-punctuation.md` | 3277 |
| `agents/openai.yaml` | 111 |

## Deviations

None: the folder is vendored as the source has it. `css-cheat-sheet.md` maps each declaration
to its Tailwind equivalent, which matches the stack of this section. `agents/openai.yaml` is
the provider companion the source ships next to `SKILL.md` and carries the Codex half of the
invocation switch (`policy.allow_implicit_invocation`); DSH does not read it.

## Notes

Model-invoked. Owns how text renders, wraps and behaves in mixed-direction content. The words
belong to `better-writing`, heading semantics to `better-accessibility`, contrast to
`better-colors`.

Local copy, no auto-update: refresh by hand when the source changes.
