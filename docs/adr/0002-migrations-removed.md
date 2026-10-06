# EF Core migrations were removed and the schema starts from scratch

The repository carried one migration, `20250504150709_InitialCommit`, describing a users table
built for an earlier version of the project. It was deleted instead of being carried through the
restructuring: the schema is about to be designed again, and a stale first migration could only be
kept alive by editing it by hand, which the rules forbid. The database was never the source of
truth here — it runs locally in a container and holds nothing worth preserving.

## Consequences

The application cannot create its tables until the first migration is generated, so nothing runs
against a fresh database yet; `WORKFLOW.md` says so. The old migration is still reachable in git
history, and this record exists so it is not restored by mistake.

The `snake_case` naming convention from the rules is not configured either. It belongs with the
first migration, so the choice shows up in the schema it produces rather than in a convention
nobody can see yet.
