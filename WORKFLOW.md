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

**All three test projects hold tests, and a run is green.** `dotnet test MySite.slnx` exits with code
0 when every test passes: the unit project covers the domain rules, the seed pipeline and the locale
fallback; the architecture project checks the dependency direction and the naming conventions, and
runs each rule against a deliberately broken fixture assembly that must fail it; the integration
project starts its own PostgreSQL container through Testcontainers, so Docker has to be running for
the solution-wide run.

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

## Seed content

The site's texts live in `backend/content/site-content.json` and reach the database through a
command — not through an endpoint and not through a migration
([ADR-0007](docs/adr/0007-content-seeding-through-a-command.md)). Run it from `backend/`:

```powershell
dotnet run --project src/MySite.Web -- seed --file ../../content/site-content.json
```

The path is resolved against the process's current directory, never against the Web project's
content root. `dotnet run` starts the application with `src/MySite.Web` as its working directory, so
from `backend/` the file is `../../content/site-content.json`; an absolute path works as well. The
command is logged with the path it actually read, so a wrong path is visible before anything is
written. A rerun converges: the profile is created once and updated in place on later runs, projects
are matched by `link`, each project's translation rows and stack become exactly what the file says,
and a project missing from the file is reported but never deleted. Exit codes: `0` seeded, `1`
invalid input (the messages go to stderr), `2` database failure. The file's shape and rules are
documented in `backend/content/README.md`; the current texts are placeholders until ticket 07
replaces them.

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

Run from `frontend/`:

```powershell
pnpm install --frozen-lockfile
pnpm lint
pnpm typecheck
pnpm build
```

The workspace is a pnpm monorepo: `apps/web` (Next.js App Router) and the packages
`ui`, `api-client`, `shared`, `eslint-config`, `tsconfig`. Versions are stated once, in the
`catalog:` of `frontend/pnpm-workspace.yaml`; every `package.json` references `catalog:` and never
repeats a range. Node 24 or newer and pnpm 12.9.1 are expected (`engines.node`,
`devEngines.packageManager`), and the lockfile is `frontend/pnpm-lock.yaml`.

| Command | What it checks |
|---|---|
| `pnpm lint` | ESLint 10 flat config: typed linting (`parserOptions.projectService`), FSD layer and slice boundaries (`eslint-plugin-boundaries` plus `import/no-restricted-paths`), deep `@mysite/ui/*` imports, and no conflict with Prettier (`eslint-config-prettier/flat`). Runs without `--cache` |
| `pnpm typecheck` | `next typegen` and `tsc --noEmit` in every workspace package |
| `pnpm build` | `next build` for `apps/web`; it also type-checks the routes and writes the production bundle |
| `pnpm --filter @mysite/api-client generate` | regenerates the client types from `backend/docs/openapi.json`; see "Generated API client" below |
| `pnpm test` | the frontend tests: Vitest through the `@mysite/web` filter, then Playwright against `next start` on port 3100 |
| `pnpm --filter @mysite/web test:e2e` | Playwright only; the home page's data path needs the local stack, see "End-to-end tests" below |

`pnpm format` rewrites files with Prettier 3.9.9 and `pnpm format:check` reports instead of writing.
Formatting is not a gate of its own: ESLint only turns off the rules Prettier owns.

The Git hook is installed by `pnpm install`: `frontend/scripts/setup-git-hooks.mjs` points
`core.hooksPath` at `frontend/.githooks`, and its `pre-commit` runs lefthook with
`frontend/lefthook.yml`. Lint runs on the staged files; `tsc --noEmit` runs over the whole project
whenever a `.ts`/`.tsx` file is staged. The project-wide type check is deliberate — TypeScript
ignores `tsconfig.json` when files are passed on the command line — and `--cache` is deliberately
absent so typed and boundary rules do not reuse stale results (ticket 10). Lefthook looks for its
config at the Git root, which is the repository root, not `frontend/`; the committed hook therefore
passes `LEFTHOOK_CONFIG` explicitly instead of relying on `lefthook install`.

