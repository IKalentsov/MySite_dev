# ARCHITECTURE.md

How this system is built: the stack, the projects, and the direction dependencies are allowed to
point. Product decisions — what the site does, its pages, its domain — do not live here.

Last updated: 2026-11-16.

## Stack

| Concern | Choice | Version |
|---|---|---|
| Runtime | .NET | `net10.0` |
| Web | ASP.NET Core Web API, controllers | framework from the SDK — `Microsoft.AspNetCore.App` 10.0.12 installed, no package pinned |
| Data | EF Core + Npgsql (PostgreSQL) + `EFCore.NamingConventions` | 10.0.12 / 10.0.3 / 10.0.1 |
| API docs | `Microsoft.AspNetCore.OpenApi` + Scalar | 10.0.12 / 2.17.14 |
| Tests | xunit v3, AwesomeAssertions, NSubstitute, ArchUnitNET | 4.0.1 / 9.6.0 / 6.2.0 / 0.13.4 |
| Analysers | Roslynator, Meziantou, AsyncFixer, SonarAnalyzer | 5.0.1 / 3.0.294 / 2.1.0 / 10.35.0.4138 |

Versions are stated once, in `backend/Directory.Packages.props`. No `.csproj` carries a
`Version`, and nothing floats.

## Projects

```
backend/
├── MySite.slnx
├── Directory.Build.props        TFM, nullable, analysers, warnings-as-errors
├── Directory.Packages.props     every package version
├── .globalconfig                analyser severities
├── .gitignore                   build output and files holding real secrets
├── global.json                  test runner mode
├── docker-compose.yml           PostgreSQL for local development
├── content/                     the content file the seed command reads, and its README
├── src/
│   ├── MySite.Domain/                    entities, value objects, domain rules
│   ├── MySite.Application/               use cases and the abstractions of the outside world
│   ├── MySite.Contracts/                 request and response DTOs of the API
│   ├── MySite.Infrastructure.Postgres/   EF Core, configurations, repositories
│   └── MySite.Web/                       controllers, DI composition, appsettings
└── tests/
    ├── MySite.UnitTests/          empty skeleton
    ├── MySite.IntegrationTests/   empty skeleton
    └── MySite.ArchitectureTests/  empty skeleton
```

## Dependency direction

Decisions that are hard to reverse are recorded in [`docs/adr/`](docs/adr/).

```
MySite.Web ──▶ MySite.Infrastructure.Postgres ──▶ MySite.Application ──▶ MySite.Domain
     │                                                  │
     └───────────▶ MySite.Contracts ◀───────────────────┘
```

- `Domain` references nothing. BCL only: no packages, no project references.
- `Application` sees `Domain` and `Contracts`, and knows nothing about Postgres or HTTP.
- `Infrastructure.Postgres` implements the abstractions `Application` declares.
- `Contracts` holds only DTOs and references nothing; it is where the API's request and response
  shapes live. The use cases answer in those shapes and `Web` serializes them, so both reference it.
- `Web` is transport: controllers, DI composition, configuration. It is the only project that
  references `Infrastructure.Postgres`, and it is where the use cases' answers become HTTP
  responses.
- Tests point at production code; production code never points at tests.

## Current shape of the code

The first use case is content delivery: `ContentController` → `IContentService` (`ContentService` in
`Application`) → `IContentRepository` (implemented in the Postgres project) with domain entities in
between. The controller resolves the request and turns the answer into an HTTP response; what the
page contains — which projects are shown, which text they show, where the next page begins and how
large a page may be — is decided in `ContentService`. The identity chain (user controller, user
service, and their adapters) was removed in ticket 01.

The second use case is content seeding, and it is deliberately not an HTTP one. `Program.cs`
dispatches the `seed` verb before the web host starts serving: it reads the file named by `--file`,
resolves `ISeedContentService`, and turns the result into an exit code (`0` seeded, `1` invalid
input, `2` database failure). `SeedContentService` (in `Application`) parses and validates the JSON —
`ContentSeedParser` and `ContentSeedValidator` are pure functions with no I/O, no clock and no
database — and hands a `ContentSeedPlan` to `IContentSeedWriter`, which `Infrastructure.Postgres`
implements as one transaction: the profile is created when the table is empty and updated in place
otherwise, each project is matched by its `link`, its translation rows become exactly the file's
locales and its stack exactly the file's list, and projects absent from the file are reported but
never deleted ([ADR-0007](docs/adr/0007-content-seeding-through-a-command.md)).

The domain holds content entities — `OwnerProfile`, `Project`, their translations, and stack items —
plus the `Locale` value object and the `IAuditable` interface. `Contracts` holds the response DTOs —
`OwnerProfileResponse`, `ProjectResponse` and `ProjectListResponse`. The Postgres project persists
entities through `MySiteDbContext` with per-entity configurations discovered by assembly scanning.

