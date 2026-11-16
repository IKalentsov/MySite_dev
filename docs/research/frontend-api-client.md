# The generated API client: the schema, the generator, and the CI checks

The question: the project requires the frontend to consume the backend through a client generated
from the backend's OpenAPI document, never edit the generated files by hand, regenerate in the same
change that moves the schema, and let CI catch a divergence. Which generator, at which exact
version, fits TypeScript 6.0.3 / Next.js 16.4 / React 19.3 in a pnpm workspace, with no query
library (ticket 18), no browser-only API in the server path, deterministic output and a package
that exposes exactly one entry point? Where does the schema come from, and what exactly does CI
check?

Method: the schema was captured from the local API that was actually running, and the winning pair
was installed and executed in a throwaway directory outside the repository against that captured
document. Every version comes from the npm registry (or NuGet for the .NET side), and every claim
about a tool comes from that tool's own documentation, repository or published package metadata.
No version and no compatibility statement is taken from a blog post or from memory. The tooling
alternatives that were not executed are marked as documentation-only.

## What the primary sources actually say

### 1. The document the backend actually serves

The local API (`dotnet run --project src/MySite.Web`, listening on `http://localhost:5054`) was
already running when this research started. The document was captured with:

```powershell
Invoke-WebRequest http://localhost:5054/openapi/v1.json -OutFile "$env:TEMP\mysite-openapi.json"
```

Measured facts about the captured file:

| Fact | Value |
|---|---|
| `openapi` | **3.1.1** |
| `info.title` / `info.version` | `MySite.Web \| v1` / `1.0.0` |
| Paths | **3**: `/api/v1/home`, `/api/v1/profile`, `/api/v1/projects` (all `GET`) |
| Schemas | **5**: `HomeResponse`, `OwnerProfileResponse`, `PaginatedProjectsResponse`, `ProblemDetails`, `ProjectResponse` |
| `servers` | one entry, `http://localhost:5054/` |
| Size | **11 334 bytes** |
| SHA-256 | `8b5575e318365656890bf67b82bef13c7cfa5f2c5fee9ec6cff3ccb8741e6ac8` |

The shape matters for the generator:

- `locale` is a query parameter on all three paths; `cursor` and `pageSize` are on `/home` and
  `/projects`.
- `pageSize` and `year` are emitted as `"type": ["integer", "string"]`; `nextCursor` as
  `"type": ["null", "string"]`; `ProblemDetails` fields are nullable. These are OpenAPI 3.1 type
  arrays, not 3.0 `nullable: true`.
- `HomeResponse.profile` is `oneOf: [{type: "null"}, {$ref: OwnerProfileResponse}]`.
- Every response advertises three content types: `text/plain`, `application/json`, `text/json`.
- The document has no `components.responses`, `parameters`, `requestBodies` or `headers`; only
  `schemas`.

The API returned the expected payloads for the documented endpoints: `/api/v1/home?locale=ru` and
`/api/v1/home?locale=en&pageSize=1` both returned `200` with a `profile` and a `projects` object;
`/api/v1/profile?locale=ru` returned `200`; `/api/v1/projects?locale=ru&pageSize=1` returned `200`.
The `400` path for an unsupported locale is documented by the controller annotations and was
already exercised by ticket 03.

The document is served only because `Program.cs` calls `app.MapOpenApi()` in the Development
environment; `AddOpenApi()` itself is unconditional. Microsoft's documentation for the built-in
OpenAPI support states that .NET 10 generates **OpenAPI 3.1** by default and that OpenAPI 3.2 becomes
the default in .NET 11 ([Generate OpenAPI documents](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0)).
That matches the captured `3.1.1`.

A second instance was started on port 5055 with `ASPNETCORE_ENVIRONMENT=Development`, a different
URL and an **unreachable** database (`ConnectionStrings__DefaultConnection` pointed at
`127.0.0.1:5999`). It served `/openapi/v1.json` with `200` and the same 11 334 bytes; the only
difference between the two captures was `servers.url` (`http://127.0.0.1:5055/` instead of
`http://localhost:5054/`). That is useful: the document can be produced without a live database as
long as the connection string is non-empty, and the `servers` entry is derived from the request
host, not from configuration.