CI is `.github/workflows/frontend.yml`: pinned pnpm 12.9.1 and Node 24.18.0, then
`pnpm install --frozen-lockfile` → `pnpm --filter @mysite/api-client generate` → `git diff
--exit-code -- frontend/packages/api-client` → `pnpm lint` → `pnpm typecheck` → `pnpm build` →
install Chromium → `pnpm --filter @mysite/web test` → `pnpm --filter @mysite/web test:e2e`.

## End-to-end tests

The runner is Playwright 1.63.0, configured in `apps/web/playwright.config.ts`: one Chromium project,
`testDir: "./e2e"`, and a `webServer` that starts the **built** application with `next start` on port
3100 (not the dev server on 3000), reused locally and restarted in CI. The specs are:

| Spec | What it checks | What it needs |
|---|---|---|
| `e2e/home.spec.ts` | the smoke path: `/` redirects to `/ru` and `<html lang="ru">` is set | a build, nothing else |
| `e2e/page.spec.ts` | the page's real data path: the profile and three published projects, the unpublished project staying hidden, the Russian fallback on `/en` (ADR-0005), the locale switch, no horizontal scroll at 360 px, the theme following the system preference | a build, the API on `:5054` and the seeded database (ticket 08) |

The screen's four states are component-level facts and live in Vitest: `_pages/home/ui/home-page-view.test.tsx`
covers empty, success and both partial results, `src/__tests__/loading.test.tsx` the loading skeleton and
`src/__tests__/error-boundary.test.tsx` the error boundary with its retry. Playwright checks the data path
those components receive from the real API.

The data-path spec talks to the live API, so it needs the whole local stack:

```powershell
# from backend/
docker compose up -d
docker compose ps                                                              # mysite_postgres … Up
dotnet ef database update -p src/MySite.Infrastructure.Postgres -s src/MySite.Web
dotnet run --project src/MySite.Web -- seed --file ../../content/site-content.json
dotnet run --project src/MySite.Web                                            # API on http://localhost:5054

# from frontend/
pnpm build
pnpm --filter @mysite/web test:e2e
```

The API base URL is `API_BASE_URL` and defaults to `http://localhost:5054`; point it somewhere else
and the page renders its error state instead. Without the API the data-path spec fails.

In CI the three specs that read data from the API skip themselves (`CI` is set in the job), because
the workflow has no database and no API: CI runs the smoke spec and the responsive and theme specs,
which need no stack. The data path is verified locally with the commands above, as ticket 16 asked.
The skip is deliberate and lives in `e2e/page.spec.ts`; running the whole suite against the live
stack is what the local gate does.

## Generated API client

The frontend does not call the API with `fetch` and a hand-written URL: it imports
`@mysite/api-client`, a workspace package whose endpoint functions are typed from the backend's
OpenAPI document. Two artefacts are generated, both committed, and neither is edited by hand:

| Artefact | Generated by | Command |
|---|---|---|
| `backend/docs/openapi.json` | the backend build (`Microsoft.Extensions.ApiDescription.Server`) | below, from `backend/` |
| `frontend/packages/api-client/src/generated/openapi.d.ts` | `openapi-typescript` 7.13.0 | below, from `frontend/` |

`MySite.Web.csproj` keeps `OpenApiGenerateDocumentsOnBuild` **false**, so an ordinary build never
runs the app; the schema is taken by an explicit step:

```powershell
# Only has to be non-empty: the generator runs the app's entrypoint, and the
# infrastructure layer rejects an empty connection string. Nothing connects to a database.
$env:ConnectionStrings__DefaultConnection = "Host=127.0.0.1;Port=5433;Database=MySite;Username=build;Password=build"
dotnet build src/MySite.Web/MySite.Web.csproj -m:1 -p:OpenApiGenerateDocumentsOnBuild=true
```