Composition lives in layer-specific DI extensions: `AddProgramDependencies` (the entry point called
from `Program.cs`) chains `AddApplication`, `AddWebDependencies` and `AddInfrastructure`;
`Program.cs` itself contains only the middleware pipeline, controller routing, and health check
wiring. Health checks are split by tags — `/health/live` for liveness, `/health/ready` for readiness (Postgres reachability).

The public side is one controller. `ContentController` serves `GET /api/v1/profile` and
`GET /api/v1/projects`, both taking an explicit `locale` query parameter — `ru` by default, `en`
when asked for, and a 400 listing the supported values for anything else. Responses are
`MySite.Contracts` records, so no domain type reaches the wire, and a project with no text in any
locale is left out rather than rendered empty.

Every response carries an `X-Correlation-Id`, taken from the request when the caller sent one and
generated otherwise, and every error is an RFC 7807 problem document. `/health/live` answers
whether the process is up; `/health/ready` answers whether the database can be reached — through
`IDatabaseProbe`, which `Application` declares and `Infrastructure.Postgres` implements, so `Web`
never touches the context.

## Content model

The public page reads two things: the owner's profile and the published projects. Every text exists
per locale ([ADR-0005](docs/adr/0005-bilingual-content-storage.md)), so a translatable field lives
in a translation table while everything language-independent stays in the main table.

| Table | Holds |
|---|---|
| `owner_profiles` | The single profile, with its creation and update timestamps |
| `owner_profile_translations` | One row per locale: `headline`, `about`. Unique on `(owner_profile_id, locale)` |
| `projects` | One row per project: `link`, `year`, `sort_order`, `is_published`, timestamps |
| `project_translations` | One row per locale: `title`, `summary`. Unique on `(project_id, locale)` |
| `project_stack_items` | A project's technologies, one row each, ordered by `position` |

Reads go through `IContentRepository`, which the Application layer owns and the Postgres project
implements. Every read is untracked, and the locale is resolved by `TranslationFor(locale)` on the
entity: the requested locale first, the default locale's text second, nothing third.

The content itself lives in `backend/content/site-content.json` and reaches the tables through the
seed command, never through a migration and never through the API: content is data of the product,
so it is edited as a reviewed file and written by a command that converges on rerun. A locale key
the file omits is the deliberate "untranslated" state — no row is written, and `TranslationFor`
answers with the default language's text. `backend/content/README.md` documents the file's shape.

The schema is created by the project's only migration, generated with `dotnet ef migrations add`.

## Testing

Three levels, one project each. The unit project holds the first tests — the seed pipeline
(`SeedContentService` against a substituted writer) and the shipped content file; the integration
and architecture projects are still empty skeletons.

| Level | Project | What it checks |
|---|---|---|
| Unit | `MySite.UnitTests` | Domain rules and use cases with substituted dependencies; today the seed validator, the parser and the shipped content file |
| Integration | `MySite.IntegrationTests` | HTTP through the application against a real PostgreSQL database, as the rules require — an in-memory provider is not a substitute |
| Architecture | `MySite.ArchitectureTests` | The dependency direction above, as an executable check |

The rules say a boundary without an executable check is a wish, so the architecture project is
what makes the direction in this document enforceable instead of advisory — and it is the first
thing to fill once the slices above it settle.

## Known deviations from the harness rules

| Deviation | Why it stands | What would close it |
|---|---|---|
| The content entities have no `Result<T>` factories | Deliberate: they are read models with no expected business errors, so a result type would carry a failure that cannot happen. The pattern earns its place where failures exist | Introduce it together with the admin panel's first write use case |
| The content entities use plain `Guid` identifiers, not strongly-typed ones | Deliberate: the aggregates never reference each other by identifier, so the pattern would prevent a mix-up that cannot happen here | Introduce strongly-typed ids when one aggregate starts referencing another by id |

## Known problems

Live findings about the system. A resolved finding leaves the list rather than being marked closed.
Product and process questions are not here; they live in `.dsh/AGENTS.md`.

- **A page can be shorter than `pageSize`.** A project with no text in the requested locale and
  none in the default one is dropped after the page has been read, so the page size and `hasMore`
  describe stored rows rather than shown ones. Moving that filter into the query would close it.

## What is deliberately absent

- Authentication. There is no identity code in this repository: the stack was removed entirely
  ([ADR-0006](docs/adr/0006-identity-stack-removed.md)). It will be written from scratch when the
  admin panel's first write scenario arrives.
- The administration panel: planned, not designed.
- Localisation. Russian is the primary language and English is a switchable alternative; how the
  switch is expressed in routes and content is not decided.
- Frontend code. `frontend/` is a reserved folder with a README; the stack is decided
  ([ADR-0004](docs/adr/0004-nextjs-app-router-and-fsd-frontend.md)) and the project is not
  started, so nothing about it appears in the tables above.
- CI configuration.
