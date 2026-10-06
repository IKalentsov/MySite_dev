# Skill source: better-writing

- **Repository:** [jakubkrehel/skills](https://github.com/jakubkrehel/skills)
- **Path in the repository:** `skills/better-writing`
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
| `SKILL.md` | 6420 |
| `agents/openai.yaml` | 108 |

## Deviations

None: the folder is vendored as the source has it. The source directory holds only `SKILL.md`
and the provider companion. `agents/openai.yaml` carries the Codex half of the invocation
switch (`policy.allow_implicit_invocation`); DSH does not read it.

## Notes

Model-invoked. Owns product copy: terminology, voice, labels, error and empty-state text. How
copy renders belongs to `better-typography`, error markup to `better-accessibility`, room for
translated strings to `better-layout`.

Overlaps `writing-guidelines` (Vercel) only on surface: that one reviews prose against an
externally fetched guide, this one owns interface copy inside a component.

Local copy, no auto-update: refresh by hand when the source changes.
