# AGENTS.md — project agent instructions

This file is the project's agent instructions. The **rules** come from the harness base and are
re-drafted from it, never invented. The project's own **facts** — stack, paths, versions, commands,
open questions — live in the "Project facts" section at the end, which a re-run of the base's
deployment preserves.

```
Harness base: IKalentsov/Harness @ 38dace02a4d6bc339f56867007a667eee48ff92c, adopted 2026-10-07
```

## Priority when this file and the repository disagree

1. **The base wins on rules.** These instructions replace any older agent configuration.
2. **The repository wins on facts.** Real paths, solution and layer names, package sets and
   exact versions, scripts and commands, domains, table names, design system, environment
   limits. These are not in the base by design and belong in the project-facts section.
3. **Missing fact ⇒ ask.** Never invent a version, a licence, a command, a file's contents or a
   project fact, and never leave a placeholder that looks like a decision.

## Shared

### Engineering discipline

- **One meaning, one place.** A skill, a principle or a template lives in one section; copies
  inside projects are consumables, never the source of truth.
- **Versions are pinned exactly.** No floating ranges and no `latest`.
- **A third-party licence stays third-party.** A vendored skill arrives with its source
  licence; the text is not rewritten. A divergence from the official line is written as an
  explicit deviation block with a reason and a date.
- **Keep the global layer minimal.** Model connections and session data only; rules and skills
  live in the base (sections) or in the project (`.dsh/`).

### Skills: which ones the agent reaches on its own

The catalog of skill descriptions sits in the session from the start, so a skill is chosen by
matching that catalog against the task. The library is large; the matching is the work.

- **On a task inside a skill's scope, the skill is loaded.** Read the catalog, take the one
  whose description matches the task, and follow it.
- **A user-invoked skill is reached by pointer.** Skills carrying
  `disable-model-invocation: true` never enter the catalog; the `skill` tool cannot reach them —
  an instruction that names the skill's file by path (`.dsh/skills/<name>/SKILL.md`) can. An
  instruction that names a user-invoked skill by bare name alone does nothing.
- **One skill per task.** A second skill is a separate task.

DSH reads exactly one level (`skills/<name>/SKILL.md`) and loads a skill in two steps: the
`description` (with `whenToUse` when the skill has one) enters the session catalog, and only the
skill a task actually needs is read in full.

| Frontmatter | Effect |
|---|---|
| neither key | The model sees the skill in the catalog and may invoke it on its own |
| `disable-model-invocation: true` | The skill is absent from the catalog and from the `skill` tool; a person opens it with `/name` |

The flag is upstream frontmatter and is not edited here.

| Section | Skills | User-invoked | Model-invoked |
|---|---|---|---|
| `shared/skills` | 26 | 14 | 12 |
| `backend/skills` | 4 | 0 | 4 |
| `frontend/skills` | 16 | 4 | 12 |
| **total** | **46** | **18** | **28** |

- **User-invoked in `shared` (14 of 26):** `ask-matt`, `grill-me`, `grill-with-docs`, `handoff`,
  `implement`, `improve-codebase-architecture`, `setup-matt-pocock-skills`, `teach`,
  `to-questionnaire`, `to-spec`, `to-tickets`, `triage`, `wait-what`, `wayfinder`.
- **Model-invoked in `shared` (12):** `architecture-drift-check`, `codebase-design`,
  `code-review`, `diagnosing-bugs`, `domain-modeling`, `grilling`, `karpathy-guidelines`,
  `prototype`, `research`, `resolving-merge-conflicts`, `tdd`, `writing-for-agents`.
- **`backend` hides nothing:** all 4 are model-invoked — `dotnet-webapi`,
  `optimizing-ef-core-queries`, `analyzing-dotnet-performance`,
  `create-datadriven-aspnetcore`. The four SQL Server skills the base ships were removed on the
  owner's decision: the database is PostgreSQL, not SQL Server.
- **User-invoked in `frontend` (4 of 16):** `break`, `explain-interface`, `interface-review`,
  `variant`.
- **Model-invoked in `frontend` (12):** `better-accessibility`, `better-colors`,
  `better-interface`, `better-layout`, `better-typography`, `better-ui`, `better-writing`,
  `composition-patterns`, `react-best-practices`, `react-view-transitions`,
  `web-design-guidelines`, `writing-guidelines`.