### 2. The candidates, and the exact registry versions

The npm registry's `dist-tags.latest` is the version a package manager would install. The publish
time is the registry's own `time` value for that version.

| Package | Latest | License | Node | Peer | Published |
|---|---|---|---|---|---|
| `openapi-typescript` | **7.13.0** | MIT | not declared | `typescript: ^5.x` | 2026-02-11 |
| `openapi-fetch` | **0.17.0** | MIT | not declared | none | 2026-02-11 |
| `openapi-typescript-helpers` | **0.1.0** | MIT | not declared | none | — (dependency of `openapi-fetch`) |
| `orval` | **8.48.0** | MIT | `>=22.18.0` | `prettier >=3.0.0` (optional) | 2026-11-14 |
| `kubb` | **5.20.2** | MIT | `>=22.22` | none at the root package | 2026-11-15 |
| `nswag` | **14.8.0** | MIT | no `node` engine; requires .NET | none | 2026-11-11 |
| `@hey-api/openapi-ts` | **0.99.0** | MIT | `>=22.18.0` | `typescript >=5.5.3 \|\| >=6.0.0 \|\| 6.0.1-rc` | 2026-06-22 |

Sources: `https://registry.npmjs.org/<package>/latest` for each, plus the package's `time` map for
the publish date. `openapi-typescript-helpers` is not a generator; it is the type-level helpers
package that `openapi-fetch` declares as its only dependency.

### 3. `openapi-typescript` + `openapi-fetch`: what the sources say

The project's own README is explicit about the two things this ticket needs most:

> "openapi-typescript turns OpenAPI 3.0 & 3.1 schemas into TypeScript quickly using Node.js. No
> Java/node-gyp/running OpenAPI servers necessary."

> "✅ Supports OpenAPI 3.0 and 3.1 (including advanced features like discriminators)"

> "✅ Generate **runtime-free types** that outperform old school codegen"

> "The code is MIT-licensed and free for use."

