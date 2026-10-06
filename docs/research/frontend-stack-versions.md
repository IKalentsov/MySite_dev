# The frontend stack: exact versions and what they require

The question: what are the current stable versions of the tools this project's frontend is built
from, are they compatible with each other, do pnpm workspace packages need
`transpilePackages`, what exactly gets pinned and where, and how is the dark theme actually done
today? This note holds the evidence and the sources; the decision itself belongs to
[ADR-0004](../adr/0004-nextjs-app-router-and-fsd-frontend.md) and the frontend rules in
`.dsh/AGENTS.md`.

Method: every version below comes from the npm registry as it is published by the maintaining
organisation (`https://registry.npmjs.org/<package>/latest` — the manifest the package manager
itself will download), and every compatibility claim comes from the project's own documentation.
No version was taken from a blog post or from memory. Where a version could not be confirmed, the
sentence says so.

## What the primary sources actually say

### The versions, straight from the registry

Each row is the `dist-tags.latest` of the package as published on npm.

| Package | Current stable | Source |
|---|---|---|
| `next` | **16.4.0** | [registry.npmjs.org/next/latest](https://registry.npmjs.org/next/latest) |
| `react` | **19.3.0** | [registry.npmjs.org/react/latest](https://registry.npmjs.org/react/latest) |
| `react-dom` | **19.3.0** | [registry.npmjs.org/react-dom/latest](https://registry.npmjs.org/react-dom/latest) |
| `typescript` | **7.0.2** (`latest`) — but see below | [registry.npmjs.org/typescript/latest](https://registry.npmjs.org/typescript/latest) |
| `tailwindcss` | **4.3.3** | [registry.npmjs.org/tailwindcss/latest](https://registry.npmjs.org/tailwindcss/latest) |
| `@tailwindcss/postcss` | **4.3.3** | [registry.npmjs.org/@tailwindcss/postcss/latest](https://registry.npmjs.org/@tailwindcss/postcss/latest) |
| `shadcn` (the CLI; shadcn/ui itself is copied source, not a dependency) | **4.21.3** | [registry.npmjs.org/shadcn/latest](https://registry.npmjs.org/shadcn/latest) |
| `next-themes` | **0.4.6** | [registry.npmjs.org/next-themes/latest](https://registry.npmjs.org/next-themes/latest) |
| `eslint` | **10.12.0** | [registry.npmjs.org/eslint/latest](https://registry.npmjs.org/eslint/latest) |
| `typescript-eslint` | **8.71.1** | [registry.npmjs.org/typescript-eslint/latest](https://registry.npmjs.org/typescript-eslint/latest) |
| `prettier` | **3.9.9** | [registry.npmjs.org/prettier/latest](https://registry.npmjs.org/prettier/latest) |
| `pnpm` | **12.9.1** | [registry.npmjs.org/pnpm/latest](https://registry.npmjs.org/pnpm/latest) |

Two of these need to be read carefully, because `latest` is not the same as "the version this
project should pin".

### TypeScript: `latest` is 7.0.2, and the linter cannot read it

TypeScript 7.0 is a real, stable release. The official announcement,
[Announcing TypeScript 7.0](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0/),
opens with:

> "Today we are proud to announce the availability of TypeScript 7, a 10x faster native port of
> TypeScript!"

and describes the port as follows:

> "The mission was a native port of TypeScript built in Go that could make the most of modern
> hardware. This port was done as faithfully as possible, writing new code while maintaining the
> structure and logic of the original codebase to keep results consistent and compatible between
> the two compilers."

TypeScript 6.0 is the last release on the old codebase, per
[Announcing TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/):

> "TypeScript 6.0 is a unique release in that we intend for it to be the last release based on the
> current JavaScript codebase."

> "TypeScript 6.0 acts as the bridge between TypeScript 5.9 and 7.0."

The npm registry's own tags for `typescript` are `latest: 7.0.2`, `rc: 7.0.1-rc`,
`beta: 6.0.0-beta`, `next: 7.1.0-dev.20261006.1`
([registry.npmjs.org/-/package/typescript/dist-tags](https://registry.npmjs.org/-/package/typescript/dist-tags)).

The reason 7.0.2 cannot simply be pinned is the linter. `typescript-eslint` — the parser and
plugin behind the project's required typed linting — states its supported range on its own page,
[Dependency Versions](https://typescript-eslint.io/users/dependency-versions/):

> "The version range of TypeScript currently supported is `>=4.8.4 <6.1.0`."

That is also the literal `peerDependencies` value in the published `typescript-eslint@8.71.1`
manifest ([registry.npmjs.org/typescript-eslint/latest](https://registry.npmjs.org/typescript-eslint/latest)):

> `"peerDependencies":{"eslint":"^8.57.0 || ^9.0.0 || ^10.0.0","typescript":">=4.8.4 <6.1.0"}`

The same page explains what happens if you run outside the range:

> "If you use a non-supported version of TypeScript, the parser will log a warning to the console."

So `latest` (7.0.2) is outside the supported window of the linter this project's rules command.
The highest TypeScript version **inside** that window is on the 6.0 line. I confirmed the
redirects directly: `6.0.3` returns a manifest and `6.0.4` and `6.1.0` both return
`version not found` ([typescript@6.0.3](https://registry.npmjs.org/typescript/6.0.3)):

> `"engines":{"node":">=14.17"}` … `"_id":"typescript@6.0.3","version":"6.0.3"`

### Node.js and the runtime requirements

Next.js's own numbers come from the framework's documentation. The installation page
([Getting Started: Installation](https://nextjs.org/docs/app/getting-started/installation))
states, under System requirements:

> "Minimum Node.js version: 20.9"

and the version-16 upgrade guide
([Upgrading: Version 16](https://nextjs.org/docs/app/guides/upgrading/version-16)) repeats it:

> "Node.js 20.9+ | Minimum version now `20.9.0` (LTS); Node.js 18 no longer supported"

Neither page documents an upper bound, so no maximum supported Node version can be quoted from a
primary source.

`next@16.4.0` also declares its own runtime and peer expectations in the published manifest
([registry.npmjs.org/next/latest](https://registry.npmjs.org/next/latest)):

> `"engines":{"node":">=20.9.0"}`

> `"peerDependencies":{"sass":"^1.3.0","react":"^18.2.0 || 19.0.0-rc-de68d2f4-20241204 || ^19.0.0","react-dom":"^18.2.0 || 19.0.0-rc-de68d2f4-20241204 || ^19.0.0", ...}`

`react-dom@19.3.0` pins its own reactor peer tightly
([registry.npmjs.org/react-dom/latest](https://registry.npmjs.org/react-dom/latest)):

> `"peerDependencies":{"react":"^19.3.0"}`

ESLint 10 narrows the Node window considerably — it is stricter than Next.js
([registry.npmjs.org/eslint/latest](https://registry.npmjs.org/eslint/latest)):

> `"engines":{"node":"^20.19.0 || ^22.13.0 || >=24"}`

The other Node floors, each from its own manifest:
`shadcn@4.21.3` → `"engines":{"node":">=20.18.1"}`;
`typescript-eslint@8.71.1` → `"engines":{"node":"^18.18.0 || ^20.9.0 || >=21.1.0"}`;
`pnpm@12.9.1` → `"engines":{"node":">=18.*"}`;
`prettier@3.9.9` → `"engines":{"node":">=14"}`.

### React, and one wrinkle worth reading before pinning it

The version-16 upgrade guide says which React Next.js 16 actually runs:

> "The App Router in **Next.js 16** uses the latest React Canary release, which includes the newly
> released React 19.2 features and other features being incrementally stabilized."

The installation page adds why `react` still belongs in `package.json` even though the framework
ships its own copy:

> "The `App Router` uses React canary releases built-in, which include all the stable React 19
> changes, as well as newer features being validated in frameworks, but you should still declare
> react and react-dom in package.json for tooling and ecosystem compatibility."

No Next.js page states a numeric minimum React version. The published peer range
(`^18.2.0 || ^19.0.0`) accepts 19.3.0, but the documentation's own account is that the App Router
runs a Canary build with React 19.2 features — not the exact published `react@19.3.0` — while
asking you to declare a React in `package.json` for tooling. That is a real ambiguity in the
primary sources, not something I can resolve from them (see the reasoning section).

### Tailwind CSS 4: CSS-first configuration

The Tailwind documentation itself is version 4.3 (the site header reads "v4.3"). Its install path
for a framework is the PostCSS plugin: `@tailwindcss/postcss@4.3.3` depends on `postcss@^8.5.16`
and `tailwindcss@4.3.3` ([registry.npmjs.org/@tailwindcss/postcss/latest](https://registry.npmjs.org/@tailwindcss/postcss/latest)):

> `"dependencies":{"postcss":"^8.5.16","tailwindcss":"4.3.3","@alloc/quick-lru":"^5.2.0","@tailwindcss/node":"4.3.3","@tailwindcss/oxide":"4.3.3"}`

Configuration is CSS, not a JavaScript config object. From
[Theme variables](https://tailwindcss.com/docs/theme):

> "Theme variables are special CSS variables defined using the `@theme` directive that influence
> which utility classes exist in your project."

> "Theme variables are also required to be defined top-level and not nested under other selectors
> or media queries, and using a special syntax makes it possible to enforce that."

The same page documents the monorepo case this project needs — the design system's tokens living
in `packages/ui` and imported by the app:

> "Since theme variables are defined in CSS, sharing them across projects is just a matter of
> throwing them into their own CSS file that you can import in each project"

> "You can put shared theme variables like this in their own package in monorepo setups or even
> publish them to NPM and import them just like any other third-party CSS files."

### Dark mode: how it is actually done

Two primary sources answer this, and they compose.

**Tailwind's side.** From [Dark mode](https://tailwindcss.com/docs/dark-mode), the default is the
media query:

> "By default this uses the `prefers-color-scheme` CSS media feature, but you can also build sites
> that support toggling dark mode manually by overriding the dark variant."

And the override is an explicit CSS declaration — this is the part that changed in v4 and is easy
to get wrong:

> ```css
> @import "tailwindcss";
>
> @custom-variant dark (&:where(.dark, .dark *));
> ```

> "Now instead of `dark:*` utilities being applied based on `prefers-color-scheme`, they will be
> applied whenever the `dark` class is present earlier in the HTML tree"

Tailwind also documents the data-attribute variant
(`@custom-variant dark (&:where([data-theme=dark], [data-theme=dark] *));`) and a three-way
light/dark/system toggle built on `window.matchMedia()`, ending with:

> "How you add the `dark` class to the `html` element is up to you, but a common approach is to use
> a bit of JavaScript that updates the `class` attribute and syncs that preference to somewhere
> like `localStorage`."

**shadcn/ui's side.** shadcn/ui is not a component dependency — the CLI copies component source
into the repository. Its official Next.js dark-mode page
([Dark mode — Next.js](https://ui.shadcn.com/docs/dark-mode/next)) prescribes `next-themes` and
gives the exact wiring: install `next-themes`, create a client `ThemeProvider` that wraps
`NextThemesProvider`, put it in the root layout, and add `suppressHydrationWarning` to the `html`
tag:

> ```tsx
> <html lang="en" suppressHydrationWarning>
>   …
>   <ThemeProvider
>     attribute="class"
>     defaultTheme="system"
>     enableSystem
>     disableTransitionOnChange
>   >
> ```

with a `ModeToggle` component driven by `useTheme()` for the light/dark/system choice. The
`attribute="class"` is what closes the loop with Tailwind's `.dark` selector above: `next-themes`
writes the class, Tailwind's overridden `dark` variant responds to it. `next-themes@0.4.6`
declares `"peerDependencies":{"react":"^16.8 || ^17 || ^18 || ^19 || ^19.0.0-rc", ...}`, so React
19 is accepted ([registry.npmjs.org/next-themes/latest](https://registry.npmjs.org/next-themes/latest)).

### `transpilePackages` in a pnpm workspace

This is the question with the most surprising answer. From the `transpilePackages` reference
([next.config.js: transpilePackages](https://nextjs.org/docs/app/api-reference/config/next-config-js/transpilePackages)):

> "Use `transpilePackages` to compile and bundle a dependency instead of treating it as untouched
> runtime code. Values are package names, including scoped names like `@scope/pkg`. Paths and glob
> patterns are not supported."

> "This replaces the `next-transpile-modules` package."

The decisive sentence is under "When you need it":

> "Turbopack transpiles workspace packages (npm, pnpm, or Yarn workspaces) in your monorepo
> automatically under both routers. Webpack does the same for the App Router."

The documentation then lists the cases where an opt-in is still required:

> "A `node_modules` dependency ships raw TypeScript or JSX. Next.js does not compile code inside
> `node_modules` by default. Listing the package opts it in, or you can build the package to plain
> JavaScript and point its `main`/`exports` at the compiled output."

> "You build with webpack for the Pages Router and the dependency's source lives outside the next
> app's directory. For example, an `apps/web` app importing `packages/ui` in the same monorepo."

> "You use the Pages Router and want a `node_modules` dependency bundled into the route."

So the honest reading is: for `apps/web` importing `packages/ui` and `packages/api-client` in a
pnpm workspace under the App Router with Turbopack, the documentation says transpilation of those
workspace packages happens **automatically** and no list is needed. `transpilePackages` is
documented as necessary for the Pages-Router-plus-webpack case and for uncompiled packages
reaching the app through `node_modules` — not for App Router workspace packages.

Two caveats I could not close from a primary source. First, the guide
[Package Bundling](https://nextjs.org/docs/app/guides/package-bundling) does not mention
`transpilePackages`, monorepo packages or filesystem-local packages at all — the only automatic
transpilation statement in the documentation is the single `transpilePackages` sentence quoted
above. Second, the documentation's statement is about *transpiling* workspace packages; it does
not say anything about how a workspace package's CSS (the Tailwind `@theme` file in `packages/ui`)
reaches the app's stylesheet, which is an `@import` question, not a transpilation one.

### What is pinned, and where

The project rule (`.dsh/AGENTS.md`) is "**A package version is stated in exactly one place**" and
"**Versions are pinned exactly.** Floating tags, ranges and `latest` are out." The relevant
mechanics, from pnpm's own documentation:

**`catalog:` — one place for versions across a workspace.** From
[Catalogs](https://pnpm.io/catalogs):

> "Catalogs are a workspace feature for defining dependency version ranges as reusable constants."

> "Catalogs reduce duplication when authoring `package.json` files and provide a few benefits in
> doing so: … **Maintain unique versions** … **Easier upgrades** — When upgrading a dependency,
> only the catalog entry in `pnpm-workspace.yaml` needs to be edited rather than all `package.json`
> files using that dependency."

The definition site is `pnpm-workspace.yaml`, and the reference site is `"react": "catalog:"`.
The `catalog:` protocol is accepted in `dependencies`, `devDependencies`, `peerDependencies` and
`optionalDependencies`.

**`engines`.** From [package.json](https://pnpm.io/package_json):

> "During local development, pnpm will always fail with an error message if its version does not
> match the one specified in the `engines` field."

with the documented shape `{"engines": {"node": ">=10", "pnpm": ">=3"}}`. The same page documents
that since pnpm 11 settings no longer live in a `pnpm` field:

> "Since v11, pnpm no longer reads settings from the `pnpm` field of `package.json`. Settings must
> be defined in `pnpm-workspace.yaml` instead."

**`packageManager` and its replacement.** The same page documents `devEngines.packageManager`
(added in pnpm v11.0.0) as the range-capable form, with the legacy field noted as
"the legacy `packageManager` field":

> "Allows specifying the pnpm version via `devEngines.packageManager` in `package.json`. Unlike the
> `packageManager` field, this supports version ranges."

```json
{
  "devEngines": {
    "packageManager": {
      "name": "pnpm",
      "version": ">=12.0.0 <13.0.0",
      "onFail": "download"
    }
  }
}
```

There is also `devEngines.runtime` / `engines.runtime` (added in v10.14 / v10.21.0) for pinning the
Node runtime itself, with `"onFail": "download"`. Note that pnpm's own position on ranges is a
range — which conflicts with this project's "pinned exactly, no ranges" rule, so the field to use
is a decision, not a documented default.

## My reasoning, and what I could not confirm

Everything below is my own conclusion or my own uncertainty. Nothing here is a quote.

### The recommended pin set

The pin that matters most is the one where `latest` is the wrong answer.

| Package | Pin | Why |
|---|---|---|
| `next` | `16.4.0` | Current stable; registry `latest`. |
| `react` | `19.3.0` | Current stable. Inside Next's peer range `^19.0.0`. See the caveat below. |
| `react-dom` | `19.3.0` | Locked to `react@19.3.0` by its own `peerDependencies`. |
| `typescript` | `6.0.3` | **Not `latest`.** `latest` is 7.0.2, outside `typescript-eslint`'s documented `>=4.8.4 <6.1.0` window; 6.0.3 is the newest confirmed 6.0 release. |
| `typescript-eslint` | `8.71.1` | Current stable; supplies the typed-linting parser. |
| `eslint` | `10.12.0` | Current stable; inside typescript-eslint's peer range `^8.57.0 \|\| ^9.0.0 \|\| ^10.0.0`. |
| `prettier` | `3.9.9` | Current stable; no constraint found in the primary sources that excludes it. |
| `tailwindcss` | `4.3.3` | Current stable. |
| `@tailwindcss/postcss` | `4.3.3` | The v4 PostCSS plugin; version-matched to `tailwindcss`. |
| `shadcn` | `4.21.3` | Current CLI. It is a `devDependency` used to generate code, not a runtime dependency. |
| `next-themes` | `0.4.6` | The theme mechanism shadcn's official Next.js dark-mode page prescribes. |
| `pnpm` | `12.9.1` (or `12.5.1` to match what is installed) | Registry `latest` is 12.9.1; the environment has 12.5.1. Both are on the 12.x line. |

For a Node pin: the narrowest floor in the set is ESLint 10's
`^20.19.0 || ^22.13.0 || >=24`, and Next.js's floor is `20.9`. The installed Node 24.18.0
satisfies every floor quoted above. I did not find a documented maximum anywhere, so a Node pin is
a choice between `24.18.0` (exactly what is installed) and a documented LTS line; the primary
sources do not choose for you.

`.nvmrc` is not mentioned anywhere in the primary sources I read. It is a widely used convention,
not a documented requirement, so writing it is a project decision — `.nvmrc` for the human
toolchain and `engines`/`devEngines.runtime` for the package manager are two different
declarations of the same fact, and this project's "one meaning, one place" rule argues for
knowing which one is authoritative rather than writing both casually.

### Why I would put the versions in a pnpm catalog

The project rule says root `package.json`. pnpm's catalog puts the literal version strings in
`pnpm-workspace.yaml` instead, with `"catalog:"` in each `package.json`. Both satisfy "one place,
exactly once"; the catalog is the stronger form of it, because it also stops `apps/web` and
`packages/ui` from drifting onto different React or TypeScript versions, which is exactly the kind
of duplicate the documentation says "can conflict at runtime and cause bugs". If `transpilePackages`
were ever needed, a version split across two workspace packages is also a common way to get there.

Because this touches the project's stated rule ("pinned in the monorepo root `package.json`"), it
should be a recorded decision rather than an implementation detail whichever way it goes.

### The React pin is genuinely uncertain

The primary sources say two different things. The published `react@19.3.0` is what the registry
calls `latest` and what Next.js's peer range accepts. The Next.js documentation says the App
Router "uses the latest React Canary release, which includes the newly released React 19.2
features" while telling you to declare React in `package.json` "for tooling and ecosystem
compatibility". Those are not contradictory in principle — the framework may bundle a Canary
internally while your declared React drives types and ecosystem tooling — but no primary source I
found states which published React version is the correct declaration for a Next.js 16.4.0 App
Router project. I am not asserting that `19.3.0` is safe here; I am asserting that it is `latest`
and inside the documented peer range, and that the Canary/declared distinction is a real gap in
the documentation. This is worth verifying empirically once the frontend exists.

### `transpilePackages`: what I would do with the answer

The documentation is clear enough to act on: with the App Router and Turbopack, workspace packages
are transpiled automatically, so an empty or absent `transpilePackages` is the documented starting
point for `packages/ui` and `packages/api-client`. I would not add the field speculatively; the
documentation names the conditions under which it is needed, and neither of them (Pages Router with
webpack; an uncompiled package arriving through `node_modules`) is this project's shape. If the
frontend is ever switched to webpack, or a package is consumed the way a published dependency is,
the field returns — that is the trigger to watch, not a preference.

### Dark theme: what "part of the design system, not an afterthought" means here

The two primary sources compose cleanly into one mechanism, and it is small:

1. `@custom-variant dark (&:where(.dark, .dark *));` in the app's CSS, so `dark:*` follows a class
   rather than `prefers-color-scheme`.
2. `next-themes` with `attribute="class"`, `defaultTheme="system"`, `enableSystem` and
   `disableTransitionOnChange` in the root layout, plus `suppressHydrationWarning` on `<html>`.
3. Design tokens as CSS variables under `@theme` in `packages/ui`, imported by the app — the
   documented monorepo sharing pattern — with both light and dark values defined as tokens rather
   than as `dark:` overrides scattered through components.

Point 3 is my reading, not a quote: the primary sources document the *mechanism* for dark mode and
the *mechanism* for sharing tokens across a monorepo, but neither states that a design system
should express its dark theme as a second set of token values. That is the design-system
conclusion the project's own rule ("colours, spacing and radii come from tokens") pushes toward,
and it is the one place where this note goes beyond what was read.

### Things I could not confirm

- No primary source states a maximum supported Node.js version for Next.js 16, ESLint 10 or
  Tailwind 4. My compatibility statement is therefore "the floors are satisfied", not "the version
  is supported".
- `typescript-eslint`'s page sells the unsupported-version behaviour as a *warning*, not an error
  ("the parser will log a warning to the console"), and documents
  `onUnsupportedTypeScriptVersion` to turn it into a failure. I did not verify whether type-aware
  rules actually work correctly against TypeScript 7.0.2 in practice — only that the supported
  range excludes it. The conservative pin (6.0.3) follows from the documented range, not from a
  measurement.
- I could not confirm the exact newest TypeScript 6.0 patch by exhaustively listing versions; I
  confirmed that `6.0.3` exists and that `6.0.4` and `6.1.0` do not, which combined with the
  published `6.0.3` being the version other projects were bumped to makes `6.0.3` the best
  confirmed candidate. A version-number-by-version-number listing was not obtained.
- The exact Tailwind v4 installation snippets for Next.js (the `postcss.config.mjs` contents and
  the `@import "tailwindcss";` entry point) could not be quoted: the documentation page truncated
  before the install steps, and the site's source repository does not carry that page's Markdown
  at the path I tried. The PostCSS plugin's identity and version-match to `tailwindcss` are
  confirmed from its published manifest; the surrounding file contents are not.
- The local environment could not be inspected. `node --version` and `pnpm --version` were denied
  by the sandbox (`SetNamedSecurityInfoW failed (Win32 5)` on the workspace grant), so the claim
  that Node 24.18.0 and pnpm 12.5.1 are installed is taken from the ticket, not verified here. It
  is consistent with the registry: `tailwindcss@4.3.3` was published with
  `"_nodeVersion":"24.18.0"`, and `shadcn@4.21.3` with `"_npmVersion":"12.2.0"`.
- No primary source was found that states whether `shadcn@4.21.3`'s generated components target
  React 19 specifically; the CLI's manifest declares `devDependencies` including
  `typescript: ^5.9.2` and shows no React peer constraint, but the generated component source is
  not in the manifest. The shadcn documentation pages I fetched were also truncated by navigation
  before the compatibility prose.