A section of forty skills costs forty short lines until one of them is used.

**Vocabulary of the source.** A vendored skill is not edited, so it speaks the vocabulary of the
tool it was written for: "the Skill tool" is DSH's `skill` tool, `/clear` and `/compact` are
context commands of the host, `/name` is a user-invoked skill. Read them as host concepts, not as
literals.

### Skill layout

- **Flat and exactly one level deep:** `skills/<name>/SKILL.md`, never
  `skills/<group>/<name>/...`. DSH reads exactly one level and would not find a nested layout.
- **No skill directory contains an `AGENTS.md`:** DSH reads that file as directory instructions
  and loads it into every session, duplicating `SKILL.md`.
- Frontmatter `name` and `description` are present, the directory name equals the `name` field,
  both kebab-case; a colon inside `description` or `whenToUse` is allowed only inside quotes — an
  unquoted `USE WHEN:` breaks the frontmatter and the provider skips the file without a word.
- A skill's resources live inside its own directory and are linked relatively.
- Every vendored skill carries a `SOURCE.md` next to `SKILL.md`; the skill is never edited,
  frontmatter included.

## Backend

### Architecture and dependency direction

Clean Architecture, dependencies strictly inward:

```
Web ──▶ Infrastructure.* ──▶ Core (Application) ──▶ Domain ◀── Contracts
```

- `Domain` — entities, value objects, domain rules, invariants. BCL only: no packages, no
  references to other projects, no knowledge of a database, HTTP or DI.
- `Core` / `Application` — use cases, validators, abstractions of the outside world. Knows
  nothing about Infrastructure.
- `Infrastructure.*` — implementations of those abstractions: database, cache, external APIs.
  Split by technology.
- `Web` — transport: controllers, middleware, DI composition.
- `Contracts` — outbound DTOs; may lean on domain types.

The direction is enforced by architecture tests, not by agreement.

Solution layout:

```
backend/
├── Directory.Build.props      # TFM, analysers, TreatWarningsAsErrors
├── Directory.Packages.props   # every package version — here and nowhere else
├── .globalconfig              # analyser rule severities
├── global.json                # SDK pinning
├── tools/                     # build scripts
├── src/<Product>.<Layer>/     # one project per layer
└── tests/
    ├── <Product>.UnitTests/
    ├── <Product>.IntegrationTests/
    └── <Product>.ArchitectureTests/
```

Domain patterns (mandatory):

1. **Result Pattern.** Expected business errors are returned as values (`Result<T>`,
   `Result<T, Error>`); exceptions are reserved for exceptional situations such as an
   infrastructure failure. An error is typed and carries a code, not one string for everything.
2. **Value objects instead of primitives.** Money, identifiers, ratings, addresses are distinct
   types. Validation lives in the value object's factory, not in a controller or a service.
3. **Strongly-typed IDs** for aggregate identifiers.
4. **Entity factories.** `public static Result<T> Create(...)`; there are no public constructors
   facing outward and no setters that break invariants.
5. **Aggregates reference each other by ID.** No navigation collections across an aggregate
   boundary.
6. **Timestamps** for creation and update (UTC) on every persisted entity.
7. **Explicit input validation** at the use-case boundary, with a single error shape.

What counts as a design mistake:

- An anaemic model and "services that do everything", with the logic collected in one class.
- Public setters on entities.
- Logic inside controllers or ORM configurations.
- A use case that knows a concrete database or HTTP client directly.
- An abstraction added "for the future" with no second implementation and no test.

### Data and storage

> Applies when the project has a database. Having none is a legitimate project state, and then
> these rules simply do not activate.

**Data access**

- **An ORM for writes and typical reads, a micro-ORM for complex queries.** Heavy analytical and
  multi-table queries are written as explicit SQL rather than assembled from an ORM tree.
- **Reads are untracked.** Read queries use `AsNoTracking()`, and the projection into a DTO is
  built inside the query: "load the whole aggregate and dissect it in memory" is out.
- **Lazy loading is off.** Related data is loaded explicitly and only where it is needed.
- **Pagination is mandatory** on every list endpoint. Keyset pagination is preferred over
  offset: it does not degrade on deep pages.
- **The transaction belongs to the use case, not the repository.** Its boundary matches the
  boundary of a business operation.

