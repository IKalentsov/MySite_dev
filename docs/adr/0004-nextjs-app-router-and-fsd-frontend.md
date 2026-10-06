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

Two things the base never decided are now genuinely open, and they are marked as open questions in
`.dsh/AGENTS.md` rather than guessed:

- **Data fetching.** "A query library only" was a rule about a browser-only application. Server
  Components make server-side fetching, a query library and a mix all defensible, and the choice
  shapes every screen.
- **Where components live.** shadcn/ui copies components into the repository, which makes them
  project code subject to the quality gates; whether they sit in the design system or inside the
  FSD layers is open.

FSD names its top layer `app`, which collides with the App Router's `app/` directory. How the two
are kept apart is a third open question; the answer changes every import path in the project, so
it is worth settling before the first component is written.
