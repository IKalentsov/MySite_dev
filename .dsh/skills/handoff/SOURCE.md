# Skill source: handoff

- **Repository:** [mattpocock/skills](https://github.com/mattpocock/skills)
- **Path in the repository:** `skills/productivity/handoff`
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
| `SKILL.md` | 894 |
| `agents/openai.yaml` | 141 |

## Deviations

None: the folder is vendored as it is in the source. `agents/openai.yaml` is the provider
companion the source ships next to `SKILL.md`; DSH does not read it.

## Notes

Thin by design in the source: the skill describes handing a session over to a file; it does
not depend on a non-vendored document.

Local copy, no auto-update: refresh by hand when the source changes.
