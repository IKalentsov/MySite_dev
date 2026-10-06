# Skill source: better-ui

- **Repository:** [jakubkrehel/skills](https://github.com/jakubkrehel/skills)
- **Path in the repository:** `skills/better-ui`
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
| `SKILL.md` | 7152 |
| `animations.md` | 7033 |
| `enter-exit.md` | 3807 |
| `icon-transitions.md` | 4061 |
| `icons.md` | 4267 |
| `performance.md` | 2928 |
| `surfaces.md` | 6268 |
| `agents/openai.yaml` | 113 |

## Deviations

None: the folder is vendored as the source has it. `agents/openai.yaml` is the provider
companion the source ships next to `SKILL.md` and carries the Codex half of the invocation
switch (`policy.allow_implicit_invocation`); DSH does not read it.

## Notes

Model-invoked. Owns optional visual polish after the interaction itself is sound: concentric
radius, optical alignment, surface depth, icons and motion. Reduced-motion requirements belong
to `better-accessibility`; this skill owns the animation recipe used when motion is
appropriate.

Fills a gap this section had: the frontend principles said nothing about motion, timing or
easing, and the only motion material was `react-view-transitions` (Vercel), which is scoped to
the View Transition API.

Local copy, no auto-update: refresh by hand when the source changes.
