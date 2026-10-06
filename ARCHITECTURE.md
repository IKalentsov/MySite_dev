# ARCHITECTURE.md

How this system is built: the stack, the projects, and the direction dependencies are allowed to
point. Product decisions — what the site does, its pages, its domain — do not live here.

Last updated: 2026-10-07.

## Stack

| Concern | Choice | Version |
|---|---|---|
| Runtime | .NET | `net10.0` |
| Web | ASP.NET Core Web API, controllers | framework from the SDK — `Microsoft.AspNetCore.App` 10.0.12 installed, no package pinned |
| Data | EF Core + Npgsql (PostgreSQL) | 10.0.12 / 10.0.3 |
| Auth | JWT bearer + BCrypt password hashing | 10.0.12 / 4.2.1 |
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
├── src/
│   ├── MySite.Domain/                    entities, value objects, domain rules
│   ├── MySite.Application/               use cases and the abstractions of the outside world
│   ├── MySite.Contracts/                 request and response DTOs of the API
│   ├── MySite.Infrastructure.Postgres/   EF Core, configurations, repositories, identity adapters
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
     │
     └──▶ MySite.Contracts
```

- `Domain` references nothing. BCL only: no packages, no project references.
- `Application` sees `Domain` and knows nothing about Postgres, HTTP or DI.
- `Infrastructure.Postgres` implements the abstractions `Application` declares.
- `Contracts` holds only DTOs and references nothing; it is where the API's request and response
  shapes live.
- `Web` is transport: controllers, DI composition, configuration. It is the only project that
  references `Infrastructure.Postgres` and `Contracts`.
- Tests point at production code; production code never points at tests.

## Current shape of the code

Two use cases exist, both about users: register and log in. The chain is
`UserController` → `UsersService` → (`IUsersRepository`, `IPasswordHasher`, `IJwtProvider`) with
the Postgres and identity adapters behind those interfaces.

The domain holds one entity, `User`, plus the `UserRight` enum and the `IAuditable` interface.
`Contracts` holds the two request DTOs, `LoginUserRequest` and `RegisterUserRequest`. The Postgres
project persists `UserEntity` through `MySiteDbContext` and `UserConfiguration`.

## Testing

Three levels, one project each, and all three are empty skeletons: no test has been written yet.

| Level | Project | What it will check |
|---|---|---|
| Unit | `MySite.UnitTests` | Domain rules and use cases with substituted dependencies |
| Integration | `MySite.IntegrationTests` | HTTP through the application against a real PostgreSQL database, as the rules require — an in-memory provider is not a substitute |
| Architecture | `MySite.ArchitectureTests` | The dependency direction above, as an executable check |

The rules say a boundary without an executable check is a wish, so the architecture project is
what makes the direction in this document enforceable instead of advisory — and it is the first
thing to fill once the slices above it settle.

## Known deviations from the harness rules

| Deviation | Why it stands | What would close it |
|---|---|---|
| JWT and password-hashing adapters live in `MySite.Infrastructure.Postgres`, although the project name says Postgres | Deliberate; see [ADR-0001](docs/adr/0001-single-infrastructure-project.md) | Split `MySite.Infrastructure.Identity` out, or rename the project to `MySite.Infrastructure` |
| Table and column names are not configured as `snake_case` | Deliberate; see [ADR-0002](docs/adr/0002-migrations-removed.md) | Add the convention together with the first migration |
| Login is a `GET` with a request body | Preserved from the original project to keep this change structural | Make it `POST` and set the HTTP contracts properly |
| Domain factories return a tuple (`(User user, string Error)`) instead of `Result<T>` | Preserved from the original project; adopting the Result pattern is a design change | Introduce `CSharpFunctionalExtensions` and move the failures into typed results |

## Known problems

Live findings about the system. A resolved finding leaves the list rather than being marked closed.
Product and process questions are not here; they live in `.dsh/AGENTS.md`.

- **Authentication is not wired.** `Program.cs` has no `AddAuthentication`, no `AddJwtBearer` and
  no `UseAuthentication`, and `JwtOptions` is never bound to configuration, so `JwtProvider` would
  sign with an empty key. The register and login endpoints exist but cannot serve a request. This
  is dormant by [ADR-0003](docs/adr/0003-public-site-first-phase.md) and gets wired together with
  the administration panel.
- **A failure is an exception, not a result.** `UsersRepository.GetByEmailAsync` throws when the
  user is missing and `UsersService.Login` throws on a wrong password, so a failed sign-in would
  become a 500 instead of a 401 once the endpoints are reachable. The Result pattern the rules
  require is not adopted.
- **There is no schema and no migration.** The application cannot create its tables, so nothing
  runs against a fresh database.

## What is deliberately absent

- Authentication. The first phase is a public site; the identity code exists but is not wired, and
  it is kept as the seed of the administration panel that comes later
  ([ADR-0003](docs/adr/0003-public-site-first-phase.md)).
- The administration panel: planned, not designed.
- Localisation. Russian is the primary language and English is a switchable alternative; how the
  switch is expressed in routes and content is not decided.
- Frontend code. `frontend/` is a reserved folder with a README; the stack is decided
  ([ADR-0004](docs/adr/0004-nextjs-app-router-and-fsd-frontend.md)) and the project is not
  started, so nothing about it appears in the tables above.
- EF Core migrations. They were removed on purpose and are generated again with the first schema.
- CI configuration.
- `CONTEXT.md`. It is the glossary of the project's own vocabulary, and the site has no product
  definition yet: writing terms now would mean inventing the product.