**Schema and migrations**

- **snake_case** for tables and columns; creation and update timestamps (UTC) on every persisted
  entity.
- **Entity configuration lives in its own class** (`IEntityTypeConfiguration<T>`), discovered by
  assembly scanning; configurations are not smeared across `OnModelCreating`.
- **Migrations are generated by a command, never hand-written.** Migration files are not edited
  by hand; one migration covers one coherent change.
- **Indexes are mandatory** for filtered and sorted fields and for foreign keys. An index
  suggested by an analyser or a query plan is verified before it is created: read benefit
  against write cost, duplicates and overlaps.

**Cache and external stores**

- The cache serves expensive external responses, rate limiting and task idempotency.
- A key carries its domain and data-schema version (`<product>:v1:<entity>:{id}`): a format change
  must not read stale values.
- Invalidation is designed together with the key; "cache forever" is not a design.

**Test data**

- Integration tests run against a **real** database (a container), not an in-memory fake: an
  in-memory provider reproduces neither constraints, nor types, nor transactions.
- External responses are reproduced from fixtures; module tests make no network calls.
- State is isolated between tests: no test depends on the order of execution.

### API

**HTTP contract**

- **A thin controller:** validate input → call the use case → map the result to an HTTP response.
  No business logic in the controller.
- **Contracts are separate DTOs**, not domain entities: the domain does not leak outward.
- **One error shape** (ProblemDetails or a single error response type); no bare 500s and no stack
  traces on the wire.
- **Versioning from day one:** the `/api/v{n}` prefix.
- **Documentation comes from the code:** OpenAPI is generated, and an interactive page (Scalar or
  an equivalent) is available in the development environment.
- **Health checks are split:** `/health/live` — the process is alive; `/health/ready` — it can
  serve traffic (database, cache, external dependencies).
- **A correlation ID** is taken from the header or generated, and reaches every log line and the
  response.

**Outbound calls**

- Only through an HTTP client factory and typed clients; base addresses and keys come from
  configuration, never from code.
- Mandatory: a timeout, retries with backoff and jitter, a circuit breaker, a concurrency limit
  and respect for the remote side's rate limits.
- **A raw external response never reaches the domain.** It is first mapped into our own model,
  then validated, then logged as a fact and a result.
- Parsing and scraping live in their own layer, with fixtures in tests.
- **External API secrets** come from environment variables or user secrets only; in
  `appsettings.json` there are empty placeholders.
- An external failure does not travel through the layers as an exception: infrastructure catches
  the concrete exceptions and returns a typed error.

**Responsibility boundaries**

| Layer | Does | Does not |
|---|---|---|
| Controller | Accepts the request, validates the shape, returns a response | Hold domain rules |
| Use case | Orchestrates domain and infrastructure, owns the transaction | Know about HTTP |
| Domain | Rules and invariants | Know about a database, the network or DI |
| Infrastructure | Implements abstractions, absorbs the outside world's exceptions | Make business decisions |

### Observability and configuration

**Logs**

- **Structured logs** (Serilog or the built-in structured logger). The message template carries
  named properties, and values are passed as parameters.
- **String interpolation in log templates is forbidden:** it turns structure into a string and
  kills field-based search.
- **We log:** incoming requests (minus sensitive data), the outcome of a use case, failures of
  outbound calls with their context (provider, request, status, duration), and timing metrics.
- **We do not log:** tokens, keys, passwords, personal data, full external responses. Secrets
  that reach an exception object or a URL are masked.
- Levels are meaningful: `Error` needs a human, `Warning` is a deviation from the norm,
  `Information` is a lifecycle event. A "just in case" log line on every step is not written.

**Configuration**

- Settings go through typed option objects (`IOptions<T>`) bound to sections; reading by a string
  key deep inside the code is not practised.
- Sources: `appsettings.json` (structure and empty placeholders) →
  `appsettings.{Environment}.json` (environment values) → environment variables → user secrets
  (locally).
- **No secrets in the repository.** The file with real values is closed by `.gitignore`; a
  template without values (`.env.example`, `appsettings.Development.json` with placeholders)
  stays in the repository.
- **A missing or empty secret does not crash the application at start-up:** settings validation
  reports what is missing, and the application either continues in a limited mode or stops with a
  clear message — it never keeps running quietly in a wrong state.