([packages/openapi-typescript/README.md](https://github.com/openapi-ts/openapi-typescript/blob/main/packages/openapi-typescript/README.md),
[registry.npmjs.org/openapi-typescript/latest](https://registry.npmjs.org/openapi-typescript/latest))

The client side is deliberately small and is a thin wrapper over the platform `fetch`:

> "openapi-fetch is a type-safe fetch client that pulls in your OpenAPI schema. Weighs **6 kb** and
> has virtually zero runtime. Works with React, Vue, Svelte, or vanilla JS."

> "`GET()`, `PUT()`, `POST()`, etc. are thin wrappers around the native fetch API (which you can swap
> for any call)."

> "Eliminates `any` types that hide bugs"

([packages/openapi-fetch/README.md](https://github.com/openapi-ts/openapi-typescript/blob/main/packages/openapi-fetch/README.md),
[docs/openapi-fetch/index.md](https://github.com/openapi-ts/openapi-typescript/blob/main/docs/openapi-fetch/index.md))

The API reference confirms the properties that matter for a Server Component: `fetch` defaults to
`globalThis.fetch`, and any valid fetch option is accepted, including `cache` and `signal`:

| Name | Type | Description |
| :--- | :--- | :--- |
| `baseUrl` | `string` | Prefix all fetch URLs with this option |
| `fetch` | `fetch` | Fetch instance used for requests (default: `globalThis.fetch`) |
| (Fetch options) | | Any valid fetch option (`headers`, `mode`, `cache`, `signal`, …) |

([docs/openapi-fetch/api.md](https://github.com/openapi-ts/openapi-typescript/blob/main/docs/openapi-fetch/api.md))

The support table adds the runtime floor:

> "**Node** — >= 18.0.0"

> "**TypeScript** — >= 4.7 (>= 5.0 recommended)"

### 4. What was actually measured

The pair was installed in `%TEMP%\api-client-eval` with exact versions (`openapi-typescript@7.13.0`,
`openapi-fetch@0.17.0`, `typescript@6.0.3`) and run against the captured document. The temporary
directory is outside the repository and nothing from it was committed.

Generation:

```text
✨ openapi-typescript 7.13.0
🚀 openapi.json → src/generated/openapi.d.ts [32.4ms]
```

The generated file is 9 370 bytes / 241 lines. It contains no `any`; the only `unknown` is the
standard `headers: { [name: string]: unknown }` index signature. The 3.1 constructs are rendered
faithfully:

- `pageSize?: number | string` and `year: number | string` (the schema's `["integer", "string"]`);
- `nextCursor: null | string`;
- `profile: null | components["schemas"]["OwnerProfileResponse"]`;
- `ProblemDetails` fields as `null | string` / `null | number | string`.

Two consecutive generations of the same document produced the same SHA-256
(`BD1667FFA53E5E0245C887B62399A5B98136D58889C2A83EBA52617078229047`), so the output is
byte-deterministic for a fixed schema and version.

Type checking under the project's TypeScript version:

```text
pnpm exec tsc --noEmit
EXIT=0
```

The checked file used `createClient<paths>`, `client.GET("/api/v1/home", { params: { query: {
locale, pageSize: 20 } }, cache: "no-store" })`, and narrowed `data` / `error` before use. No
suppression and no cast was needed.

A Node run against the still-running API proved the server path needs no browser API:

```json
{
  "profileLocale": "ru",
  "headline": "Заглушка: .NET-разработчик, восемь лет в промышленной разработке",
  "projectCount": 3,
  "hasMore": false,
  "nextCursor": null
}
```

`EXIT=0`; `fetch`, `Request`, `Response` and `Headers` came from Node 24 itself, not from a DOM
shim. `cache: "no-store"` was accepted as a fetch option.

One measured caveat: `openapi-typescript@7.13.0` declares `typescript: ^5.x`, and the project uses
6.0.3. `pnpm peers check` reports:

```text
✕ unmet peer typescript
  Installed: 6.0.3
  Wanted:
    ^5.x:
      openapi-typescript@7.13.0
```

The generated types still compile under 6.0.3 (above), but upstream has not widened the range. The
open issue is [Add support for TypeScript 6 (#2723)](https://github.com/openapi-ts/openapi-typescript/issues/2723),
opened 2026-03-27 and still open on 2026-11-16, with open pull requests #2774, #2818 and #2872. The
issue's own text is narrow:

> "please allow the new TypeScript 6 as a peer dependency"

So this is a declared-compatibility gap, not a demonstrated incompatibility. The repository is still
maintained (last commit 2026-09-25), but the last release is 2026-02-11.

The published `openapi-fetch` declaration file does contain `any` in a few internal helper
signatures (`QuerySerializer<T> = T extends { parameters: any }`, `BodySerializer<T> = … => any`).
Those are library internals under `skipLibCheck`; they do not appear in the generated types and do
not leak into the call sites, which are exact. The generated output itself has no `any`.

### 5. The alternatives, as documented

These were **not executed**. The claims below are the projects' own documentation or published
source; the "not confirmed" section records what that means.

**orval 8.48.0.** MIT, Node `>=22.18.0`, published 2026-11-14. It explicitly handles OpenAPI 3.1
JSON Schema keywords:

> "Orval automatically resolves JSON Schema 2020-12 `$dynamicRef` / `$dynamicAnchor` keywords in
> OpenAPI 3.1 specs. No configuration is needed."

Its default `client` is `'axios-functions'`; `client: 'fetch'` selects the native-fetch output. The
fetch guide claims the strongest runtime properties of the field:

> "Generated code has no runtime dependencies of its own, and every operation is a standalone
> function, so bundlers can tree-shake unused operations and split the rest by route."

> "**No runtime dependencies.** Everything, including the shared runtime, is generated into your
> project."

> "**Tree-shaking.** Each operation is a separate export…"

It also ships a CI-oriented command:

> "`--outdated` … Compare what Orval would generate with the files already on disk… exits with code
> 1 when any is found, so CI can catch a specification or configuration change that was not followed
> by a regeneration"

([guides/fetch.mdx](https://github.com/orval-labs/orval/blob/master/docs/content/docs/guides/fetch.mdx),
[reference/configuration/output.mdx](https://github.com/orval-labs/orval/blob/master/docs/content/docs/reference/configuration/output.mdx),
[reference/cli.mdx](https://github.com/orval-labs/orval/blob/master/docs/content/docs/reference/cli.mdx))

Orval generates types **and** one function per operation. React Query hooks are opt-in
(`client: 'react-query'`), not the default; the query options can also be suppressed individually.
That means it does not force a query library, but it does mean adopting a full SDK generator and
turning its defaults off rather than adopting a types-only generator.

**kubb 5.20.2.** MIT, Node `>=22.22`, published 2026-11-15. It reads OpenAPI 2.0–3.2:

> "The default OpenAPI adapter reads OpenAPI 2.0, 3.0, 3.1, and 3.2."

It is plugin-based: `pluginTs()` generates types, `pluginFetch()` generates a client, and
`pluginReactQuery()` is a separate opt-in plugin whose docs say "The plugin ships no HTTP client"
and "`hooks: true` adds `useQuery` and `useMutation` hooks". The fetch plugin's runtime is copied
into the output:

> "The runtime is copied into `.kubb/`, so the generated code needs only the global `fetch`."

> "No HTTP client package is needed. The generated code calls the global `fetch`."

([docs/5.x/introduction.md](https://github.com/kubb-labs/kubb/blob/main/docs/5.x/introduction.md),
[plugin-fetch/overview.md](https://github.com/kubb-labs/plugins/blob/main/packages/plugin-fetch/docs/overview.md),
[plugin-react-query/overview.md](https://github.com/kubb-labs/plugins/blob/main/packages/plugin-react-query/docs/overview.md))

Two risks are visible in the published artifacts and docs: the generated runtime imports with
explicit `.ts` specifiers (`import … from './core.ts'`), which needs a tsconfig decision in a
Next.js project, and no documentation states that the output is deterministic; `output.clean`
defaults to `false`, so a deleted schema can leave stale files behind. The plugin versions are also
split across majors (`@kubb/plugin-ts@5.5.4`, `@kubb/plugin-fetch@6.0.4`,
`@kubb/adapter-oas@5.20.2`).

**NSwag 14.8.0.** MIT, published 2026-11-11, but the npm package requires .NET installed:

> "**This NPM module requires Full .NET Framework 4.6.2+ or .NET 6.0+ to be installed on your
> system!**"

The serializer documents only 3.0:

> "The NSwag serializer supports Swagger 2.0 and OpenApi 3.0."

Its TypeScript resolver emits `any` in several branches (any-type, binary, dictionary default), the
default output is class-based in one file, and every generated file carries the toolchain version.
It fails the no-`any` and 3.1 criteria and is rejected.

([npm README](https://www.npmjs.com/package/nswag),
[OpenAPI-3-support](https://github.com/RicoSuter/NSwag/wiki/OpenAPI-3-support),
[TypeScriptTypeResolver.cs](https://github.com/RicoSuter/NSwag/blob/master/src/NJsonSchema.CodeGeneration.TypeScript/TypeScriptTypeResolver.cs))

**@hey-api/openapi-ts 0.99.0.** MIT, Node `>=22.18.0`, published 2026-06-22, and the only
candidate whose peer range explicitly includes TypeScript 6 (`>=5.5.3 || >=6.0.0 || 6.0.1-rc`).
It generates types and an SDK by default, with a fetch client; TanStack Query is an optional plugin.
A throwaway run produced 15 files / ~60 KB: `types.gen.ts`, `sdk.gen.ts`, `client.gen.ts` and a
vendored `client/` + `core/` runtime. That runtime contains explicit `any` (for example
`body?: any;`, `let emptyData: any;`, `let data: any;`), and the generated `client.gen.ts` hard-codes
the `servers` entry as the default `baseUrl`. It is a real option if TypeScript 6 support is the
deciding criterion, but it trades the types-only property for a generated runtime that does not
meet the project's no-`any` rule.

### 6. Where the schema lives: build time, not a running API

The primary source for the mechanism is Microsoft's documentation. It names "committed into source
control" as one of the scenarios for build-time generation:

> "In some scenarios, it's helpful to generate the OpenAPI document during the app's build step.
> These scenarios include generating OpenAPI documentation that is: Committed into source control.
> Used for spec-based integration testing. Served statically from the web server."

> "To add support for generating OpenAPI documents at build time, install the
> `Microsoft.Extensions.ApiDescription.Server` package."

> "Upon installation, this package: Automatically generates the Open API documents associated with
> the app during build. Populates the Open API documents in the app's output directory."

The output location and file name are MSBuild properties:

> "To modify the location of the emitted file, set the target path in the `OpenApiDocumentsDirectory`
> property."

> "To modify the name of the emitted file, set the `--file-name` argument in the
> `OpenApiGenerateDocumentsOptions` property."

The important operational detail is that this is not a static analysis pass:

> "Build-time OpenAPI document generation functions by launching the apps entrypoint with a mock
> server implementation. A mock server is required to produce accurate OpenAPI documents because all
> information in the OpenAPI document can't be statically analyzed. Because the apps entrypoint is
> invoked, any logic in the apps startup is invoked. This includes code that injects services into
> the DI container or reads from configuration."

([Generate OpenAPI documents — Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0))

That sentence has a direct consequence for this repository: `MySite.Infrastructure.Postgres` throws
`InvalidOperationException` when `ConnectionStrings:DefaultConnection` is empty, and the tracked
`appsettings.json` deliberately contains an empty placeholder. Build-time generation therefore needs
a non-empty connection string in the build environment, even though no database is contacted. The
same is true of the runtime capture: the second instance on 5055 started only because the connection
string was non-empty.

The package's own MSBuild props and targets (version 10.0.12, read from the `.nupkg`) confirm the
switch that keeps a normal `dotnet build` from running the app on every build:

> `OpenApiGenerateDocumentsOnBuild` — "If 'true' (the default if `$(OpenApiGenerateDocuments)` is
> 'true'), will generate OpenAPI documents after every build. Set to 'false' when targets are
> invoked from the command line or tied to another target."

and the target:

> `<Target Name="_GenerateOpenApiDocuments" BeforeTargets="Build"
> Condition=" '$(OpenApiGenerateDocumentsOnBuild)' == 'true' … " DependsOnTargets="GenerateOpenApiDocuments" />`

The plan for this repository is therefore: install the package at **10.0.12** (matching the
existing `Microsoft.AspNetCore.OpenApi` 10.0.12 line), set
`OpenApiGenerateDocumentsOnBuild=false` in the project, and invoke
`-p:OpenApiGenerateDocumentsOnBuild=true` only in the schema step with a dummy non-empty
connection string. The output is `backend/docs/openapi.json`, committed.

Why not generate the schema from a running API during every frontend build: that makes the frontend
build non-hermetic, dependent on a live backend and a database, and impossible to run offline. The
committed-document approach keeps the build reproducible and makes the schema reviewable in the
pull request. The runtime capture is still the way this ticket verified the contract, and it stays
the documented fallback if build-time generation cannot be made to work in this app.

### 7. The two divergences CI has to catch

There are two different failures, and they need two different checks.

**Client vs schema (the generated types are stale).** The frontend job regenerates from the
committed `backend/docs/openapi.json` and fails if the tracked output changes:

```text
pnpm install --frozen-lockfile
pnpm --filter @mysite/api-client generate
git diff --exit-code -- packages/api-client
```

This catches "the schema moved, the types were not regenerated". It does not catch "the backend
moved, the committed schema was not regenerated".

**Schema vs backend (the committed schema is stale).** The backend job builds the schema and fails
if it differs from the tracked file:

```text
dotnet build MySite.slnx -m:1 -p:OpenApiGenerateDocumentsOnBuild=true
git diff --exit-code -- docs/openapi.json
```

with `ConnectionStrings__DefaultConnection` set to a non-empty dummy value for the build step. This
catches "the backend contract moved, the schema was not regenerated", with no database and no API
process. The cost is the build that already runs in `backend.yml` plus the document-generation step;
the measured runtime path was a few seconds of startup. The build-time path does not start Kestrel,
but its exact cost was not measured here.

The two `git diff --exit-code` steps above are deliberately strict: a stale generated file must
fail the build, not be silently rewritten in the CI workspace.

### 8. The package shape

The package is a pnpm workspace package whose only public entry point is `"."`:

```text
frontend/packages/api-client/
├── package.json                 # exports: { ".": "./src/index.ts" } only
├── tsconfig.json
└── src/
    ├── index.ts                 # the public API: client factory, endpoint functions, ApiError, types
    ├── client.ts                # createClient<paths>, baseUrl from process.env.API_BASE_URL
    ├── api-error.ts             # ApiError and the mapping from ProblemDetails
    └── generated/
        └── openapi.d.ts         # openapi-typescript output; never edited by hand
```

The schema lives at `backend/docs/openapi.json` and is committed. The package's `generate` script
reads it with the relative path `../../../backend/docs/openapi.json` and writes
`src/generated/openapi.d.ts`. `openapi-typescript` is a dev dependency, `openapi-fetch` is a runtime
dependency, and both are referenced as `catalog:` from `pnpm-workspace.yaml` with exact versions.

**Deep imports and the package boundary.** Deep imports are blocked by `exports` (only `"."`) and by
`no-restricted-imports` for `@mysite/api-client/*`. The root import is a different check, and the
current boundary policy is looser than the project rule:
`packages/eslint-config/src/index.mjs` allows `api-client` from `route`, `pages`, `widgets`,
`features`, `entities` and `shared`. Ticket 15 must remove `api-client` from every allowed importer
except `pages`, so that a component importing the package is a violation and `_pages/<slice>/api`
stays the intended caller. The existing `import/no-restricted-paths` zone (`target: ./packages`,
`from: ./apps`) only forbids packages importing apps, not the reverse, so it does not cover this.

**Generated files and the quality gates.** The header of the generated file is the marker:

```text
/**
 * This file was auto-generated by openapi-typescript.
 * Do not make direct changes to the file.
 */
```

The policy:

- `tsc --noEmit` **includes** `src/generated/openapi.d.ts`; the types must compile. This is the whole
  point of generating them.
- ESLint **ignores** `src/generated/**` through the root flat config's `globalIgnores`. The file is
  not authored here, cannot be fixed here, and is already protected by the compiler and by the
  regeneration diff; linting it would only turn a generator upgrade into an unfixable build break.
  This is a scope decision, not a disabled rule on authored code. It needs a sentence in
  `.dsh/AGENTS.md` under Quality → Lint and types (planner's edit).
- The CI diff is the guarantee that the generated file matches the committed schema.
- The generated output has no `any` today; if a future generator version emits one, it appears in the
  review diff. The project's no-`any` rule applies to authored code, and the generated output is
  checked by `tsc` and by the diff, not by a hand edit.

The public API is typed from the generated `paths`; it does not export the generated module itself.
`src/index.ts` uses explicit named exports (no `export *`), and the package has no default export.
The client is created per call with `process.env.API_BASE_URL` (not `import.meta.env`, which is
Vite-only) and passes `cache: "no-store"` through to `fetch`; it does not duplicate server data in a
client store and does not introduce a query library.

## My reasoning, and what I could not confirm

### The decision

**`openapi-typescript@7.13.0` + `openapi-fetch@0.17.0`.** The schema is generated at backend build
time by `Microsoft.Extensions.ApiDescription.Server@10.0.12` into the committed
`backend/docs/openapi.json`; the generated TypeScript lives in
`frontend/packages/api-client/src/generated/openapi.d.ts` and is committed too.

What the pair gives over the alternatives:

- **Types only.** The generated file is a declaration file; there is no generated runtime to ship or
  tree-shake. The only runtime is the 6 KB `openapi-fetch` client, which the docs describe as a thin
  wrapper over native `fetch`.
- **No query library.** There is no hook generator and no TanStack Query dependency, which is
  exactly ticket 18's boundary.
- **Exact typing from the document.** Paths, query parameters, response contents and error shapes
  are inferred; the generated file contains no `any`, and the call site needs no cast.
- **Deterministic output.** Two runs over the same document produced the same bytes.
- **A small, reviewable footprint.** One 9 KB declaration file for three endpoints, not 15 generated
  files with a vendored runtime.
- **Server-side use.** The client accepts any fetch option, defaults to `globalThis.fetch`, and was
  run in Node against the live API with no DOM.

What it does not give:

- **No generated SDK functions.** The caller writes `client.GET("/api/v1/home", …)`; the path is
  still a literal string, and the compiler checks it against the schema. That is acceptable for
  three endpoints and matches the thin `getHomeContent` boundary.
- **No runtime validation.** The types are compile-time only; a backend that violates its own schema
  is not detected. That is not in this project's requirements.
- **No built-in "is the committed output current" command.** Orval has `--outdated`; here the check
  is `git diff --exit-code` after regeneration. That is one CI step and is stronger than a tool
  command because it also covers the committed schema.
- **No official TypeScript 6 support yet.** The peer range is `^5.x`; the generated output compiles
  under 6.0.3, but upstream has not declared it. The mitigation is an explicit
  `peerDependencyRules.allowedVersions` entry for `openapi-typescript>typescript: 6` in
  `pnpm-workspace.yaml`, with a comment, exactly as ticket 14 allowed
  `eslint-plugin-import>eslint: 10` for the same kind of stale peer range. The revisit trigger is a
  release that widens the peer range to `^6`, or the TypeScript 7 migration, whichever comes first.
  If the owner rejects the override, **orval** is the documented fallback: it is fresh, supports
  OpenAPI 3.1, has a native-fetch client with no runtime dependencies, and has `--outdated`; the
  cost is a full SDK generator to configure and a larger generated surface.

### Why not the other candidates

- **orval** is the strongest alternative and the fallback. It is actively released (2026-11-14),
  explicitly handles 3.1, and its fetch client is runtime-free. It is not the first choice because
  it generates an SDK (one function per operation) and defaults to axios, so adopting it means
  switching defaults off and maintaining more generated code for three endpoints. If the types-only
  property stops mattering, orval is the natural next pick.
- **kubb** is fresh and explicitly supports 3.1/3.2, but it brings a bundled runtime with `.ts`
  imports, no documented determinism guarantee and `clean` off by default. That is more moving parts
  than this project needs.
- **NSwag** fails the criteria outright: only 3.0 is documented, `any` is emitted, output is
  class-based, and the npm package needs .NET installed.
- **@hey-api/openapi-ts** is the only candidate with explicit TypeScript 6 support, but its generated
  output vendors a runtime with `any` and is several times larger. The TypeScript 6 problem is
  smaller than the no-`any` and types-only problems it would introduce.

### A project inconsistency this ticket found

Ticket 03 created `GET /api/v1/home` as one composite response to avoid a second round-trip at SSR.
Ticket 18's answer still describes `getHomeContent` as two requests through
`Promise.allSettled` (`getProfile` + `getProjects`) and a partial state for "one of the two failed".
Both cannot be the page's data path. The generated client exposes all three endpoints, so this
ticket does not have to pick one, but ticket 15 does. The recommendation is to use the composite
`/home` once: it is the contract ticket 03 chose, it matches the "one source of data" rationale, and
`profile: null` + `projects` already expresses the partial/empty content. If the owner wants
request-level partial failure instead, 15 uses the two granular endpoints and `/home` stays unused by
the page; that would be a change to ticket 18's implementation note, not to its boundary decision.
Either way, the synchronous view keeps its states, and `error.tsx` handles a total failure.

### Not confirmed from a primary source

- **Build-time generation was not executed in this repository.** The package's props/targets
  (10.0.12) and Microsoft's documentation were read; the exact `dotnet-getdocument` behaviour with
  this app's `Program.cs` (seed command, HTTPS redirection, health checks, the empty
  connection-string guard) has not been observed. Ticket 15 must verify it and stop on the exact
  error if it fails.
- **The build-time document may differ from the runtime document** in `servers` (the runtime entry is
  derived from the request host) and possibly in descriptions. `openapi-typescript` ignores
  `servers`, and the client's base URL comes from the environment, so the generated types are
  unaffected; the CI comparison must compare build-time output with the committed build-time file,
  not with the runtime capture.
- **orval, kubb and NSwag were not executed.** Their sections are documentation/source reading. The
  exact rendered TypeScript for this document's `["integer", "string"]` and `oneOf`-with-null was not
  observed for any of them.
- **The "6 KB" size of `openapi-fetch` is the project's own published claim**, not a bundle
  measurement made here. What was measured is the installed package's source size and the Node
  execution; the claim is attributed to the README.
- **TypeScript 6 support is not official.** The generated output compiled in this one evaluation;
  that is evidence, not a guarantee for every future schema construct. The peer override is the
  explicit acknowledgement of the gap.
- **The `openapi-typescript` release cadence is nine months.** The repository has commits through
  2026-09-25, so it is not abandoned, but the last published version is 2026-02-11. This is the main
  reason the revisit trigger is written down rather than left implicit.

## Sources

- [openapi-typescript README](https://github.com/openapi-ts/openapi-typescript/blob/main/packages/openapi-typescript/README.md)
- [openapi-fetch README](https://github.com/openapi-ts/openapi-typescript/blob/main/packages/openapi-fetch/README.md)
- [openapi-fetch documentation: index](https://github.com/openapi-ts/openapi-typescript/blob/main/docs/openapi-fetch/index.md)
- [openapi-fetch documentation: API](https://github.com/openapi-ts/openapi-typescript/blob/main/docs/openapi-fetch/api.md)
- [openapi-typescript issue #2723 — Add support for TypeScript 6](https://github.com/openapi-ts/openapi-typescript/issues/2723)
- [registry.npmjs.org/openapi-typescript/latest](https://registry.npmjs.org/openapi-typescript/latest)
- [registry.npmjs.org/openapi-fetch/latest](https://registry.npmjs.org/openapi-fetch/latest)
- [orval README](https://github.com/orval-labs/orval/blob/master/README.md)
- [orval fetch guide](https://github.com/orval-labs/orval/blob/master/docs/content/docs/guides/fetch.mdx)
- [orval output configuration](https://github.com/orval-labs/orval/blob/master/docs/content/docs/reference/configuration/output.mdx)
- [orval CLI](https://github.com/orval-labs/orval/blob/master/docs/content/docs/reference/cli.mdx)
- [kubb introduction](https://github.com/kubb-labs/kubb/blob/main/docs/5.x/introduction.md)
- [kubb plugin-fetch overview](https://github.com/kubb-labs/plugins/blob/main/packages/plugin-fetch/docs/overview.md)
- [kubb plugin-react-query overview](https://github.com/kubb-labs/plugins/blob/main/packages/plugin-react-query/docs/overview.md)
- [NSwag OpenAPI 3 support](https://github.com/RicoSuter/NSwag/wiki/OpenAPI-3-support)
- [NSwag TypeScript type resolver](https://github.com/RicoSuter/NSwag/blob/master/src/NJsonSchema.CodeGeneration.TypeScript/TypeScriptTypeResolver.cs)
- [npm: nswag](https://www.npmjs.com/package/nswag)
- [Microsoft Learn — Generate OpenAPI documents](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0)
- [NuGet — Microsoft.Extensions.ApiDescription.Server versions](https://api.nuget.org/v3-flatcontainer/microsoft.extensions.apidescription.server/index.json)
- Local evidence: the captured `%TEMP%\mysite-openapi.json` (11 334 bytes, SHA-256 above) and the
  throwaway `%TEMP%\api-client-eval` project. These are not primary sources for a tool's behaviour;
  they are the measurements this ticket made.
