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

`docker-compose.yml` starts one PostgreSQL service on `localhost:5432` behind a named volume.
Docker 29.7.2 and Compose v5.3.1 are installed here. The `version:` key in that file is obsolete
for Compose v5 and only produces a warning.

The schema lives in generated migrations. Apply them with:

```powershell
dotnet ef database update -p src/MySite.Infrastructure.Postgres -s src/MySite.Web
```

`MySite.Web` carries `Microsoft.EntityFrameworkCore.Design` because `dotnet ef` needs it in the
startup project; in the infrastructure project the same package is `PrivateAssets="all"`, so it
does not flow onward. Migration files are declared as generated code by the `.editorconfig` beside
them, which keeps the analysers off code nobody wrote instead of silencing rules one by one.

## Run the API

```powershell
dotnet run --project src/MySite.Web
```

In Development the OpenAPI document is served and Scalar renders it. Not verified end to end yet:
the connection string in `appsettings.json` is a development placeholder and the authentication
pipeline is not wired.

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