- Configuration is validated at start-up: required fields, ranges, mutual dependencies.

**Errors and diagnosis**

- A log record answers three questions: what was being done, what was expected, what happened.
- One exception, one record. Re-logging at every layer is out: whoever handles the error logs it.
- Diagnostic messages for the user are separate from technical detail: the user gets a clear text
  and a next step, the log gets the technical context.

### Testing

**Levels**

| Level | What it checks | What it uses |
|---|---|---|
| Unit | Domain, value objects, validators, use cases with substituted dependencies | Fast, no network and no database |
| Integration | HTTP plus a real database and cache, an end-to-end scenario through the app | Containers, fixtures |
| Architecture | Dependency direction and structural conventions | Assembly inspection (ArchUnitNET or an equivalent) |

- **TDD where a rule is involved:** a failing test on the business rule first, then the
  implementation.
- **One test, one behaviour.** The name describes the scenario and the expected outcome.
- **Duplicated test bodies are removed by parameterisation** (`[Theory]` and the like), not by
  silencing an analyser rule.

**Architecture tests as fitness functions**

- Every architecture rule has an executable check: no upward references between layers, no
  `async void`, mandatory type suffixes, no forbidden dependencies.
- **A new boundary comes with a negative fixture:** an assembly that violates it, on which the
  check must fail. Without that, the check is not considered working.
- A rule without an executable check is a wish, not an architecture.

**Keeping the test contour clean**

- **References point from tests to code only.** Production projects know nothing about tests: no
  `InternalsVisibleTo`, no "test" branches in the code.
- A test that the design obstructs is a signal to fix the design (an explicit contract,
  dependency injection, a time abstraction), not to add a back door to production code.
- Module tests make no external calls: abstractions are substituted.
- Execution order does not affect the result; tests are isolated by state.

**Acceptance**

- The build is clean, with no errors **and no warnings**: under `TreatWarningsAsErrors` a warning
  is a build error.
- All tests are green, including the pre-existing ones: regressions are not acceptable.
- New functionality is covered at the matching level; an exception is granted by the user.
- Tests are run the way the project runs them: the commands come from the project's
  `WORKFLOW.md`, not from memory.

### Toolchain

**Centralised settings**

| File | Purpose |
|---|---|
| `Directory.Build.props` | TFM, `Nullable`, `ImplicitUsings`, analysers, `TreatWarningsAsErrors`, style rules at build time |
| `Directory.Packages.props` | **Every** package version; `.csproj` files carry `<PackageReference>` without `Version` |
| `.globalconfig` | Analyser rule severities (levels, not versions) |
| `global.json` | SDK version pinning and the test runner mode |
| `.gitignore` | Build output and files holding real secrets |

- **A package version is stated in exactly one place** — `Directory.Packages.props`. Editing a
  version in a `.csproj`, or adding a package outside the central file, is a mistake.
- **Versions are pinned exactly and deliberately.** Floating tags, ranges and `latest` are out. A
  version bump is a separate change, made after reading the release notes.
- Shared build properties are inherited by every project in the solution automatically; repeating
  them in individual `.csproj` files is unnecessary.

**Analysers and warnings**

- Enabled: general style and refactoring rules, security and correctness rules, async/await
  rules, and a code-quality and security analyser.
- `TreatWarningsAsErrors` plus the top analysis level is a deliberate mode: the build must be
  clean, not nearly clean.
- **Silencing an analyser is not a substitute for fixing the code.** The order of work on a
  finding:
  1. fix the code — the analyser is almost always right;
  2. suppression (`#pragma`, an attribute, an edit to `.globalconfig`) is not done on one's own;
  3. the user is told: the rule code, its exact text, the file and line, and whether the case is
     single or widespread. The decision is theirs.
- **The project keeps a registry of disabled rules:** the rule, the reason, the date, who
  decided. A suppression without a registry entry does not count as a decision.
- "The rule is absent from `.globalconfig`, therefore it is off" is **false**: the SDK enables
  hundreds of rules by default, and only a build reveals the truth.

**Building inside the agent environment**

- A multi-process build uses named pipes, which the sandbox blocks: build and restore run with
  `-m:1`.
- Without network access the NuGet audit turns into an error: restore runs with the audit
  disabled or from the local cache.
