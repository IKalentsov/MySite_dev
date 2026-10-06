# WORKFLOW.md

Build, test and run commands for this repository, and what "done" means here. Every command below
was run in this environment.

## Build

Run from `backend/`:

```powershell
dotnet build MySite.slnx
```

Inside an agent sandbox add `-m:1`: a multi-process build opens named pipes, which the sandbox
blocks.

The solution file is `MySite.slnx` — the XML solution format, not `.sln`. It needs the .NET 10 SDK
or Visual Studio 2022 17.13 and later; an older Visual Studio cannot open it.

The build is clean by construction. `Directory.Build.props` sets `TreatWarningsAsErrors` with
`AnalysisMode=All` and four analyser packs (Roslynator, Meziantou, AsyncFixer, SonarAnalyzer), so
any finding is an error and there is no "builds with warnings" state.

## Test

Run from `backend/` **only**. `backend/global.json` is what selects the test runner
(`Microsoft.Testing.Platform`); running `dotnet test` from the repository root finds no
`global.json` and fails with "Testing with VSTest target is no longer supported by
Microsoft.Testing.Platform on .NET 10 SDK and later".

```powershell
dotnet test MySite.slnx
```

The three test projects are empty skeletons, so today this reports "zero tests" and exits 5. That
code means the runner ran nothing, not that a test failed; it goes away with the first test.

## Database

```powershell
docker compose up -d
docker compose down
```

`docker-compose.yml` starts one PostgreSQL service behind a named volume, shaped after the
DirectoryService project's compose file: the credentials come from `backend/.env` (untracked;
`.env.example` is the template), the container restarts itself, and the volume is mounted at the
image's own `PGDATA` rather than at a guessed path. Two differences from that template are
deliberate and commented in the file: no `container_name`, which would collide with another
project's container of that name, and a pinned image tag, because the volume layout belongs to a
PostgreSQL major version.

**It listens on `localhost:5433`, not 5432.** A native PostgreSQL service is installed on this
machine and owns port 5432, and Windows resolves `localhost` to `::1` first — a container published
on 5432 is never reached, and the application silently talks to the local server instead. That is
also why the template picks its own port.

The schema lives in generated migrations. Apply them with:

```powershell
dotnet ef database update -p src/MySite.Infrastructure.Postgres -s src/MySite.Web
```

The connection string lives in `src/MySite.Web/appsettings.Development.json`, which git ignores, so
no credential is committed. `dotnet ef` reads the launch profile, which sets the Development
environment, so it finds the same file.

`MySite.Web` carries `Microsoft.EntityFrameworkCore.Design` because `dotnet ef` needs it in the
startup project; in the infrastructure project the same package is `PrivateAssets="all"`, so it
does not flow onward. Migration files are declared as generated code by the `.editorconfig` beside
them, which keeps the analysers off code nobody wrote instead of silencing rules one by one.

## Run the API

```powershell
dotnet run --project src/MySite.Web
```

It listens on `http://localhost:5054`. `src/MySite.Web/Content.http` holds a request for every
endpoint and for both health probes. In Development the OpenAPI document is at
`/openapi/v1.json` and Scalar renders it at `/scalar/v1`.

The account endpoints are still not reachable: the authentication pipeline is not wired, which is
deliberate for the first phase ([ADR-0003](docs/adr/0003-public-site-first-phase.md)).

## Definition of done

1. `dotnet build MySite.slnx` is clean: no errors and no warnings.
2. `dotnet test MySite.slnx` is green, including the pre-existing tests.
3. New behaviour is covered at the matching level: unit, integration, architecture.
4. No analyser is silenced to make the build pass. The finding is fixed, or the user decides on
   the suppression and it is recorded in the registry below.
5. Documentation reflects the change: `ARCHITECTURE.md` when the structure, a contract or the
   stack moved; `.dsh/AGENTS.md` when a project fact changed.

## Disabled-rule registry

Suppressions inherited from the harness base's `.globalconfig` (`backend/.globalconfig`) carry
their reason in that file. Project-specific suppressions are added here, one row each: rule,
reason, date, who decided.

| Rule | Reason | Date | Decided by |
|---|---|---|---|
| — | none added yet | — | — |
