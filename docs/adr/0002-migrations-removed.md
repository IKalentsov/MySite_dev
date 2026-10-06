# EF Core migrations were removed and the schema starts from scratch

The repository carried one migration, `20250504150709_InitialCommit`, describing a users table
built for an earlier version of the project. It was deleted instead of being carried through the
restructuring: the schema is about to be designed again, and a stale first migration could only be
kept alive by editing it by hand, which the rules forbid. The database was never the source of
truth here — it runs locally in a container and holds nothing worth preserving.

## Consequences

The first migration, `20261006231005_ContentSchema`, generates the content tables and applies the
`snake_case` naming convention (`EFCore.NamingConventions` package +
`UseSnakeCaseNamingConvention()` in `AddInfrastructure`). The application can now create its
schema on a fresh database. The old migration is still reachable in git history, and this record
exists so it is not restored by mistake.
