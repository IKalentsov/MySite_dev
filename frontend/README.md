# frontend — the site's client side

The pnpm monorepo for the site: a Next.js App Router application (`apps/web`) and the workspace
packages it builds from. The stack is recorded in
[ADR-0004](../docs/adr/0004-nextjs-app-router-and-fsd-frontend.md); the layer rules are in
[`ARCHITECTURE.md`](../ARCHITECTURE.md#frontend), and the gates are in
[`WORKFLOW.md`](../WORKFLOW.md#frontend).

## Layout

```
frontend/
├── apps/web/                    the Next.js application
│   └── src/
│       ├── app/                 App Router: layout, providers, globals.css, [lang]
│       ├── _pages/              FSD pages (src/pages would be the Pages Router)
│       ├── widgets/ features/ entities/
│       └── shared/
├── packages/ui/                 design system: tokens, styles, cn, components
├── packages/api-client/         the generated client: typed functions over the backend OpenAPI schema
├── packages/shared/             skeleton for shared types and utilities
├── packages/eslint-config/      shared flat config, FSD boundaries
├── packages/tsconfig/           base, Next.js and React library tsconfigs
├── pnpm-workspace.yaml          workspace globs and the version catalog
├── package.json                 scripts and the Node/pnpm pins
├── eslint.config.mjs            root flat config
├── lefthook.yml                 the pre-commit hook definition
└── .githooks/pre-commit         the hook Git runs; it passes LEFTHOOK_CONFIG to lefthook
```

## Commands

Run from this directory:

```powershell
pnpm install --frozen-lockfile
pnpm lint
pnpm typecheck
pnpm build
```

`pnpm --filter @mysite/api-client generate` rebuilds the client types from the committed
`backend/docs/openapi.json`; the schema itself is produced by the backend's build. Both generated
files are committed and never edited by hand — [`WORKFLOW.md`](../WORKFLOW.md#generated-api-client)
has the two commands and the CI checks.

`pnpm format` and `pnpm format:check` run Prettier. The three gates plus their CI are described in
[`WORKFLOW.md`](../WORKFLOW.md#frontend).

## Versions

Every version is stated once, in the `catalog:` of `pnpm-workspace.yaml`; a package's
`package.json` says `"catalog:"` and never repeats a range. The workspace expects Node 24 or newer
and pnpm 12.9.1 (`engines.node`, `devEngines.packageManager`), and the lockfile is
`pnpm-lock.yaml`.

## Boundaries

`packages/ui` exposes `src/index.ts` (explicit named exports) and `src/styles/globals.css` only.
Deep imports such as `@mysite/ui/components/ui/button` fail both package resolution and ESLint.
`packages/api-client` exposes `src/index.ts` only for the same reason, and only the `_pages` layer
may import it: a slice's server-side data function is the client's single caller.
`apps/web` follows the FSD direction `app → _pages → widgets → features → entities → shared`, and
`eslint-plugin-boundaries` plus `import/no-restricted-paths` enforce it.

## Git hook

`pnpm install` runs `scripts/setup-git-hooks.mjs`, which points `core.hooksPath` at `.githooks`.
Git runs hooks from the repository root, while lefthook looks for its config there too; this
workspace's config lives in `frontend/`, so the committed hook passes `LEFTHOOK_CONFIG` explicitly.
Lint runs on the staged files, and `tsc --noEmit` runs over the whole project when a `.ts`/`.tsx`
file is staged.
