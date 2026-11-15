# Skill source: retro

- **Repository:** [mattpocock/skills](https://github.com/mattpocock/skills)
- **Path in the repository:** `skills/engineering/retro`
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
| `SKILL.md` | 4369 |
| `agents/openai.yaml` | 146 |

## Deviations

None: the folder is vendored as it is in the source. `agents/openai.yaml` is the provider
companion the source ships next to `SKILL.md`; DSH does not read it.

## Notes

Reads the session, not the code: every suggestion is about the agent environment. A
mechanical rule violation is turned into a check (linter rule, hook, CI job) rather than
into prose, which is what keeps a project standard from rotting into advice.

Local copy, no auto-update: refresh by hand when the source changes.