The file lands in `backend/docs/openapi.json` and is deterministic for a fixed backend: two runs
over the same code produce the same bytes. It is also the same bytes on Linux and on Windows: the
compiler writes its XML documentation file with CRLF on Windows, those line breaks reach the
descriptions as `\r\n`, and Linux produces `\n` for the same text — so a target in `MySite.Web.csproj`
normalizes the generated document to `\n`. Without it the CI diff would fail on every platform except
the one that committed the file. Then, from `frontend/`:

```powershell
pnpm --filter @mysite/api-client generate
```

When the API contract moves, both artefacts are regenerated in the same change:

1. change the backend (controller, DTO, route) and build normally;
2. take the new schema with the command above and commit `backend/docs/openapi.json`;
3. regenerate the types and commit `frontend/packages/api-client/src/generated/openapi.d.ts`.

The package's public API is `src/index.ts` only — `createApiClient`, `getHome`, `ApiError` and the
response types. Deep imports fail both the `exports` map and `no-restricted-imports`. The client is
called from one place in the app: `_pages/home/api/get-home-content.ts`; ESLint's boundary policy
lets `pages` import `@mysite/api-client` and nothing else. The base URL comes from
`API_BASE_URL` and defaults to `http://localhost:5054`; requests go out with `cache: "no-store"`.

`src/generated/openapi.d.ts` is not linted: the root flat config ignores it in `globalIgnores`,
because it is generated rather than authored and cannot be fixed here. `tsc --noEmit` still compiles
it, and the CI regeneration diff keeps it current — the ignore takes nothing away from the gate.

CI keeps the two artefacts honest, and the two checks catch different mistakes:

| Check | Job | Catches |
|---|---|---|
| `pnpm --filter @mysite/api-client generate` then `git diff --exit-code -- frontend/packages/api-client` | `.github/workflows/frontend.yml` | the schema moved, the types were not regenerated |
| the schema step above, then `git diff --exit-code -- docs/openapi.json` from `backend/` | `.github/workflows/backend.yml` | the backend moved, the committed schema was not regenerated |

Neither check needs a database or a running API: the first reads the committed schema, the second
only needs a non-empty connection string.

## Definition of done

1. For backend work, `dotnet build MySite.slnx` (inside the agent sandbox `dotnet build MySite.slnx
   -m:1`) is clean: no errors and no warnings.
2. `dotnet test MySite.slnx` is green — exit code 0 with tests actually passing — including the
   pre-existing tests. The architecture rules are checked against their negative fixture assembly,
   and the integration tests run against a real PostgreSQL container rather than an in-memory fake.
3. New behaviour is covered at the matching level: unit, integration, architecture.
4. No analyser is silenced to make the build pass. The finding is fixed, or the user decides on
   the suppression and it is recorded in the registry below.
5. Documentation reflects the change: `ARCHITECTURE.md` when the structure, a contract or the
   stack moved; `.dsh/AGENTS.md` when a project fact changed.
6. For frontend work, `pnpm lint`, `pnpm typecheck`, `pnpm build`, `pnpm test` and — with the local
   stack up — `pnpm test:e2e` from `frontend/` pass with no errors and no warnings, and the four
   mandatory screen states are implemented where a screen reads data.
7. Generated artefacts follow the contract in the same change: `backend/docs/openapi.json` and the
   client types are regenerated, not edited by hand, and the two CI diff checks pass.

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
| 2026-10-07 | `dotnet format --verify-no-changes` is not available | The command opens the workspace through MSBuild and needs restore, which the agent sandbox does not run | Every task that would check style | Does not come back: style is checked by the build (`EnforceCodeStyleInBuild=true`) |

Resolved departures were removed; their history stays in git. The empty-test row was closed by
ticket 06, the frontend-gates row by ticket 14, and the inaccessible `git fetch` row by the working
SSH setup.
