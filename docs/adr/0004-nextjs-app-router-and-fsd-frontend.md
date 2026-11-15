# The frontend is Next.js with the App Router and Feature-Sliced Design

The harness base's reference frontend is a Vite SPA in a pnpm monorepo. This project overrides
that. The site is built on Next.js with the App Router, TypeScript, Tailwind CSS and shadcn/ui,
with the code organised by Feature-Sliced Design. Owner's decision, 2026-10-07: the site is meant
to grow into a personal product, and for a content-facing site server rendering, file-based
routing and a component library that is accessible out of the box are worth more than the
simplicity of the SPA the base assumes.

## Considered options

- **Follow the base — Vite SPA, React Router, a design system written in the repository** —
  rejected by the owner. It is the smaller machine and it matches the base, but it means hand-
  building the components and shipping a client-rendered shell for pages that are mostly text.
- **Next.js App Router, Tailwind, shadcn/ui, Feature-Sliced Design** — chosen.

## Consequences

The base's frontend principles were written for an SPA, so they split in two. The stack-agnostic
half — the four mandatory screen states, WCAG 2.2 AA, the import direction between layers, the
quality gates, the test levels — keeps its force. The half that named Vite, React Router, a pnpm
`apps/` monorepo, nuqs and Zustand has been rewritten for the App Router and FSD in
`.dsh/AGENTS.md`.

One thing the base never decided is still open, and it is marked as an open question in
`.dsh/AGENTS.md` rather than guessed:

- **Data fetching.** "A query library only" was a rule about a browser-only application. Server
  Components make server-side fetching, a query library and a mix all defensible, and the choice
  shapes every screen. This is [ticket 18](../../.scratch/site-development/issues/18-data-fetching-boundary.md).

The component home and the `app` collision are settled (owner's decision, 2026-11-16,
[ticket 13](../../.scratch/site-development/issues/13-component-home-and-tokens.md)):

- **Components live in `packages/ui`.** shadcn/ui copies its components into the repository, so the
  design system owns the primitives, `cn`, the tokens and the styles; app-specific composition
  lives in the FSD layers of `apps/web`. `packages/ui` exposes only `src/index.ts` and
  `src/styles/globals.css`; deep imports are blocked by the package's `exports` map and by ESLint.
- **The App Router's `app/` directory is the composition root.** Routes live in
  `apps/web/src/app/`; the FSD layers live in
  `apps/web/src/{_pages,widgets,features,entities,shared}`. There is no separate FSD `app` layer,
  and the FSD `pages` layer is `_pages`, because `src/pages` is the Pages Router directory.
- **Tokens live in `packages/ui/src/styles/globals.css`** under Tailwind 4 `@theme`, with the
  semantic variables in `:root`/`.dark`; the dark theme is `@custom-variant dark` plus
  `next-themes`, as [ticket 09](../../.scratch/site-development/issues/09-frontend-stack-and-versions.md)
  established.
