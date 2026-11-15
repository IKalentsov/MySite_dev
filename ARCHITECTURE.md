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
| Frontend build | Next.js (App Router), React, TypeScript | 16.4.0 / 19.3.0 / 6.0.3 |
| Frontend UI | Tailwind CSS 4, shadcn/ui copied into `packages/ui` | 4.3.3 |
| Frontend quality | ESLint 10 + typescript-eslint, Prettier, lefthook | 10.12.0 / 8.71.1 / 3.9.9 / 2.1.17 |

Backend versions are stated once, in `backend/Directory.Packages.props`. No `.csproj` carries a
`Version`, and nothing floats. Frontend versions are stated once, in the `catalog:` of
`frontend/pnpm-workspace.yaml`; every `package.json` references `catalog:`.

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
    ├── MySite.UnitTests/          domain rules, the seed pipeline, the locale fallback
    ├── MySite.IntegrationTests/   the seed command against a real PostgreSQL container
    └── MySite.ArchitectureTests/  dependency direction and naming rules, as executable checks
```

```
frontend/
├── pnpm-workspace.yaml          catalog: every version, plus workspace settings
├── package.json                 root scripts and the pnpm/Node pins
├── eslint.config.mjs            the root flat config
├── lefthook.yml                 pre-commit: lint on staged files, types over the project
├── .githooks/pre-commit         the hook Git runs; lefthook needs its config path passed in
├── apps/web/                    the Next.js App Router app
│   └── src/
│       ├── app/                 routes, providers, globals.css, [lang] layout and page
│       ├── _pages/              FSD pages: one screen per slice
│       ├── widgets/
│       ├── features/
│       ├── entities/
│       └── shared/
└── packages/
    ├── ui/                      design system: tokens, styles, cn, components
    ├── api-client/              skeleton; the generated client is tickets 11/15
    ├── shared/                  skeleton for shared types and utilities
    ├── eslint-config/           shared flat config: typed linting and FSD boundaries
    └── tsconfig/                base, Next.js and React library tsconfigs
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

## Frontend

Frontend dependencies point down the Feature-Sliced Design ladder and into the workspace packages:

```
apps/web/src/app → _pages → widgets → features → entities → shared
                                                              ↓
                          packages/ui, packages/api-client, packages/shared
```

- `apps/web/src/app` is the App Router route tree and holds no FSD `app` layer of its own. The FSD
  `pages` layer is `_pages`, because `src/pages` belongs to the Pages Router ([ticket 13](.scratch/site-development/issues/13-component-home-and-tokens.md)).
- A layer imports only the layers below it; slices inside one layer do not import each other.
  `packages/eslint-config` enforces this with `eslint-plugin-boundaries` and
  `import/no-restricted-paths`, so the rule is a gate rather than a convention.
- `packages/ui` is the design system: components, `cn`, tokens and styles. Its public API is
  `src/index.ts` (explicit named exports) plus `src/styles/globals.css`; the `exports` map and ESLint
  both reject deep imports.
- Tokens and the light/dark semantic variables live in `packages/ui/src/styles/globals.css` under
  Tailwind 4 `@theme`/`:root`/`.dark`, with `@custom-variant dark` and `@source` covering both
  `packages/ui` and `apps/web/src`. `apps/web/src/app/globals.css` only imports Tailwind and that
  file.
- `/` redirects to `/ru`; `/[lang]` is the only route, and `<html lang>` comes from that segment.
  The final route shape — status code, language switch, metadata — belongs to
  [ticket 12](.scratch/site-development/issues/12-public-route-shape.md).
- `packages/api-client` is a skeleton only: the generated client arrives in tickets 11/15 and is
  never imported into the UI directly.
- The gates are `pnpm lint`, `pnpm typecheck` and `pnpm build` from `frontend/`; the exact commands,
  the hook and CI are in `WORKFLOW.md`.

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
`OwnerProfileResponse`, `ProjectResponse` and `PaginatedProjectsResponse`. The Postgres project persists
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

Three levels, one project each. The unit project holds the domain rules, the seed pipeline
(`SeedContentService` against a substituted writer), the locale fallback and the shipped content
file. The integration project seeds through `SeedContentService` and `ContentSeedWriter` against a
real PostgreSQL container started by Testcontainers, and checks convergence on a rerun. The
architecture project turns the dependency direction above into executable checks.

| Level | Project | What it checks |
|---|---|---|
| Unit | `MySite.UnitTests` | Domain rules, the seed validator and parser, the locale fallback and the shipped content file |
| Integration | `MySite.IntegrationTests` | The seed path against a real PostgreSQL database, as the rules require — an in-memory provider is not a substitute |
| Architecture | `MySite.ArchitectureTests` | The dependency direction above, the ban on `async void` and the naming conventions |

The rules say a boundary without an executable check is a wish, so the architecture project is what
makes the direction in this document enforceable instead of advisory. Every rule is also applied to
`MySite.ArchitectureTests.NegativeFixture` — a deliberately broken assembly that must fail it —
because a check that has never been seen to fail is not a check.

The frontend's test contour is decided ([ticket 10](.scratch/site-development/issues/10-frontend-quality-and-test-contour.md):
Vitest, React Testing Library, MSW and Playwright) but not installed in this change: the first
tests land with tickets 15/16. The frontend gates that do exist are lint, typecheck and build.

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
- The first screen and its data boundary. The frontend shell exists and its gates are green, but
  the page is a placeholder: [ticket 16](.scratch/site-development/issues/16-the-page-itself.md)
  builds it and [ticket 18](.scratch/site-development/issues/18-data-fetching-boundary.md) decides
  where the data is fetched.
- The generated API client. `packages/api-client` is a skeleton; tickets 11/15 generate it and wire
  it to the UI.
- Frontend tests. The runner and levels are chosen in ticket 10 but no test runner is installed yet.
