---
name: architecture-drift-check
description: Compares the codebase with ARCHITECTURE.md and brings the architecture documentation up to date after changes to structure, contracts or stack.
whenToUse: When structure, contracts, the stack or the package set changed, or when you need to check that the code has not drifted from ARCHITECTURE.md.
---

# Architecture Drift Check

`ARCHITECTURE.md` is the single source of truth for how the system is built. This skill checks
that the document and the code tell the same story, and closes the gap.

## Who edits the document

`ARCHITECTURE.md` is edited by whoever owns the project's architecture. The change is agreed
with the user before the edit; the executor of a task does not rewrite the document.

## What to check

Concrete paths, layers and prohibitions come from the **active project** (`ARCHITECTURE.md`,
`.dsh/AGENTS.md`, `WORKFLOW.md`), not from memory:

- the actual project and folder layout matches `ARCHITECTURE.md`;
- the dependency direction between layers is intact;
- contracts, entities and value objects are described and match the code;
- the stack and package versions in the document match the package management files;
- a testing section exists; add one if it does not;
- the document mentions no technology the project does not have (a database, containers,
  a frontend and the like) until that is decided explicitly;
- files and folders named in the document exist; nothing extra is described.

## When to update

After any change to structure, contracts, the stack or the package set, update
`ARCHITECTURE.md` **in the same run**, without deferring it. Move the "last updated" date.

## Order

1. Read the project's `ARCHITECTURE.md`.
2. Read the actual structure (glob/grep) instead of relying on memory.
3. Compare and record the divergences as a list.
4. Update `ARCHITECTURE.md` (and the related process documents), stating the changes.
5. Service directories (`.git`, `.dsh`, `.vscode`, `.vs`, `bin`, `obj`, `node_modules`) are
   not walked without need — only when the check is impossible without them.

## Finding registry

Findings and open questions live in `ARCHITECTURE.md` (the "Known problems" and TODO
sections). A resolved finding is **struck out**, not marked "closed": the list must show only
what is still live.
