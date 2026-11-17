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
| API schema | `Microsoft.Extensions.ApiDescription.Server`, emitted at build time into `backend/docs/openapi.json` | 10.0.12 |
| Tests | xunit v3, AwesomeAssertions, NSubstitute, ArchUnitNET | 4.0.1 / 9.6.0 / 6.2.0 / 0.13.4 |
| Analysers | Roslynator, Meziantou, AsyncFixer, SonarAnalyzer | 5.0.1 / 3.0.294 / 2.1.0 / 10.35.0.4138 |
| Frontend build | Next.js (App Router), React, TypeScript | 16.4.0 / 19.3.0 / 6.0.3 |
| Frontend UI | Tailwind CSS 4, shadcn/ui copied into `packages/ui` | 4.3.3 |
| Frontend quality | ESLint 10 + typescript-eslint, Prettier, lefthook | 10.12.0 / 8.71.1 / 3.9.9 / 2.1.17 |
| Frontend API client | `openapi-typescript` (types) + `openapi-fetch` (typed fetch), generated from the committed schema | 7.13.0 / 0.17.0 |

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
├── docs/                        openapi.json: the API contract, generated at build time and committed
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
    ├── api-client/              the generated client: typed endpoint functions, public API in src/index.ts
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
- `/` redirects to `/ru`; `/[lang]` is the only route, and `<html lang>` comes from that segment. The
  segment is validated in one place (`shared/config/locales.ts`) by both the layout and the page, so an
  unsupported locale is a 404 instead of a 400 from the API. The locale switch is two plain links: the
  locale is the only state the page shares through its URL
  ([ticket 12](.scratch/site-development/issues/12-public-route-shape.md)).
- `packages/api-client` is the frontend's only way to the API: `openapi-typescript` turns the
  committed `backend/docs/openapi.json` into types, `openapi-fetch` wraps `fetch` with them, and
  `src/index.ts` exposes `createApiClient`, `getHome`, `ApiError` and the response types. Both
  generated artefacts — the schema and `src/generated/openapi.d.ts` — are committed and never edited
  by hand; CI regenerates each and fails on a non-empty `git diff`. Only the `pages` layer may
  import the package: the slice's server-side data function is the single caller and the ESLint
  boundary policy enforces it, so no component reaches the client directly. The base URL is
  `API_BASE_URL`, `http://localhost:5054` by default.
- One screen, one data boundary: `_pages/home/api/get-home-content.ts` calls `getHome` from the
  client and returns `{ profile, projects }`; the route renders the screen and the data path stays
  server-side (`cache: "no-store"`, no query library). `home-page.tsx` is the thin asynchronous
  Server Component, `home-page-view.tsx` is the synchronous view on props that decides the screen's
  four states, and `app/[lang]/loading.tsx` and `app/[lang]/error.tsx` are the route's loading and
  error files ([ticket 16](.scratch/site-development/issues/16-the-page-itself.md)).
- The screen is composed from slices below it, each exposing an `index.ts` as its public API: the
  `site-header`, `profile-about`, `project-list` and `contact-links` widgets, the `locale-switch`
  feature, and the `project` entity as a card. `shared/` holds what they all read — the supported
  locales, the page's own interface copy (`shared/i18n/home-text.ts`) and the placeholder contact
  links — so no slice imports a sibling of its own layer. The API's `Project` is passed straight into
  a card whose props name the fields it renders: the generated contract stays the single source of
  truth, while no layer below `pages` imports the API client.
- The gates are `pnpm lint`, `pnpm typecheck`, `pnpm build`, `pnpm test` and — with the local stack
  up — `pnpm test:e2e` from `frontend/`; the exact commands, their prerequisites, the hook and CI are
  in `WORKFLOW.md`.

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

The frontend's test contour is installed ([ticket 10](.scratch/site-development/issues/10-frontend-quality-and-test-contour.md),
[ticket 20](.scratch/site-development/issues/20-frontend-test-contour.md)): Vitest 5.0.3 with React
Testing Library and MSW in `apps/web`, and Playwright 1.63.0 in `apps/web/e2e`. Component tests cover
the synchronous view and the route's loading and error files, MSW covers the API client, and Playwright
drives the server-rendered page against the real local API. The asynchronous Server Component itself is
tested by no runner, which is why its data path is an end-to-end test. The frontend gates are the
generated-client diff, lint, typecheck, build, `test` and `test:e2e`.

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
- The visual design. The page follows the design system's tokens with minimal typography and layout;
  the owner's references, and with them the real visual language, arrive as a separate step.
- Contact data. The contact block is two placeholder links in `shared/config/contact-links.ts`, because
  no endpoint serves contact data yet; they are replaced together with the owner's texts (ticket 07)
  and by the contact form when it is specified.
- A real English text. The route, the per-locale tables and the fallback in code are in place
  ([ADR-0005](docs/adr/0005-bilingual-content-storage.md)); what English shows today is the seed
  placeholder, and the Harness project deliberately has no translation so the fallback stays covered.
