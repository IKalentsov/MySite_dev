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

There is a second, faster way to run the same tests: invoke the built test assembly directly.

```powershell
dotnet tests\MySite.UnitTests\bin\Debug\net10.0\MySite.UnitTests.dll
dotnet tests\MySite.ArchitectureTests\bin\Debug\net10.0\MySite.ArchitectureTests.dll
dotnet tests\MySite.IntegrationTests\bin\Debug\net10.0\MySite.IntegrationTests.dll
```

| Way | Who uses it | What it prints |
|---|---|---|
| `dotnet test MySite.slnx` | the owner and CI | one summary for the whole solution |
| direct assembly run | the agent, while working | per-assembly numbers (`Total`, `Failed`, `Succeeded`) and exit code 0 |

Both are correct; the direct run is what an executor uses because it gives exact numbers per project
and does not depend on the solution-wide runner.

**Today the three test projects are empty skeletons.** `dotnet test` then reports "zero tests" and
**exits with code 8**; a direct assembly run exits 0 with `Total: 0`. The 8 is the runner's "nothing
ran" code, not a failing test, and it goes away with the first test — after ticket 06 the criterion
is a plain one: exit code 0 with tests actually passing.

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

The connection string placeholder in `appsettings.json` (`ConnectionStrings:DefaultConnection` set
to `""`) causes a startup exception if no real value is provided — see the infrastructure layer's
`AddInfrastructure` extension for the check.

## Frontend

**There is no frontend yet.** `frontend/` holds a README and nothing else, so there are no frontend
commands in this file. They are written here by ticket 14
(`.scratch/site-development/issues/14-frontend-scaffold-and-gates.md`), together with the workspace
itself, and the toolchain is decided in tickets 09, 10 and 17.

The three commands the gates are expected to be named after — `pnpm lint`, `pnpm typecheck`,
`pnpm build`, all from `frontend/` — are **not written down as a decision here**: an executor that
needs them before ticket 14 is done must not invent them. Ticket 14 writes the real commands into
this section as part of its definition of done.

## Definition of done

1. For backend work, `dotnet build MySite.slnx` (inside the agent sandbox `dotnet build MySite.slnx
   -m:1`) is clean: no errors and no warnings.
2. `dotnet test MySite.slnx` is green, including the pre-existing tests. While the three test
   projects are still empty the runner reports "zero tests" and exits with code **8**; that is the
   runner's "nothing ran" code, not a failing test. A task is not done on that code — the first real
   tests arrive with ticket 06, and from then on done means exit code 0 with tests actually passing.
3. New behaviour is covered at the matching level: unit, integration, architecture.
4. No analyser is silenced to make the build pass. The finding is fixed, or the user decides on
   the suppression and it is recorded in the registry below.
5. Documentation reflects the change: `ARCHITECTURE.md` when the structure, a contract or the
   stack moved; `.dsh/AGENTS.md` when a project fact changed.
6. For frontend work, the gates named in the Frontend section above pass with no errors and no
   warnings, and the four mandatory screen states are implemented where a screen reads data.

The executor's side of this list — including the docker and migration commands and the exact
report format — is `.dsh/AGENT-TASKS.md`.

## Disabled-rule registry

Suppressions inherited from the harness base's `.globalconfig` (`backend/.globalconfig`) carry
their reason in that file. Project-specific suppressions are added here, one row each: rule,
reason, date, who decided.

| Rule | Reason | Date | Decided by |
|---|---|---|---|
| — | none added yet | — | — |

## Deviations from the general rules

A deliberate departure from a project rule is not kept quiet: it is written down here with a date,
the requirement that does not apply, the reason, the scope and the **condition for coming back**.
A departure without an entry here is not a decision. The executor's copy of this journal is in
`.dsh/AGENT-TASKS.md`, § 11.

| Date | Requirement that does not apply | Reason | Scope | Condition for coming back |
|---|---|---|---|---|
| 2026-10-07 | The "green tests" criterion cannot be checked: there are no tests, and `dotnet test` exits 8 on empty projects | The three test projects are empty skeletons, and on an empty set `Microsoft.Testing.Platform` reports the run as unsuccessful | Every task until ticket 06 | With the first test (ticket 06) done means exit code 0 with a non-zero number of tests |
| 2026-10-07 | The frontend gate commands (`pnpm lint`, `pnpm typecheck`, `pnpm build`) are not written down | There is no frontend in the repository yet: `frontend/` is a README | Frontend tasks until ticket 14 | Ticket 14 creates the pnpm workspace and writes the real commands into the Frontend section of this file |
| 2026-10-07 | `dotnet format --verify-no-changes` is not available | The command opens the workspace through MSBuild and needs restore, which the agent sandbox does not run | Every task that would check style | Does not come back: style is checked by the build (`EnforceCodeStyleInBuild=true`) |
| 2026-10-07 | `git fetch`/`git pull` from `origin` do not work | GitHub over SSH does not answer from this machine (`Permission denied (publickey)`) | Every task | The owner sets up SSH, or the remote moves to HTTPS |
