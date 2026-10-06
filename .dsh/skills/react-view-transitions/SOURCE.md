# Skill source: react-view-transitions

- **Repository:** [vercel-labs/agent-skills](https://github.com/vercel-labs/agent-skills)
- **Path in the repository:** `skills/react-view-transitions/`
- **Branch:** `main`
- **Commit:** `063bee94c3f4df8453406c830b0a7df0f2860278` (28.08.2026, "Merge pull request #328 … Update React View Transitions guidance and troubleshooting")
- **Retrieved:** 2026-09-21
- **Licence:** MIT (stated in the repository README; there is no separate `LICENSE` file in the repository root)
- **Retrieval method:** `web_fetch` + `write`, and for six files (`metadata.json`, `README.md`, `references/css-recipes.md`, `references/implementation.md`, `references/nextjs.md`, `references/patterns.md`) — a direct `curl -sSL --fail` download, because `web_fetch` corrupted the content while decoding (the size came out 1–177 bytes smaller, and the SHA-1 did not match). Every file was verified against the git-blob-SHA1 from the GitHub API — size and SHA-1 matched byte for byte.

## What was vendored

8 files, 62,688 bytes.

| File | Bytes |
|---|---|
| `SKILL.md` | 14407 |
| `README.md` | 2231 |
| `metadata.json` | 852 |
| `references/css-recipes.md` | 8337 |
| `references/implementation.md` | 9280 |
| `references/nextjs.md` | 10700 |
| `references/patterns.md` | 11941 |
| `references/troubleshooting.md` | 4940 |

## Not vendored

- There are no binary or non-text files inside the skill itself.
- `AGENTS.md` — a compiled version of the same materials (`SKILL.md` + `references/**` in a single build, 58,993 bytes), not vendored: it duplicates the files already downloaded and is available at the source
  <https://raw.githubusercontent.com/vercel-labs/agent-skills/main/skills/react-view-transitions/AGENTS.md>.
- Next to the skill, the repository holds a binary artifact `skills/react-view-transitions.zip` (28,866 bytes) — a packed version of the same skill. It was not downloaded (a binary) and is not part of the local copy.

## Notes

- `SKILL.md` and the frontmatter were not edited: the `name` field is `vercel-react-view-transitions` and does not match the directory name `react-view-transitions` (that is how it is in the source). Because of this, `scripts/verify-library.ps1` reports `frontmatter name does not match the folder`.

Local copy, no auto-update: refresh by hand when the source changes.