- Packages missing from the cache are restored by the user; the agent builds without restore.
- Running the application and its containers is the user's job; the agent builds and tests.

## Frontend

> **Deviation from the harness base.** The base's reference frontend is a Vite SPA in a pnpm `apps/`
> monorepo. This project uses Next.js with the App Router, TypeScript, Tailwind CSS, shadcn/ui and
> Feature-Sliced Design instead. The owner decided it; the reasoning is in
> `docs/adr/0004-nextjs-app-router-and-fsd-frontend.md`. The rules below are the base's rules
> re-drafted for that stack — the stack-agnostic ones are unchanged, and what the base never
> decided is marked as an open question rather than guessed.

### Stack and structure

**Stack**

| Layer | Technologies |
|---|---|
| Build | Next.js (App Router), React, TypeScript |
| UI | Tailwind CSS, shadcn/ui components |
| Data | Open question — see "Data and state" |
| Forms | react-hook-form + zod |
| API | Backend OpenAPI schema → a generated client |
| Tests | Vitest, React Testing Library, MSW, Playwright |
| Quality | ESLint (flat config), typescript-eslint, Prettier, git hooks |

Exact versions are pinned in the monorepo root `package.json`; a version bump is a separate
change with a reason.

**Monorepo layout (pnpm)**

```
frontend/
├── apps/web/                 # the Next.js app
├── apps/<second-app>/        # a landing page or a second client — its own app
├── packages/ui/              # design system: components, tokens, styles
├── packages/api-client/      # generated client
├── packages/shared/          # types, utilities, constants
├── packages/eslint-config/   # shared linter configs
├── packages/tsconfig/        # base tsconfigs
├── pnpm-workspace.yaml
└── package.json              # root scripts and versions
```

**Feature-Sliced Design: layers**

| Layer | Holds |
|---|---|
| `app/` | Next.js App Router routes and global providers. Minimal logic |
| `pages/` | One screen composed from widgets, features and entities |
| `widgets/` | Independent UI blocks that carry their own logic |
| `features/` | One folder per business capability |
| `entities/` | Domain entities, reusable only |
| `shared/` | UI kit, utilities, config, test utilities; knows nothing about features |

A layer may import only from the layers below it:

```
app → pages → widgets → features → entities → shared
```

- **A slice lives inside a layer, and slices inside one layer do not import each other.**
  Composition happens in the layer above. A linter rule enforces it, not discipline.
- **The App Router also calls its route directory `app/`.** How the FSD `app` layer and the
  `app/` route directory are kept apart is open; it changes every import path, see
  `docs/adr/0004-nextjs-app-router-and-fsd-frontend.md`.
- **Barrel files** (re-exporting everything) are out: they break tree-shaking. The exception is
  `features/<name>/index.ts` as the feature's public API, with explicit exports.
- Generated files (the API client) are never edited by hand and never imported into the UI
  directly.

### Data and state

> **Open — carried over from the base, not re-derived.** The base's data-and-state guidance was
> written for a Vite SPA where every screen runs in the browser. Under the App Router the base's
> "a query library only" rule and its query-key, `queryFn`-location and mutation-invalidation
> rules do not settle the server-side / query-library / mixed choice, and the design system's home
> (whether shadcn/ui components live in a design-system package or in the FSD layers) is open
> too. Both are open questions, not decisions: see
> `docs/adr/0004-nextjs-app-router-and-fsd-frontend.md`.

**Client state**

- **Client state is for client concerns only.** Server data is not duplicated in a client store.
- A local store holds UI state and drafts that have not been applied yet.
- **State that must be shareable through a link lives in the URL** (query, filters, sorting,
  page): a link to a result survives a reload and forwarding.
- Derived values are computed, not stored as a second field.

**Forms**

- Only the form-plus-schema pair (`react-hook-form` + `zod` through a resolver); a separate
  `useState` per field is out.
- **The schema is the single source of the form's type** (inferred from the schema), not a
  hand-written interface next to it.
- Error messages come from the schema and are not duplicated in the markup.
- A validation error is tied to its field: invalid and described-by attributes are set on the
  input element.

**API client**

- The client is **generated from the backend OpenAPI schema**; generated files in the repository
  are never edited by hand.
