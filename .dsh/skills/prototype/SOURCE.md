# Skill source: prototype

- **Repository:** [mattpocock/skills](https://github.com/mattpocock/skills)
- **Path in the repository:** `skills/engineering/prototype`
- **Branch:** `main`, tag `v1.3.1`, commit `24fe0ef7737efae15c87225755e9f6f5965e4888` (2026-10-04)
- **Version:** `1.3.1` in `package.json` and `.claude-plugin/plugin.json` at that
  commit. The tag points at this commit, so the pin is a release rather than a point
  past one.
- **Retrieved:** 2026-10-23
- **Licence:** MIT (the `LICENSE` file in the repository root)
- **Retrieval method:** folder copied from the release tarball, without `.git`
- **Integrity:** per-file SHA-256 of the whole set is in `../SOURCES.json`;
  `../verify-set.cmd` re-checks it

## Files

| File | Bytes |
|---|---|
| `SKILL.md` | 2931 |
| `LOGIC.md` | 6036 |
| `UI.md` | 6913 |
| `agents/openai.yaml` | 100 |

## Deviations

None: the folder is vendored as it is in the source. `LOGIC.md` and `UI.md` are the two
variants the skill chooses between. `agents/openai.yaml` is the provider companion the
source ships next to `SKILL.md`; DSH does not read it.

Local copy, no auto-update: refresh by hand when the source changes.
