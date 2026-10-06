# Skill source: setup-matt-pocock-skills

- **Repository:** [mattpocock/skills](https://github.com/mattpocock/skills)
- **Path in the repository:** `skills/engineering/setup-matt-pocock-skills`
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
| `SKILL.md` | 6841 |
| `domain.md` | 2033 |
| `issue-tracker-github.md` | 3731 |
| `issue-tracker-gitlab.md` | 3809 |
| `issue-tracker-local.md` | 1810 |
| `triage-labels.md` | 1045 |
| `agents/openai.yaml` | 152 |

## Deviations

None: the folder is vendored as it is in the source. `agents/openai.yaml` is the provider
companion the source ships next to `SKILL.md`; DSH does not read it.

## Notes

A one-off setup skill: it prepares a repository for the upstream process and writes
`AGENTS.md`, not `CLAUDE.md`. It is not part of day-to-day work, and a project that already
has its own `.dsh/AGENTS.md` does not need it.

Local copy, no auto-update: refresh by hand when the source changes.