- When the backend schema changes, the client is regenerated in the same change; a divergence is
  caught by CI.
- The UI works only through the feature's public API: calling the client directly from a component
  is out.

### UI and accessibility

**Mandatory screen states**

Every screen with asynchronous data implements **four** states, even when "empty never happens":

| State | Requirement |
|---|---|
| Loading | A skeleton or spinner in place of the content, with no layout shift; a refetch does not reset the screen |
| Empty | A "nothing found" text plus what to change in the query; never a blank white screen |
| Error | A clear message and a retry action; technical detail goes to the log, not to the user |
| Success | The data; a partial result is handled separately from an error |

Cross-cutting states: "no access" (where authorisation exists) and "partial load" — show what
arrived, marked as partial.

**Accessibility (WCAG 2.2 AA — mandatory)**

- **Semantics:** `<button>` for actions, `<a>` for navigation, `<label>` for inputs.
- **Keyboard:** everything interactive is reachable by Tab, focus is visible, and the focus order
  matches the visual one.
- **Forms:** an error is tied to its field and announced; required fields and formats are
  declared.
- **Asynchronous results:** the changing container is marked as a polite live region so the result
  is announced.
- **Images:** meaningful ones carry a text alternative, decorative ones carry an empty one.
  Icon-only buttons carry a screen-reader label.
- **Interactive targets** are at least 24×24 CSS px.
- **Contrast** of text and controls meets AA; it is checked against a checklist, not by eye.

**Styling**

- Utilities and theme tokens only. A per-component CSS file, inline styles and direct DOM
  manipulation are out.
- Colours, spacing and radii come from tokens: hard-coded values and `!important` are out.
- Dark theme and responsiveness are part of the design system, not a separate "later" task.

**Where the recipes are**

This section states what the project requires. The how of it is vendored, not written here:
`better-accessibility` (semantics, focus, forms, hit areas), `better-layout` (grouping, spacing,
responsive structure), `better-typography`, `better-colors` (measuring a rendered pair),
`better-ui` (surfaces, icons, motion), `better-writing` (interface copy). `better-interface` runs
the cross-discipline review and owns the severity ladder. The accessibility bar (WCAG 2.2 AA, the
four mandatory screen states) stays here: it is a project requirement, and those skills supply
the recipes that meet it.

### Quality

**Lint and types**

- ESLint (flat config) and `tsc --noEmit` pass with **no errors and no warnings**.
- Forbidden without a justifying comment: `any`, type suppression (`@ts-expect-error`,
  `@ts-ignore`), double casting through `unknown`, default exports (except where a tool requires
  them), barrel files.
- **A linter or type rule is not switched off instead of fixing the code.** Relaxing a rule is the
  user's decision, with a reason and a registry entry in the project.
- A git hook runs lint and types on the changed files: a defect does not reach review.

**Tests**

| Level | What it checks |
|---|---|
| Unit | Utilities, reducers, pure functions, validation schemas |
| Component | Component behaviour through accessible roles and text (React Testing Library) |
| Integration | A screen with a substituted network (MSW): loading, empty, error, success |
| End-to-end | A critical user path in a browser (Playwright) |

- Tests check **behaviour, not implementation**: markup snapshots and reaching into internal
  state are out.
- The network is substituted at the request level, not by mocking modules.
- New functionality arrives with a test at the matching level.

**Definition of done**

1. `typecheck` and `lint` pass with no errors and no warnings.
2. Tests are green; new functionality is covered at the matching level.
3. The build passes and the bundle stays inside the project's budget.
4. The screen implements every mandatory state and passes the accessibility checklist.
5. State that must be shareable through a link is reproducible from the URL.
6. No forbidden techniques; new dependencies are either absent or agreed.
7. Documentation and derived artefacts (the generated client, the stack schema) are updated when
   versions, structure or a contract changed.

**Red lines**

- Switching off a linter or type rule instead of fixing the code.
- Committing a generated client that diverges from the backend OpenAPI schema.
- Duplicating server data into a client store or local state "for convenience".
- Working around types and "temporarily" breaking the FSD boundary between layers or between
  slices.
- Editing generated code by hand.
- Implementing a screen without handling the empty result and the error.
- Adding a dependency without justification and a check of its licence and freshness.

## Project facts

