# Skill source: diagnosing-bugs

- **Repository:** [mattpocock/skills](https://github.com/mattpocock/skills)
- **Path in the repository:** `skills/engineering/diagnosing-bugs`
- **Branch:** `main`, commit `c55ee46073ed923f86ce59a5eb3b6d895095d1b7` (2026-09-18)
- **Version:** `1.2.3` in the `package.json` of that commit; the commit sits on `main` after
  the `v1.2.3` tag, which points at a different tree
- **Retrieved:** 2026-09-21
- **Licence:** MIT (the `LICENSE` file in the repository root)
- **Retrieval method:** folder copied from the repository tarball, without `.git`
- **Integrity:** per-file SHA-256 of the whole set is in `../SOURCES.json`; `../verify-set.cmd`
  re-checks it

## Files

| File | Bytes |
|---|---|
| `SKILL.md` | 8529 |
| `scripts/hitl-loop.template.sh` | 1316 |
| `agents/openai.yaml` | 103 |

## Deviations

None: the folder is vendored as it is in the source. `scripts/hitl-loop.template.sh` is a
template the skill fills in, not a script that runs on its own. `agents/openai.yaml` is the
provider companion the source ships next to `SKILL.md`; DSH does not read it.

Local copy, no auto-update: refresh by hand when the source changes.
