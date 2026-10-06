# Skill source: explain-interface

- **Repository:** [jakubkrehel/skills](https://github.com/jakubkrehel/skills)
- **Path in the repository:** `skills/explain-interface`
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
| `SKILL.md` | 9898 |
| `find-the-effect.md` | 5020 |
| `from-an-image.md` | 3448 |
| `no-browser.md` | 3880 |
| `read-the-system.md` | 7871 |
| `agents/openai.yaml` | 160 |

## Deviations

None: the folder is vendored as the source has it. `agents/openai.yaml` is the provider
companion the source ships next to `SKILL.md` and carries the Codex half of the invocation
switch (`policy.allow_implicit_invocation`); DSH does not read it.

## Notes

**User-invoked** (`disable-model-invocation: true`): studying an interface someone else built
is only ever something a person asks for, so a URL pasted into a message does not by itself
start an analysis.

Relevant to safety: the skill treats a fetched page as **untrusted input** and separates what
was measured from what was derived and from what was inferred. That is the right posture for
reading someone else's markup, and it is the same posture this base asks for when it fetches
anything from the web.

Local copy, no auto-update: refresh by hand when the source changes.
