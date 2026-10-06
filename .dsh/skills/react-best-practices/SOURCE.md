# Skill source: react-best-practices

- **Repository:** [vercel-labs/agent-skills](https://github.com/vercel-labs/agent-skills)
- **Path in the repository:** `skills/react-best-practices/`
- **Branch:** `main`
- **Commit:** `063bee94c3f4df8453406c830b0a7df0f2860278` (28.08.2026, "Merge pull request #328 … Update React View Transitions guidance and troubleshooting")
- **Retrieved:** 2026-09-21
- **Licence:** MIT (stated in the repository README and in the `license` field of `SKILL.md`; there is no separate `LICENSE` file in the repository root)
- **Retrieval method:** `web_fetch` + `write`; every file was verified against the git-blob-SHA1 from the GitHub API. The files `SKILL.md`, `README.md`, `metadata.json` and `rules/**` were retrieved without edits — size and SHA-1 matched byte for byte.

## What was vendored

75 files, 122,122 bytes.

| File | Bytes |
|---|---|
| `SKILL.md` | 7251 |
| `README.md` | 3360 |
| `metadata.json` | 921 |
| `rules/_sections.md` | 1554 |
| `rules/_template.md` | 631 |
| `rules/advanced-effect-event-deps.md` | 1802 |
| `rules/advanced-event-handler-refs.md` | 1483 |
| `rules/advanced-init-once.md` | 958 |
| `rules/advanced-use-latest.md` | 1072 |
| `rules/async-api-routes.md` | 1125 |
| `rules/async-cheap-condition-before-await.md` | 1220 |
| `rules/async-defer-await.md` | 2208 |
| `rules/async-dependencies.md` | 1293 |
| `rules/async-parallel.md` | 654 |
| `rules/async-suspense-boundaries.md` | 2510 |
| `rules/bundle-analyzable-paths.md` | 2382 |
| `rules/bundle-barrel-imports.md` | 2795 |
| `rules/bundle-conditional.md` | 949 |
| `rules/bundle-defer-third-party.md` | 920 |
| `rules/bundle-dynamic-imports.md` | 791 |
| `rules/bundle-preload.md` | 1149 |
| `rules/client-event-listeners.md` | 1969 |
| `rules/client-localstorage-schema.md` | 1950 |
| `rules/client-passive-event-listeners.md` | 1644 |
| `rules/client-swr-dedup.md` | 1159 |
| `rules/js-batch-dom-css.md` | 3266 |
| `rules/js-cache-function-results.md` | 1949 |
| `rules/js-cache-property-access.md` | 532 |
| `rules/js-cache-storage.md` | 1651 |
| `rules/js-combine-iterations.md` | 753 |
| `rules/js-early-exit.md` | 1133 |
| `rules/js-flatmap-filter.md` | 1411 |
| `rules/js-hoist-regexp.md` | 1028 |
| `rules/js-index-maps.md` | 837 |
| `rules/js-length-check-first.md` | 1747 |
| `rules/js-min-max-loop.md` | 2290 |
| `rules/js-request-idle-callback.md` | 2647 |
| `rules/js-set-map-lookups.md` | 532 |
| `rules/js-tosorted-immutable.md` | 1782 |
| `rules/rendering-activity.md` | 564 |
| `rules/rendering-animate-svg-wrapper.md` | 1185 |
| `rules/rendering-conditional-render.md` | 980 |
| `rules/rendering-content-visibility.md` | 815 |
| `rules/rendering-hoist-jsx.md` | 1039 |
| `rules/rendering-hydration-no-flicker.md` | 2308 |
| `rules/rendering-hydration-suppress-warning.md` | 872 |
| `rules/rendering-resource-hints.md` | 2549 |
| `rules/rendering-script-defer-async.md` | 1868 |
| `rules/rendering-svg-precision.md` | 588 |
| `rules/rendering-usetransition-loading.md` | 2074 |
| `rules/rerender-defer-reads.md` | 973 |
| `rules/rerender-dependencies.md` | 824 |
| `rules/rerender-derived-state.md` | 728 |
| `rules/rerender-derived-state-no-effect.md` | 1201 |
| `rules/rerender-functional-setstate.md` | 2968 |
| `rules/rerender-lazy-state-init.md` | 2016 |
| `rules/rerender-memo.md` | 1148 |
| `rules/rerender-memo-with-default-value.md` | 1173 |
| `rules/rerender-move-effect-to-event.md` | 1268 |
| `rules/rerender-no-inline-components.md` | 2135 |
| `rules/rerender-simple-expression-in-memo.md` | 1018 |
| `rules/rerender-split-combined-hooks.md` | 1844 |
| `rules/rerender-transitions.md` | 1055 |
| `rules/rerender-use-deferred-value.md` | 1846 |
| `rules/rerender-use-ref-transient-values.md` | 1742 |
| `rules/server-after-nonblocking.md` | 2012 |
| `rules/server-auth-actions.md` | 2649 |
| `rules/server-cache-lru.md` | 1353 |
| `rules/server-cache-react.md` | 2228 |
| `rules/server-dedup-props.md` | 2060 |
| `rules/server-hoist-static-io.md` | 4423 |
| `rules/server-no-shared-module-state.md` | 1766 |
| `rules/server-parallel-fetching.md` | 1554 |
| `rules/server-parallel-nested-fetching.md` | 992 |
| `rules/server-serialization.md` | 996 |

## Not vendored

- There are no binary or non-text files inside the skill.
- `AGENTS.md` — a compiled version of the same rules (`SKILL.md` + `rules/**` in a single build, 108,261 bytes), not vendored: it duplicates `rules/` and is available at the source
  <https://raw.githubusercontent.com/vercel-labs/agent-skills/main/skills/react-best-practices/AGENTS.md>.

## Notes

- `SKILL.md` and the frontmatter were not edited: the `name` field is `vercel-react-best-practices` and does not match the directory name `react-best-practices` (that is how it is in the source). Because of this, `scripts/verify-library.ps1` reports `frontmatter name does not match the folder`.
- In the "Full Compiled Document" section, `SKILL.md` refers to `AGENTS.md`, which is deliberately not vendored (see above).

Local copy, no auto-update: refresh by hand when the source changes.