**This section is the project's own memory. A re-run of the harness base's deployment preserves
it:** the skills are replaced wholesale and the rules above are re-drafted from the base, but the
facts below survive.

| Fact | Value |
|---|---|
| Sections adopted | `shared`, `backend`, `frontend` |
| Skills installed | 46, flat in `.dsh/skills`. The four `sqlserver-*` skills were removed on the owner's decision: the database is PostgreSQL, not SQL Server |
| The site | The personal site of Илья Каленцов — a .NET developer of more than eight years, with Angular on the frontend. This site is his first React project, built to learn the stack |
| First screen | One page: an "about me" block, then a list of the owner's projects |
| Content | Lives in PostgreSQL and is edited through the administration panel; the API is what serves it to the site |
| A project entry | Title, short description, technology stack, a link to the repository or a demo, and the year |
| Copy | The site's text is written, not transcribed. The owner's own words are the source; what appears on the site is a proper text |
| Visual design | Not started. The owner brings references later; until then the page has to read acceptably as plain text |
| Languages | Russian first, English behind a switch. Public routes are `/[lang]/...`; each text is stored per locale in translation tables ([ADR-0005](docs/adr/0005-bilingual-content-storage.md)) |
| Backend stack | .NET 10 (`net10.0`); ASP.NET Core Web API with controllers; EF Core + Npgsql (PostgreSQL); JWT bearer + BCrypt; OpenAPI + Scalar |
| Frontend stack | Next.js with the App Router, TypeScript, Tailwind CSS, shadcn/ui, Feature-Sliced Design ([ADR-0004](docs/adr/0004-nextjs-app-router-and-fsd-frontend.md)) |
| Physical layout | `backend/src/MySite.{Domain,Application,Contracts,Infrastructure.Postgres,Web}`, `backend/tests/MySite.{UnitTests,IntegrationTests,ArchitectureTests}`, solution `backend/MySite.slnx`; `frontend/` is reserved and empty |
| Package set and versions | stated once, in `backend/Directory.Packages.props`; no `.csproj` carries a `Version` |
| Database | PostgreSQL in Docker via `backend/docker-compose.yml`, published on `localhost:5433`; credentials in the untracked `backend/.env`; the schema is one generated migration |
| Commands | `WORKFLOW.md` |
| Architecture | `ARCHITECTURE.md` |
| Vocabulary | `CONTEXT.md` |
| Decisions | `docs/adr/` |
| Research | `docs/research/` |
| First phase | Public site: every page open, no sign-in. Authentication is deferred to the administration panel ([ADR-0003](docs/adr/0003-public-site-first-phase.md)) |
| Planned | The administration panel and the visual design; both after the public page |
| Environment | .NET SDK 10.0.401; Docker 29.7.2 with Compose v5.3.1 |

### Open questions

Product and process decisions, not architecture findings — the live findings about the system
itself are in `ARCHITECTURE.md` (§ Known problems).

**Blocking the first page**

- Nothing is seeded: the endpoints exist, but the page has nothing to read until the profile text
  and the projects are in the database.
- The frontend project does not exist: `frontend/` holds a README and nothing else.

**Frontend, carried over from ADR-0004**

- Data fetching under the App Router: server components, a query library, or a mix.
- Where shadcn/ui components live: a design-system package or inside the FSD layers.
- The FSD `app` layer and the App Router's `app/` directory both claim the name; how they are kept
  apart changes every import path.
- The test runner is still listed as Vitest, which is unconfirmed for Next.js.

**Later**

- The administration panel: its screens, its content model, and what it may change.
- The visual design, which waits on the owner's references.
- Whether the existing users and JWT code becomes the panel's foundation or is replaced.
- `Properties/launchSettings.json` still carries a JWT secret for the dormant account code. The
  PostgreSQL credentials are out of the tracked files: they live in the ignored `backend/.env` and
  `appsettings.Development.json`.

## Build templates

`Directory.Build.props`, `Directory.Packages.props`, `.globalconfig`, `global.json` and
`.gitignore` in `backend/` came from the harness base and have been reconciled with this project:
the target framework, the package set and every version are this project's, and the build is
clean under `TreatWarningsAsErrors` with all four analysers.

`frontend/templates/` is empty in the base by design and holds only a README explaining what
belongs there. The frontend configuration for this project is not written yet.
