# Skill source: composition-patterns

- **Repository:** [vercel-labs/agent-skills](https://github.com/vercel-labs/agent-skills)
- **Path in the repository:** `skills/composition-patterns/`
- **Branch:** `main`
- **Commit:** `063bee94c3f4df8453406c830b0a7df0f2860278` (28.08.2026)
- **Retrieved:** 2026-09-21
- **Licence:** MIT (stated in the repository README and in the `license` field of `SKILL.md`; there is no separate `LICENSE` file in the repository root)
- **Retrieval method:** `web_fetch` + `write`; every file was verified against the git-blob-SHA1 from the GitHub API — size and SHA-1 matched byte for byte.

## What was vendored

13 files, 27,712 bytes.

| File | Bytes |
|---|---|
| `SKILL.md` | 2886 |
| `README.md` | 2140 |
| `metadata.json` | 530 |
| `rules/_sections.md` | 829 |
| `rules/_template.md` | 329 |
| `rules/architecture-avoid-boolean-props.md` | 2267 |
| `rules/architecture-compound-components.md` | 2600 |
| `rules/patterns-children-over-render-props.md` | 1886 |
| `rules/patterns-explicit-variants.md` | 2395 |
| `rules/react19-no-forwardref.md` | 953 |
| `rules/state-context-interface.md` | 4974 |
| `rules/state-decouple-implementation.md` | 2699 |
| `rules/state-lift-state.md` | 3224 |

## Not vendored

- There are no binary or non-text files inside the skill.
- `AGENTS.md` (22,627 bytes) — a compiled version of the same rules (`SKILL.md` + `rules/**`
  in a single build). **Removed from the base:** DSH reads any `AGENTS.md` in the tree as directory
  instructions, so it would be loaded automatically into every session's context and would duplicate
  `SKILL.md` together with `rules/**`. Available at the source:
  <https://raw.githubusercontent.com/vercel-labs/agent-skills/main/skills/composition-patterns/AGENTS.md>

## Notes

- `SKILL.md` and the frontmatter were not edited: the `name` field is `vercel-composition-patterns`
  and does not match the directory name `composition-patterns` — that is how it is in the source. DSH takes the skill name
  from the frontmatter, so the mismatch is harmless; `scripts/verify-library.ps1`
  does not compare vendored names with the folder name.

Local copy, no auto-update: refresh by hand when the source changes.
