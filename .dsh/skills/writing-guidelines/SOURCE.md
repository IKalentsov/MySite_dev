# Skill source: writing-guidelines

- **Repository:** [vercel-labs/agent-skills](https://github.com/vercel-labs/agent-skills)
- **Path in the repository:** `skills/writing-guidelines/`
- **Branch:** `main`
- **Commit:** `063bee94c3f4df8453406c830b0a7df0f2860278` (28.08.2026, "Merge pull request #328 … Update React View Transitions guidance and troubleshooting")
- **Retrieved:** 2026-09-21
- **Licence:** MIT (stated in the repository README; there is no separate `LICENSE` file in the repository root)
- **Retrieval method:** `web_fetch` + `write`; the file was verified against the git-blob-SHA1 from the GitHub API — size and SHA-1 matched byte for byte.

## What was vendored

1 file, 1,233 bytes.

| File | Bytes |
|---|---|
| `SKILL.md` | 1233 |

## Not vendored

- There are no binary or non-text files.
- There are no files that were not downloaded: the source directory `skills/writing-guidelines/` contains only `SKILL.md`.

## Notes

- `SKILL.md` and the frontmatter were not edited; the `name` field matches the directory name.

## Deviation: the rules are not vendored

`SKILL.md` keeps no rules locally. On every invocation it sends the agent to
`https://raw.githubusercontent.com/vercel-labs/writing-guidelines/main/command.md` — a
**different repository** from the one pinned above, and one this skill does not pin.

What that means, plainly:

- the skill needs network access to work at all;
- the commit above pins the wrapper, not the rules;
- the rules can change under a project without a single byte here changing.

Mitigation recorded on 2026-09-21, the retrieval date: the live file was fetched and hashed —
14228 bytes, SHA-256 `fb638d7821bb4472e4492aedcfb51f2636c7d31d34ff9f01cca5bcdce9b1841f`. Re-hash
the URL to learn whether the rules moved.

Vendoring the file was rejected on purpose: `SKILL.md` would have to be edited to read a local
copy, and vendored text is not edited in this base.

Local copy, no auto-update: refresh by hand when the source changes.
