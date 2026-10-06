# The frontend quality and test contour — the evidence

The question: [ADR-0004](../adr/0004-nextjs-app-router-and-fsd-frontend.md) left the test runner
listed as Vitest, unconfirmed for Next.js. The project's rules require four test levels
(unit / component with React Testing Library / integration with MSW / end-to-end with Playwright),
ESLint on a flat config with typescript-eslint, Prettier, and a git hook that runs lint and types
on changed files — plus a linter rule, not discipline, holding the Feature-Sliced Design import
direction. This note holds the evidence and the sources; it is the input to a decision, not the
decision.

Everything below is anchored to a primary source: the tool's own documentation, its own repository,
or the package registry's own published metadata. Numbers are reported only where a source was
actually retrieved during this research, and every unverified point is listed in
[Not confirmed from a primary source](#not-confirmed-from-a-primary-source).

**A note on the version landscape.** Every source retrieved is stamped in the 2026 line: the
Next.js documentation pages report `version: 16.4.0`, Vitest 5.0 was released on 3 September 2026,
and ESLint 10 removed the `.eslintrc` format. The stack this project is about to build is therefore
the 2026 stack, not the 2024 one most write-ups describe. That is why the next section matters
more than usual: several things that "everyone knows" about this stack changed in the last major
round.

## What the primary sources actually say

### 1. Vitest versus Jest on the App Router

#### Next.js documents both, and documents the difference

The App Router testing overview
([Testing](https://nextjs.org/docs/app/guides/testing)) lists four tools and states which level each
is for:

> "Next.js with four commonly used testing tools — Cypress, Playwright, Vitest, and Jest."

> "- [Jest](/docs/app/guides/testing/jest) — Learn how to set up Jest with Next.js for Unit Testing
> and Snapshot Testing.
> - [Playwright](/docs/app/guides/testing/playwright) — Learn how to set up Playwright with Next.js
> for End-to-End (E2E) Testing.
> - [Vitest](/docs/app/guides/testing/vitest) — Learn how to set up Vitest with Next.js for Unit
> Testing."

So both runners are officially supported and both have a maintained setup guide. There is no
deprecation of either.

#### The decisive sentence is about `async` Server Components — and it is the same for both

The testing overview, in full:

> "## Async Server Components
>
> Since `async` Server Components are new to the React ecosystem, some tools do not fully support
> them. In the meantime, we recommend using **End-to-End Testing** over **Unit Testing** for `async`
> components."

The Vitest guide ([How to set up Vitest with Next.js](https://nextjs.org/docs/app/guides/testing/vitest)):

> "> **Good to know:** Since `async` Server Components are new to the React ecosystem, Vitest
> currently does not support them. While you can still run **unit tests** for synchronous Server and
> Client Components, we recommend using **E2E tests** for `async` components."

The Jest guide ([How to set up Jest with Next.js](https://nextjs.org/docs/app/guides/testing/jest))
says the same thing about Jest:

> "> **Good to know:** Since `async` Server Components are new to the React ecosystem, Jest
> currently does not support them. While you can still run **unit tests** for synchronous Server and
> Client Components, we recommend using an **E2E tests** for `async` components."

This is the finding that matters most for the ticket. **The choice between Vitest and Jest does not
change what is testable.** Neither runner is officially supported for `async` Server Components, and
Next.js points at E2E for those in both cases. "Is Vitest supported for Next.js?" has the answer
*yes, for unit and component testing of synchronous components* — the same answer Jest gets.

#### How Vitest is configured, per the Next.js guide

Installation, verbatim (the guide gives all four package managers; the pnpm variant is the one this
project would use):

> "```bash package="pnpm"
> # Using TypeScript
> pnpm add -D vitest @vitejs/plugin-react jsdom @testing-library/react @testing-library/dom vite-tsconfig-paths
> ```"

The config file:

> "Create a `vitest.config.mts|js` file in the root of your project, and add the following options:"

> "```ts filename="vitest.config.mts" switcher
> import { defineConfig } from 'vitest/config'
> import react from '@vitejs/plugin-react'
> import tsconfigPaths from 'vite-tsconfig-paths'
>
> export default defineConfig({
>   plugins: [tsconfigPaths(), react()],
>   test: {
>     environment: 'jsdom',
>   },
> })
> ```"

The script and the first test:

> "```json filename="package.json"
> {
>   "scripts": {
>     "dev": "next dev",
>     "build": "next build",
>     "start": "next start",
>     "test": "vitest"
>   }
> }
> ```"

> "When you run `npm run test`, Vitest will **watch** for changes in your project by default."

> "```tsx filename="__tests__/page.test.tsx" switcher
> import { expect, test } from 'vitest'
> import { render, screen } from '@testing-library/react'
> import Page from '../app/page'
>
> test('Page', () => {
>   render(<Page />)
>   expect(screen.getByRole('heading', { level: 1, name: 'Home' })).toBeDefined()
> })
> ```"

> "> **Good to know**: The example above uses the common `__tests__` convention, but test files can
> also be colocated inside the `app` router."

Note what is *not* in that config: no `next/jest` equivalent, no transformer, no aliasing beyond
`vite-tsconfig-paths`. Vitest compiles through Vite with the React plugin and needs the DOM
environment named explicitly. The Next.js guide's Quickstart also points at an official example —
`pnpm create next-app --example with-vitest with-vitest-app` — which is the supported scaffold.

#### How Jest is configured, per the Next.js guide

> "Since the release of [Next.js 12](https://nextjs.org/blog/next-12), Next.js now has built-in
> configuration for Jest."

Installation:

> "```bash package="npm"
> npm install -D jest jest-environment-jsdom @testing-library/react @testing-library/dom @testing-library/jest-dom ts-node @types/jest
> ```"

The config, which is still `next/jest`:

> "Update your config file to use `next/jest`. This transformer has all the necessary configuration
> options for Jest to work with Next.js:
>
> ```ts filename="jest.config.ts" switcher
> import type { Config } from 'jest'
> import nextJest from 'next/jest.js'
>
> const createJestConfig = nextJest({
>   // Provide the path to your Next.js app to load next.config.js and .env files in your test environment
>   dir: './',
> })
>
> // Add any custom config to be passed to Jest
> const config: Config = {
>   coverageProvider: 'v8',
>   testEnvironment: 'jsdom',
>   // Add more setup options before each test is run
>   // setupFilesAfterEnv: ['<rootDir>/jest.setup.ts'],
> }
>
> // createJestConfig is exported this way to ensure that next/jest can load the Next.js config which is async
> export default createJestConfig(config)
> ```"

And what `next/jest` buys, verbatim:

> "Under the hood, `next/jest` is automatically configuring Jest for you, including:
>
> * Setting up `transform` using the [Next.js Compiler](/docs/architecture/nextjs-compiler).
> * Auto mocking stylesheets (`.css`, `.module.css`, and their scss variants), image imports and
> [`next/font`](/docs/app/api-reference/components/font).
> * Loading `.env` (and all variants) into `process.env`.
> * Ignoring `node_modules` from test resolving and transforms.
> * Ignoring `.next` from test resolving.
> * Loading `next.config.js` for flags that enable SWC transforms."

This is the one genuine capability Jest has and Vitest does not: out-of-the-box mocking of CSS
imports, image imports and `next/font`. With Vitest those become config the project writes itself.
The Vitest guide contains no equivalent list.

#### `next/jest`'s own status

`next/jest` is documented as the supported path in the Jest guide, and there is **no primary source
saying it is deprecated**. The only dedicated page about its status,
[experimental-jest-transformer](https://nextjs.org/docs/messages/experimental-jest-transformer),
still frames it as pre-stable:

> "You are using `next/jest` which is currently an experimental feature of Next.js. In a future
> version of Next.js `next/jest` will be marked as stable."

#### `create-next-app` has no test-runner prompt

The
[`create-next-app` CLI reference](https://nextjs.org/docs/app/api-reference/cli/create-next-app)
documents the interactive prompts in full, and no test runner appears among them:

> "Would you like to use TypeScript? No / Yes
> Which linter would you like to use? ESLint / Biome / None
> Would you like to use React Compiler? No / Yes
> Would you like to use Tailwind CSS? No / Yes
> Would you like your code inside a `src/` directory? No / Yes
> Would you like to use App Router? (recommended) No / Yes
> Would you like to use Cache Components? No / Yes
> Would you like to customize the import alias (`@/*` by default)? No / Yes
> What import alias would you like configured? @/*
> Would you like to include AGENTS.md to guide coding agents to write up-to-date Next.js code? No / Yes"

Testing setup is offered by *example* instead (`--example with-vitest`, `--example with-jest`), not
by a prompt. Neither the Next.js 16 nor the Next.js 16.4 release post mentions Vitest or Jest.

#### Versions

Retrieved from the package registry's own `latest` metadata during this research:

| Package | Version retrieved | Source |
|---|---|---|
| `next` | 16.4.0 | `https://registry.npmjs.org/next/latest` |
| `vitest` | 5.0.3 | `https://registry.npmjs.org/vitest/latest` |
| `jest` | 30.5.2 | `https://registry.npmjs.org/jest/latest` |
| `@testing-library/react` | 16.3.3 | `https://registry.npmjs.org/@testing-library/react/latest` |

And from the official Vitest documentation, the current floor:

> "Vitest requires Vite >=v6.4.0 and Node >=v22.12.0"

([Vitest — Getting Started](https://vitest.dev/guide/)); Vitest 5's own release post repeats it:
"Vitest 5 requires Vite >= 6.4.0 and Node.js >= 22.12.0"
([Vitest 5.0 is out!](https://vitest.dev/blog/vitest-5.html)). The published `vitest@5.0.3` metadata
carries `"engines":{"node":"^22.12.0 || ^24.0.0 || >=26.0.0"}` and `"vite"` as a **required peer
dependency** — the migration guide states it outright: "`vite` is no longer a direct dependency of
`vitest`. It is now a required peer dependency"
([Migrating to Vitest 5.0](https://vitest.dev/guide/migration/)).

Two configuration facts worth stating because they are recent and easy to get wrong:

- **`vitest.workspace.ts` is out.** [Test Projects](https://vitest.dev/guide/projects) says:
  "This feature is also known as a `workspace`. The `workspace` is deprecated since 3.2 and replaced
  with the `projects` configuration. They are functionally the same." The current config reference
  lists `projects` and no `workspace` entry.
- **In Vitest 5, inline projects inherit the root config by default.** Same page: "Projects defined
  with an inline configuration inherit all options from the root-level configuration. This is
  controlled by the `extends` option, which is enabled by default since Vitest 5.0".

There is **no official Vitest documentation page about Next.js.** Vitest's own example list
(`basic, fastify, in-source-test, lit, vue, marko, preact, qwik, react, solid, svelte, profiling,
typecheck, projects`) contains no Next.js entry — the Next.js guidance exists only on the Next.js
side. Likewise, **React's own documentation contains no testing guide**; the only test-related
reference page is [`act`](https://react.dev/reference/react/act), which says nothing about Next.js,
Vitest, Jest or Server Components.

### 2. Component testing with React Testing Library

The design goal is stated on the
[RTL introduction](https://testing-library.com/docs/react-testing-library/intro):

> "You want to write maintainable tests for your React components. As a part of this goal, you want
> your tests to avoid including implementation details of your components and rather focus on making
> your tests give you the confidence for which they are intended. As part of this, you want your
> testbase to be maintainable in the long run so refactors of your components (changes to
> implementation but not functionality) don't break your tests and slow you and your team down."

And the method:

> "So rather than dealing with instances of rendered React components, your tests will work with
> actual DOM nodes. The utilities this library provides facilitate querying the DOM in the same way
> the user would. Finding form elements by their label text (just like a user would), finding links
> and buttons from their text (like a user would)."

The [Guiding Principles](https://testing-library.com/docs/guiding-principles) page is the source of
the rule the project's AGENTS.md restates:

> "> [The more your tests resemble the way your software is used, the more confidence they can give
> you.](https://twitter.com/kentcdodds/status/977018512689455106)
>
> We try to only expose methods and utilities that encourage you to write tests that closely
> resemble how your web pages are used."

> "1. If it relates to rendering components, then it should deal with DOM nodes rather than
> component instances, and it should not encourage dealing with component instances."

#### The query priority list

The [About Queries — Priority](https://testing-library.com/docs/queries/about/#priority) section is
the operative guidance for "behaviour through accessible roles and text":

> "Based on [the Guiding Principles](/docs/guiding-principles), your test should resemble how users
> interact with your code (component, page, etc.) as much as possible. With this in mind, we
> recommend this order of priority:
>
> 1. **Queries Accessible to Everyone** Queries that reflect the experience of visual/mouse users as
> well as those that use assistive technology.
>    1. `getByRole`: This can be used to query every element that is exposed in the
> [accessibility tree](https://developer.mozilla.org/en-US/docs/Glossary/AOM). With the `name` option
> you can filter the returned elements by their
> [accessible name](https://www.w3.org/TR/accname-1.1/). This should be your top preference for
> just about everything. There's not much you can't get with this (if you can't, it's possible your
> UI is inaccessible). Most often, this will be used with the `name` option like so:
> `getByRole('button', {name: /submit/i})`.
>    2. `getByLabelText`: This method is really good for form fields. …
>    3. `getByPlaceholderText`: [A placeholder is not a substitute for a label](…). …
>    4. `getByText`: Outside of forms, text content is the main way users find elements. …
>    5. `getByDisplayValue`: …
> 2. **Semantic Queries** HTML5 and ARIA compliant selectors. …
> 3. **Test IDs**
>    1. `getByTestId`: The user cannot see (or hear) these, so this is only recommended for cases
> where you can't match by role or text or it doesn't make sense (e.g. the text is dynamic)."

That sentence — "if you can't, it's possible your UI is inaccessible" — is the official link between
this testing level and the project's WCAG 2.2 AA requirement. `getByTestId` sitting at priority 3
is also the official basis for the project's ban on implementation-detail assertions.

The `byrole` query page adds the accessible-name mechanism and one caveat:

> "You can query the returned element(s) by their
> [accessible name or description](https://www.w3.org/TR/accname-1.1/). The accessible name is for
> simple cases equal to e.g. the label of a form element, or the text content of a button, or the
> value of the `aria-label` attribute. … If you only query for a single element with
> `getByText('The name')` it's oftentimes better to use `getByRole(expectedRole, { name: 'The name' })`."

> "`getByRole` is the most preferred query to use as it most closely resembles the user experience,
> however the calculations it must perform to provide this confidence can be expensive (particularly
> with large DOM trees)."

And the official accessibility page makes the connection explicit
([api-accessibility](https://testing-library.com/docs/dom-testing-library/api-accessibility)):

> "One of the guiding principles of the Testing Library APIs is that they should enable you to test
> your app the way your users use it, including through accessibility interfaces like screen
> readers."

#### Setup: RTL needs almost nothing, the matchers need one line

From the [Setup page](https://testing-library.com/docs/react-testing-library/setup):

> "`React Testing Library` does not require any configuration to be used. However, there are some
> things you can do when configuring your testing framework to reduce some boilerplate. In these
> docs we'll demonstrate configuring Jest, but you should be able to do similar things with
> [any testing framework](#using-without-jest) (React Testing Library does not require that you use
> Jest)."

The only Vitest-specific content on that page is about automatic cleanup:

> "If you're using Vitest and want automatic cleanup to work, you can
> [enable globals](https://vitest.dev/config/#globals) through its configuration file:"

> "```ts
> import {defineConfig} from 'vitest/config'
>
> export default defineConfig({
>   test: {
>     globals: true,
>   },
> })
> ```"

> "If you don't want to enable globals, you can import `cleanup` and call it manually in a top-level
> `afterEach` hook"

Note that the RTL Setup page **does not mention `@testing-library/jest-dom` at all**. The
Jest-versus-Vitest import difference lives in the jest-dom repository's own README
([`@testing-library/jest-dom` usage](https://github.com/testing-library/jest-dom#usage)):

> "Import `@testing-library/jest-dom` once (for instance in your tests setup file) and you're good
> to go:"

> "If you are using [vitest][], this module will work as-is, but you will need to use a different
> import in your tests setup file. This file should be added to the [`setupFiles`][vitest setupfiles]
> property in your vitest config:"

> "```javascript
> // In your own vitest-setup.js (or any other name)
> import '@testing-library/jest-dom/vitest'
>
> // In vitest.config.js add (if you haven't already)
> setupFiles: ['./vitest-setup.js']
> ```"

> "Also, depending on your local setup, you may need to update your `tsconfig.json`:"

> "```json
> "types": ["vitest/globals", "@testing-library/jest-dom"]
> ```"

The published `@testing-library/jest-dom@7.0.1` metadata confirms the `/vitest` entrypoint exists
(`"./vitest": {… "default": "./dist/vitest.mjs"}`) and declares `"vitest": ">= 0.32"` as an
**optional** peer dependency.

#### Interaction: `user-event`, not `fireEvent`

From [`user-event` introduction](https://testing-library.com/docs/user-event/intro):

> "`fireEvent` dispatches _DOM events_, whereas `user-event` simulates full _interactions_, which
> may fire multiple events and do additional checks along the way."

> "`user-event` allows you to describe a user interaction instead of a concrete event. It adds
> visibility and interactability checks along the way and manipulates the DOM just like a user
> interaction in the browser would. It factors in that the browser e.g. wouldn't let a user click a
> hidden element or type in a disabled text box."

> "We recommend invoking [`userEvent.setup()`](/docs/user-event/setup) before the component is
> rendered."

The same page states the documented major: "These docs describe `user-event@14`. We recommend
updating your projects to this version". The registry returned `@testing-library/user-event@14.6.7`.

#### Asynchrony: `findBy*` and `waitFor`

From [Async Methods](https://testing-library.com/docs/dom-testing-library/api-async):

> "`findBy` methods are a combination of `getBy` queries and `waitFor`. They accept the waitFor
> options as the last argument (e.g. `await screen.findByText('text', queryOptions, waitForOptions)`)."

> "`findBy` queries work when you expect an element to appear but the change to the DOM might not
> happen immediately."

> "When in need to wait for any period of time you can use `waitFor`, to wait for your expectations
> to pass. Returning _a falsy condition is not sufficient_ to trigger a retry, the callback must
> throw an error in order to retry the condition."

Defaults, verbatim: "The default `interval` is `50ms`. … The default `timeout` is `1000ms`."

This is the mechanism the project's mandatory loading / empty / error / success screen states would
be asserted with: `findByRole` for the loaded state, `queryByRole` returning null for the empty
one, and a role-based assertion for the error message.

#### Versions

| Package | Version retrieved | Source |
|---|---|---|
| `@testing-library/react` | 16.3.3 | `https://registry.npmjs.org/@testing-library/react/latest` |
| `@testing-library/dom` | 10.4.2 | `https://registry.npmjs.org/@testing-library/dom/latest` |
| `@testing-library/jest-dom` | 7.0.1 | `https://registry.npmjs.org/@testing-library/jest-dom/latest` |
| `@testing-library/user-event` | 14.6.7 | `https://registry.npmjs.org/@testing-library/user-event/latest` |
| `jsdom` | 30.1.2 | `https://registry.npmjs.org/jsdom/latest` |
| `happy-dom` | 20.14.5 | `https://registry.npmjs.org/happy-dom/latest` |

Registry-declared floors worth knowing: `@testing-library/jest-dom@7.0.1` declares
`"engines":{"node":">=22", …}` and `jsdom@30.1.2` declares
`"engines":{"node":"^22.22.2 || ^24.15.0 || >=26.0.0"}`.

### 3. MSW: substituting the network at the request level

#### The philosophy is exactly the project's rule

From [MSW — Philosophy](https://mswjs.io/docs/philosophy) and the official README
([mswjs/msw](https://github.com/mswjs/msw)):

> "MSW utilizes a minimal-intrusion framework when it comes to intercepting outgoing network
> traffic. This means having zero changes to your code altogether by using a designated Service
> Worker in the browser, or implementing custom request interception algorithms in Node.js that
> focus on the integrity of your code."

> "- This library intercepts requests on the network level, which means _after_ they have been
> performed and "left" your application. As a result, the entirety of your code runs, giving you
> more confidence when mocking;"

> "- Imagine your application as a box. Every API mocking library out there opens your box and
> removes the part that does the request, placing a blackbox in its stead. Mock Service Worker
> leaves your box intact, 1-1 as it is in production."

> "- Does not stub `fetch`, `axios`, etc. As a result, your tests know _nothing_ about mocking;"

That last line is the official statement behind "the network is substituted at the request level,
not by mocking modules".

#### The two integration points

Node, for the test runner
([Node.js integration](https://mswjs.io/guides/integrations/node)):

> "```js
> // src/mocks/node.js
> import { setupServer } from 'msw/node'
> import { handlers } from './handlers'
>
> export const server = setupServer(...handlers)
> ```"

> "`server.listen()` is a _synchronous_ API so you don't have to await it."

> "**There are three key steps to integrating MSW with any test runner:**
> 1. Enable mocking _before_ all tests run (`server.listen()`);
> 1. Reset any handlers _between_ tests (`server.resetHandlers()`);
> 1. Restore native request-issuing modules _after_ all tests run (`server.close()`)."

Browser, for the running app
([Browser integration](https://mswjs.io/guides/integrations/browser)):

> "In the browser, MSW works by registering a Service Worker responsible for request interception on
> the network level."

> "If your application registers a Service Worker it must **host and serve it**. The library CLI
> provides you with the `init` command to quickly copy the `./mockServiceWorker.js` worker script
> into your application's public directory."

> "```sh
> npx msw init <PUBLIC_DIR> --save
> ```"

> "Make sure to await the `worker.start()` Promise! Service Worker registration is asynchronous and
> failing to await it may result in a race condition between the worker registration and the initial
> requests your application makes."

#### Handlers are shared; only the entrypoint differs

The [Quick start](https://mswjs.io/docs/quick-start) states the sharing model:

> "One of the core benefits of MSW is the ability to reuse the same mocks (e.g. `handlers.ts`) across
> different tools and environments. On their own, request handlers don't do anything. They have to be
> provided to the `setupServer` or `setupWorker` functions to configure API mocking in a Node.js or a
> browser process, respectively."

> "This integration has nothing specific to Vitest. You can reuse it to apply MSW to any Node.js
> process."

> "MSW applies on the environment level (Node.js or browser). Integrating it with different tools
> will come down to finding the appropriate environment entrypoint provided to you by that tool and
> following the same setup as you did above."

And the file-structure recommendation
([Structuring handlers](https://mswjs.io/guides/best-practices/structuring-handlers)):

> "We recommend utilizing a single `handlers.js` module to describe the successful states (happy
> paths) of your network."

> "Instead, consider splitting them on the file system level, grouping them by domain, and composing
> the list of handlers later."

#### The Node floor is 22 — and this is a hard requirement

From the [MSW FAQ](https://mswjs.io/docs/faq):

> "This error means that the version of Node.js you're using doesn't support the global Fetch API.
>
> Resolve this by upgrading to Node.js version 22 or higher. MSW does not support Node.js versions
> below version 22."

The 2.x → 3.x migration guide
([Migrations](https://mswjs.io/docs/migrations/2.x-to-3.x)):

> "**This release sets the minimal supported Node.js version to 22.0.0.**
>
> **Node.js versions prior to Node.js 22 are no longer supported.**"

The published metadata for the current release agrees: `msw@3.0.2` carries
`"engines":{"node":">=22.12.0"}` and `"type":"module"` — **MSW 3 is ESM-only**, which the
[v3.0.0 release notes](https://github.com/mswjs/msw/releases/tag/v3.0.0) state as a breaking change:
"**MSW is now ESM-only** (#2765)."

#### Next.js App Router and Server Components — the honest state

This is the weakest part of the evidence, and it should be reported as such.

- **The Next.js documentation does not mention MSW at all.** The App Router testing guides
  ([testing](https://nextjs.org/docs/app/guides/testing),
  [vitest](https://nextjs.org/docs/app/guides/testing/vitest),
  [jest](https://nextjs.org/docs/app/guides/testing/jest),
  [playwright](https://nextjs.org/docs/app/guides/testing/playwright)) install no MSW package and
  contain no MSW section.
- **The MSW documentation publishes no Next.js integration guide.** The official integration pages
  are `browser`, `cloudflare`, `node`, `react-native`, `storybook`, `vite`, `vitest` — no Next.js and
  no Playwright page.
- **The official MSW examples repository has no Next.js example.**
  [`mswjs/examples`](https://github.com/mswjs/examples) contains `with-angular`, `with-jest-jsdom`,
  `with-jest`, `with-karma`, `with-playwright`, `with-remix`, `with-svelte`, `with-vitest-cjs`,
  `with-vitest`, `with-vue`. A Next.js App Router example is
  [an open, unmerged draft PR (#101)](https://github.com/mswjs/examples/pull/101), whose own body
  reads:

  > "Adds a Next.js 14 (App directory ) + MSW usage example.
  >
  > ## Todos
  >
  > - [x] https://github.com/vercel/next.js/pull/68193
  > - [ ] https://github.com/vercel/next.js/issues/69098"

  That draft starts the Node server from a layout, not from an instrumentation hook — its
  `app/layout.tsx` reads:

  > "```tsx
  > if (process.env.NEXT_RUNTIME === 'nodejs') {
  >   const { server } = require('@/mocks/node')
  >   server.listen()
  > }
  > ```"

  and it is pinned to `msw@2.4.2` and a Next.js 15 canary — i.e. it predates MSW 3 and its renamed
  option.
- **The documented history of the limitation** is issue
  [mswjs/msw#1644 "Support Next.js 13 (App Router)"](https://github.com/mswjs/msw/issues/1644),
  opened by the maintainer:

  > "As of 1.2.2, MSW _does not support_ the newest addition to Next.js being the App directory in
  > Next.js 13."

  > "In Node.js, MSW works by patching global Node modules, like `http` and `https`. Those patches
  > _must persist in the process_ in order for the mocking to work. Since Next uses this fluctuating
  > process structure, it introduces two main issues: …"

  > "MSW is blocked by the way Next.js is implemented internally."

  The issue is now **closed as completed**, and the closing comment relocates the remaining blocker
  to Vercel's side:

  > "I'm going to close this issue. No action is needed on MSW side, never has been. Every step in
  > making this support happen required changes or bug fixes in Next.js.
  >
  > Currently, https://github.com/vercel/next.js/issues/69098 is the remaining blocker to have a
  > proper HMR in the browser. The server-side integration, including HMR, works great. …
  >
  > As always, here's your integration reference: https://github.com/mswjs/examples/pull/101. Once
  > we have a proper HMR in place, I will merge that example."

- **The maintainer explicitly warns against the `instrumentation.ts` approach**, in the official
  repository discussion
  [mswjs/msw#2531](https://github.com/mswjs/msw/discussions/2531#discussioncomment-13444761):

  > "Note that you **must not use the instrumentation hook**. It doesn't work for the purposes
  > needed by MSW. It also only runs _once_, which kills HMR."

- **There *is* one first-party, Vercel-owned, MSW-shaped integration — and it is experimental.**
  `next/experimental/testmode` ships a Playwright fixture (`next/experimental/testmode/playwright`)
  with an MSW variant (`.../playwright/msw`), documented only in the Next.js source tree
  ([README](https://github.com/vercel/next.js/blob/canary/packages/next/src/experimental/testmode/playwright/README.md)):

  > "### Optionally install MSW in your project
  >
  > [MSW](https://mswjs.io/) can be helpful for fetch mocking."

  > "### Or use the `next/experimental/testmode/playwright/msw`"

  Its scope is narrower than MSW's, and the README says so:

  > "// NOTE: `next.onFetch` only intercepts external `fetch` requests (for both client and server).
  > For example, if you `fetch` a relative URL (e.g. `/api/hello`) from the client that's handled by
  > a Next.js route handler (e.g. `app/api/hello/route.ts`), it won't be intercepted."

#### MSW 3 renamed the unhandled-request option

Anyone writing MSW 3 config from a 2024-era example will get this wrong
([Migrations](https://mswjs.io/docs/migrations/2.x-to-3.x)):

> "### `onUnhandledRequest`
>
> The `onUnhandledRequest` option of `worker.start()` and `server.listen()` has been renamed to
> `onUnhandledFrame`. It now applies to any unhandled _network frame_, which includes both requests
> and WebSocket connections."

The current API documents the strategies as `"warn"` (default), `"error"` and `"bypass"`.

### 4. Playwright: configuration and CI

#### Where the config lives, per the docs

The official structure the scaffolder produces
([Installation](https://playwright.dev/docs/intro)):

> "```bash
> playwright.config.ts         # Test configuration
> package.json
> package-lock.json            # Or yarn.lock / pnpm-lock.yaml
> tests/
>   example.spec.ts            # Minimal example test
> ```"

> "The playwright.config centralizes configuration: target browsers, timeouts, retries, projects,
> reporters and more. In existing projects dependencies are added to your current `package.json`."

The [Test configuration](https://playwright.dev/docs/test-configuration) page states the base
config, including the location rule for `testDir`:

> "```ts
> export default defineConfig({
>   // Look for test files in the "tests" directory, relative to this configuration file.
>   testDir: 'tests',
>   …
>   use: {
>     // Base URL to use in actions like `await page.goto('/')`.
>     baseURL: 'http://localhost:3000',
>     // Collect trace when retrying the failed test.
>     trace: 'on-first-retry',
>   },
>   projects: [ { name: 'chromium', use: { ...devices['Desktop Chrome'] } } ],
>   // Run your local dev server before starting the tests.
>   webServer: {
>     command: 'npm run start',
>     url: 'http://localhost:3000',
>     reuseExistingServer: !process.env.CI,
>   },
> })
> ```"

> "Note that test runner options are **top-level**, do not put them into the `use` section."

**There is no official Playwright documentation about monorepos, and none about pnpm workspaces
or `pnpm --filter`.** The only location primitives the docs give are: `testDir` is "relative to
this configuration file", the CLI's `-c/--config` "Defaults to `playwright.config.ts` or
`playwright.config.js` in the current directory"
([Command line](https://playwright.dev/docs/test-cli)), and `webServer.cwd` "defaults to the
directory of the configuration file" ([webServer](https://playwright.dev/docs/test-webserver)).
pnpm appears only as a package-manager tab (`pnpm exec playwright test`).

#### CI

[Continuous Integration](https://playwright.dev/docs/ci) gives the three steps:

> "3 steps to get your tests running on CI:
> 1. **Ensure CI agent can run browsers**: Use our Docker image in Linux agents or install your
> dependencies using the CLI.
> 2. **Install Playwright**:
> ```
> # Install NPM packages
> npm ci
>
> # Install Playwright browsers and dependencies
> npx playwright install --with-deps
> ```
> 3. **Run your tests**:
> ```
> npx playwright test
> ```"

> "We recommend setting workers to "1" in CI environments to prioritize stability and reproducibility.
> Running tests sequentially ensures each test gets the full system resources, avoiding potential
> conflicts. However, if you have a powerful self-hosted CI system, you may enable parallel tests.
> For wider parallelization, consider sharding - distributing tests across multiple CI jobs."

The recommended GitHub Actions shape (published page), including the artifacts upload:

> "```yml
> - uses: actions/checkout@v6
> - uses: actions/setup-node@v6
>   with:
>     node-version: lts/*
> - name: Install dependencies
>   run: npm ci
> - name: Install Playwright Browsers
>   run: npx playwright install --with-deps
> - name: Run Playwright tests
>   run: npx playwright test
> - uses: actions/upload-artifact@v5
>   if: ${{ !cancelled() }}
>   with:
>     name: playwright-report
>     path: playwright-report/
>     retention-days: 30
> ```"

Sharding, from [Sharding](https://playwright.dev/docs/test-sharding):

> "To shard the test suite, pass `--shard=x/y` to the command line. For example, to split the suite
> into four shards, each running one fourth of the tests:"

> "```bash
> npx playwright test --shard=1/4
> ```"

> "Start with adding `blob` reporter to the config when running on CI:"

> "```ts
> export default defineConfig({
>   testDir: './tests',
>   reporter: process.env.CI ? 'blob' : 'html',
> });
> ```"

The blob reports are then merged:

> "```bash
> npx playwright merge-reports --reporter html ./all-blob-reports
> ```"

Retries appear in the config sample as `retries: process.env.CI ? 2 : 0`, but the CI pages
themselves contain no retries recommendation.

Docker, from [Docker](https://playwright.dev/docs/docker), which is the CI-container path:

> "`docker pull mcr.microsoft.com/playwright:v1.63.0-noble`"

> "It is recommended to always pin your Docker image to a specific version if possible. If the
> Playwright version in your Docker image does not match the version in your project/tests,
> Playwright will be unable to locate browser executables."

#### Component testing is not the same product any more

[Playwright component testing](https://playwright.dev/docs/test-components) has changed shape — worth
knowing because a 2024-era tutorial would describe the old packages:

> "The experimental `@playwright/experimental-ct-react`, `-ct-react17` and `-ct-vue` packages have
> been removed and are no longer published."

> "A component test is a regular Playwright end-to-end test that runs against a small **story
> gallery** page served by your own dev server. There is no dedicated component-testing runtime, no
> bundler integration and no extra npm packages — the built-in fixtures.mount() fixture of
> `@playwright/test` drives it all."

This is irrelevant to this project as long as component testing stays with React Testing Library,
as the rules require — but it removes the argument that Playwright duplicates the component level.

#### Versions

`@playwright/test` — retrieved from `https://registry.npmjs.org/@playwright/test/latest`:
`"_id":"@playwright/test@1.63.0"`, `"engines":{"node":">=20"}`, `"license":"Apache-2.0"`,
`"dependencies":{"playwright":"1.63.0"}`, corroborated by
[the release notes](https://playwright.dev/docs/release-notes) ("## Version 1.63") and by the
Docker page's tag list.

### 5. ESLint flat config, typescript-eslint, and enforcing the FSD layer direction

#### `.eslintrc` is gone in ESLint 10

The current configuration page
([Configuration Files](https://eslint.org/docs/latest/use/configure/configuration-files)) lists the
accepted file names and the format:

> "The ESLint configuration file may be named any of the following:
> - `eslint.config.js`
> - `eslint.config.mjs`
> - `eslint.config.cjs`
> - `eslint.config.ts` (requires additional setup)
> - `eslint.config.mts` (requires additional setup)
> - `eslint.config.cts` (requires additional setup)
>
> It should be placed in the root directory of your project and export an array of configuration
> objects."

The legacy system's removal is stated in the v10 migration guide
([Migrate to v10.x](https://eslint.org/docs/latest/use/migrate-to-10.0.0)):

> "ESLint v9 introduced a [new default configuration format](./configure/configuration-files) based
> on the `eslint.config.js` file. The old format, which used `.eslintrc` or `.eslintrc.json`, could
> still be enabled in v9 by setting the `ESLINT_USE_FLAT_CONFIG` environment variable to `false`.
>
> Starting with ESLint v10.0.0, the old configuration format is no longer supported."

Two further v10 changes that touch a monorepo, same page:

> "- Be aware that the deprecated APIs `FlatESLint` and `LegacyESLint` have been removed. Always use
> `ESLint` instead."

> "In ESLint v9, the alternate config lookup behavior could be enabled with the
> `v10_config_lookup_from_file` feature flag. This behavior made ESLint locate `eslint.config.*` by
> starting from the directory of each linted file and searching up towards the filesystem root. In
> ESLint v10.0.0, this behavior is now the default and the `v10_config_lookup_from_file` flag has
> been removed."

The second one matters: per-package `eslint.config.mjs` files are now the native way to configure a
workspace, and a package's own config governs its files.

`eslint` — retrieved from `https://registry.npmjs.org/eslint/latest`: `"_id":"eslint@10.12.0"`,
`"engines":{"node":"^20.19.0 || ^22.13.0 || >=24"}`.

#### typescript-eslint: one package, and typed linting is on

The official quickstart ([Getting Started](https://typescript-eslint.io/getting-started)) installs a
single package:

> "```
> npm install --save-dev eslint @eslint/js typescript typescript-eslint
> ```"

> "Next, create an `eslint.config.mjs` config file in the root of your project, and populate it with
> the following:"

> "```js
> // @ts-check
> import js from '@eslint/js';
> import { defineConfig } from 'eslint/config';
> import tseslint from 'typescript-eslint';
>
> export default defineConfig({
>   files: ['**/*.{js,ts}'],
>   extends: [js.configs.recommended, tseslint.configs.recommended],
> });
> ```"

> "`defineConfig(...)` is an optional helper function built in to current versions of ESLint."

The package's own page confirms the role
([typescript-eslint package](https://typescript-eslint.io/packages/typescript-eslint/)):

> "This package is the main entrypoint that you can use to consume our tooling with ESLint."

The old two-package split is now internal — the page documents `parser` as "A re-export of
`@typescript-eslint/parser`" and `plugin` as "A re-export of `@typescript-eslint/eslint-plugin`".
And a related deprecation:

> "danger — The `config(...)` utility function was deprecated in favor of ESLint core's
> `defineConfig(...)`."

[Typed linting](https://typescript-eslint.io/getting-started/typed-linting) is what turns the
project's "no errors and no warnings, no `any`, no suppressions" requirement from a style rule into
a checked one:

> "Some typescript-eslint rules utilize TypeScript's type checking APIs to provide much deeper
> insights into your code. This requires TypeScript to analyze your entire project instead of just
> the file being linted. As a result, these rules are slower than traditional lint rules but are much
> more powerful."

> "To enable typed linting, there are two small changes you need to make to your config file:
> 1. Add `TypeChecked` to the name of any preset configs you're using, namely `recommended`,
> `strict`, and `stylistic`.
> 2. Add `languageOptions.parserOptions` to tell our parser how to find the TSConfig for each source
> file."

> "```js
> export default defineConfig({
>   files: ['**/*.{js,ts}'],
>   extends: [
>     js.configs.recommended,
>     tseslint.configs.recommended,
>     tseslint.configs.recommendedTypeChecked,
>   ],
>   languageOptions: { parserOptions: { projectService: true } },
> });
> ```"

> "`parserOptions.projectService: true` indicates to ask TypeScript's type checking service for each
> source file's type information"

> "**We strongly recommend you do use type-aware linting**, but the above information is included so
> that you can make your own, informed decision."

`typescript-eslint` — retrieved from `https://registry.npmjs.org/typescript-eslint/latest`:
`"_id":"typescript-eslint@8.71.1"`, with
`"peerDependencies":{"eslint":"^8.57.0 || ^9.0.0 || ^10.0.0","typescript":">=4.8.4 <6.1.0"}` — i.e.
it already declares ESLint 10 support.

#### Next.js's own ESLint story changed in 16

[Config: ESLint](https://nextjs.org/docs/app/api-reference/config/eslint) documents flat config as
the setup, not `.eslintrc`:

> "1. Install ESLint and the Next.js config:
> ```bash package="pnpm"
> pnpm add -D eslint eslint-config-next
> ```"

> "2. Create `eslint.config.mjs` with the Next.js config:"

> "```js filename="eslint.config.mjs"
> import { defineConfig, globalIgnores } from 'eslint/config'
> import nextVitals from 'eslint-config-next/core-web-vitals'
>
> const eslintConfig = defineConfig([
>   ...nextVitals,
>   globalIgnores([
>     '.next/**',
>     'out/**',
>     'build/**',
>     'next-env.d.ts',
>   ]),
> ])
>
> export default eslintConfig
> ```"

> "3. Run ESLint:
> ```bash package="pnpm"
> pnpm exec eslint .
> ```"

And the removal:

> "`next lint` removal
>
> Starting with Next.js 16, `next lint` is removed.
>
> As part of the removal, the `eslint` option in your Next config file is no longer needed and can be
> safely removed."

> "| `v16.0.0` | `next lint` and the `eslint` next.config.js option were removed in favor of the ESLint
> CLI. A [codemod](/docs/app/guides/upgrading/codemods#migrate-from-next-lint-to-eslint-cli) is
> available to help you migrate. |"

The config variants are `eslint-config-next`, `eslint-config-next/core-web-vitals`,
`eslint-config-next/typescript`, and the page records an immediate peer-dependency reality:

> "**Good to know**: `eslint-config-next` supports ESLint 9 and ESLint 10. ESLint 10 requires Node.js
> `^20.19.0`, `^22.13.0`, or `>=24`. Some of the plugins included in `eslint-config-next` don't list
> ESLint 10 in their peer dependencies yet, so your package manager may show peer dependency warnings,
> or fail when strict peer dependencies are enabled."

The page also documents the monorepo case explicitly — directly relevant to `apps/web`:

> "If you're using `@next/eslint-plugin-next` in a project where Next.js isn't installed in your root
> directory (such as a monorepo), you can tell `@next/eslint-plugin-next` where to find your Next.js
> application using the `settings` property in your `eslint.config.mjs`:"

> "```js filename="eslint.config.mjs"
> const eslintConfig = defineConfig([
>   {
>     files: ['**/*.{js,jsx,ts,tsx}'],
>     plugins: { '@next/next': eslintNextPlugin },
>     settings: { next: { rootDir: 'packages/my-app/' } },
>   },
> ])
> ```"

> "`rootDir` can be a path (relative or absolute), a glob (i.e. `"packages/*/"`), or an array of paths
> and/or globs."

And Prettier integration is documented inline:

> "ESLint also contains code formatting rules, which can conflict with your existing
> [Prettier](https://prettier.io/) setup. We recommend including
> [eslint-config-prettier](https://github.com/prettier/eslint-config-prettier) in your ESLint config
> to make ESLint and Prettier work together."

> "```js
> import prettier from 'eslint-config-prettier/flat'
>
> const eslintConfig = defineConfig([
>   ...nextVitals,
>   prettier,
>   globalIgnores([…]),
> ])
> ```"

`eslint-config-next` — retrieved from `https://registry.npmjs.org/eslint-config-next/latest`:
`"_id":"eslint-config-next@16.4.0"`, `"peerDependencies":{"eslint":">=9.0.0","typescript":">=3.3.1"}`,
and among its dependencies `"typescript-eslint":"^8.56.0"`, `"eslint-plugin-import":"^2.32.0"`,
`"eslint-plugin-react-hooks":"^7.1.0"`.

#### Enforcing the FSD layer direction

The rule itself is stated by Feature-Sliced Design's own documentation
([Layers](https://feature-sliced.design/docs/reference/layers)):

> "Layers are made up of _slices_ — highly cohesive groups of modules. Dependencies between slices
> are regulated by **the import rule on layers**:
>
> > _A module (file) in a slice can only import other slices when they are located on layers strictly
> below._"

> "Layers App and Shared are **exceptions** to this rule — they are both a layer and a slice at the
> same time."

**FSD's documentation does not name a tool that enforces it, and the linter page is gone.** The URL
`https://feature-sliced.design/docs/guides/tech/with-eslint` returns HTTP 404 today; the current docs
sidebar exposes only Guides and Reference sections with no ESLint or linter page. Everything FSD
says about linting sits on its blog, which is not a primary source for this note.

The candidates, and what each one's own documentation says:

**`eslint-plugin-boundaries`** — its README
([javierbrea/eslint-plugin-boundaries](https://github.com/javierbrea/eslint-plugin-boundaries)):

> "**ESLint Plugin Boundaries** is an ESLint plugin that helps you maintain clean architecture by
> enforcing boundaries between different parts of your codebase. Define your architectural layers,
> specify how they can interact, and get instant feedback when boundaries are violated."

> "- **Flexible Configuration**: Adapt the plugin to any project structure. It works with monorepos,
> modular architectures, layered patterns, and any custom structure you can imagine"

Its README documents flat config directly, with a policy model that names both sides of a dependency
— which is what a layer rule needs:

> "```javascript
> import boundaries from "eslint-plugin-boundaries";
>
> export default [
>   {
>     plugins: { boundaries },
>     settings: {
>       "boundaries/elements": [
>         { type: "controller", pattern: "controllers/*" },
>         { type: "model", pattern: "models/*" },
>         { type: "view", pattern: "views/*" }
>       ],
>       "boundaries/files": [
>         { category: "test", pattern: "**/*.test.js" }
>       ]
>     }
>   }
> ];
> ```"

> "```javascript
> {
>   rules: {
>     "boundaries/dependencies": [2, {
>       default: "disallow",
>       policies: [
>         { from: { element: { type: "controller" } },
>           allow: { to: { element: { types: { anyOf: ["model", "view"] } } } } },
>         { from: { element: { type: "view" } },
>           allow: { to: { element: { type: "model" } } } },
>         { disallow: { to: { file: { categories: "test" } } } },
>         { from: { element: { type: "!controller" } },
>           disallow: { to: { module: { origin: "external", source: "axios" } } } }
>       ]
>     }]
>   }
> }
> ```"

Its own docs site ([JS Boundaries](https://www.jsboundaries.dev/docs/overview/)) states the scope
and the recommended pairing:

> "This plugin focuses on enforcing architectural boundaries by analyzing the relationships between
> abstract elements. It does not inspect import syntax or enforce coding standards unrelated to module
> dependencies.
>
> note — This plugin is not a replacement for eslint-plugin-import. In fact, using both together is
> recommended."

And the ESLint-version compatibility statement, which matters because this project is on ESLint 10
([Installation](https://www.jsboundaries.dev/docs/installation/)):

> "ESLint Version Compatibility — Starting from version `5.0.0`, this plugin is compatible with ESLint
> **v9 and above**. While it may still work with earlier versions of ESLint, refer to the documentation
> for version `4.2.2` if you need to configure it using the legacy format."

Registry and release data for `eslint-plugin-boundaries`: `"version":"7.2.0"` from
`https://registry.npmjs.org/eslint-plugin-boundaries/latest`; latest GitHub release `v7.2.0`,
published `2026-08-09T18:42:19Z`, from
`https://api.github.com/repos/javierbrea/eslint-plugin-boundaries/releases/latest`.

**`eslint-plugin-import` with `no-restricted-paths`** — its README documents flat config, and the
rule resolves paths rather than matching strings
([no-restricted-paths](https://github.com/import-js/eslint-plugin-import/blob/main/docs/rules/no-restricted-paths.md)):

> "Some projects contain files which are not always meant to be executed in the same environment. …
> In order to prevent such scenarios this rule allows you to define restricted zones where you can
> forbid files from being imported if they match a specific path."

> "Each zone consists of a `target`, a `from`, and optional `except` and `message` attributes.
>
> - `target` - Identifies which files are part of the zone. It can be expressed as:
>   - A simple directory path, matching all files contained recursively within it
>   - A glob pattern
>   - An array of any of the two types above
> - `from` - Identifies folders from which the zone is not allowed to import. …"

> "*Note: The `from` attribute is NOT matched literally against the import path string as it appears
> in the code. Instead, it's matched against the path to the imported file after it's been resolved
> against `basePath`.*"

That resolution is the property core `no-restricted-imports` lacks. Its maintenance data is a caveat
rather than a red flag: `https://registry.npmjs.org/eslint-plugin-import/latest` returns
`"_id":"eslint-plugin-import@2.32.0"` with
`"peerDependencies":{"eslint":"^2 || ^3 || ^4 || ^5 || ^6 || ^7.2.0 || ^8 || ^9"}` — **ESLint 10 is
not listed** — and the latest GitHub release is `v2.32.0`, published `2025-06-20`. It arrives
transitively via `eslint-config-next@16.4.0` regardless.

**Core `no-restricted-imports`** — the rule exists and has a `patterns`/`group` mechanism
([no-restricted-imports](https://eslint.org/docs/latest/rules/no-restricted-imports)):

> "This rule allows you to specify imports that you don't want to use in your application.
>
> It applies to static imports only, not dynamic ones."

> "#### group — The `patterns` array can also include objects. The `group` property is used to specify
> the `gitignore`-style patterns for restricting modules and the `message` property is used to specify
> a custom message."

What the rule's own documentation does **not** claim is any ability to resolve or classify a layer.
It matches module specifier strings, so it can block an alias like `@/pages/*` from a scoped
`files: ['src/features/**']` block, but it cannot reason about `../../pages/...`.

**`@feature-sliced/eslint-config`** — the FSD project's own package, and the one candidate whose
official material rules it out
([feature-sliced/eslint-config](https://github.com/feature-sliced/eslint-config)):

> "> `WIP:` At the moment at beta-testing - [use carefully](https://github.com/feature-sliced/eslint-config/discussions/75)"

> "Linting of [FeatureSliced](https://github.com/feature-sliced/documentation) concepts *by existing
> eslint-plugins*"

Its documented configuration is the legacy format only:

> "3. Add config to the `extends` section of your `.eslintrc` configuration file (for **recommended**
> rules). You can omit the `eslint-config` postfix:"

> "```json
> {
>     "extends": ["@feature-sliced"]
> }
> ```"

**No flat-config example appears anywhere in its README.** The registry returns
`"_id":"@feature-sliced/eslint-config@0.1.1"` with
`"peerDependencies":{"eslint-plugin-import":">=2","eslint-plugin-boundaries":">=2"}`, and the latest
GitHub release is `v0.1.0-beta.6`, published `2022-02-12`. Under ESLint 10, `.eslintrc` is removed, so
an `.eslintrc`-only package cannot be used as documented.

### 6. Prettier and git hooks

#### Prettier

The supported config file names
([Configuration File](https://prettier.io/docs/configuration)):

> "You can configure Prettier via (in order of precedence):"
> - "A `"prettier"` key in your `package.json`, or [`package.yaml`](https://github.com/pnpm/pnpm/pull/1799) file."
> - "A `.prettierrc` file written in JSON or YAML."
> - "A `.prettierrc.json`, `.prettierrc.yml`, `.prettierrc.yaml`, or `.prettierrc.json5` file."
> - "A `.prettierrc.js`, `prettier.config.js`, `.prettierrc.ts`, or `prettier.config.ts` file …"
> - "A `.prettierrc.mjs`, `prettier.config.mjs`, `.prettierrc.mts`, or `prettier.config.mts` file …"
> - "A `.prettierrc.cjs`, `prettier.config.cjs`, `.prettierrc.cts`, or `prettier.config.cts` file …"
> - "A `.prettierrc.toml` file."

> "The configuration file will be resolved starting from the location of the file being formatted,
> and searching up the file tree until a config file is (or isn't) found."

No format is documented as deprecated. The only caveat is TypeScript config files: "TypeScript
support requires Node.js>=22.6.0".

On the split of duties
([Integrating with Linters](https://prettier.io/docs/integrating-with-linters)):

> "Linters usually contain not only code quality rules, but also stylistic rules. Most stylistic rules
> are unnecessary when using Prettier, but worse – they might conflict with Prettier! Use Prettier for
> code formatting concerns, and linters for code-quality concerns"

> "Luckily it's easy to turn off rules that conflict or are unnecessary with Prettier, by using these
> pre-made configs: … [eslint-config-prettier](https://github.com/prettier/eslint-config-prettier)"

And on not running Prettier as a lint rule:

> "The downsides of those plugins are:"
> - "You end up with a lot of red squiggly lines in your editor, which gets annoying. Prettier is supposed to make you forget about formatting – and not be in your face about it!"
> - "They are slower than running Prettier directly."
> - "They're yet one layer of indirection where things may break."

Prettier's own docs are also the source of the eslint-plus-public-platform concern — the page documents
`eslint-config-prettier/flat` in the flat-config form, which is the variant Next.js's ESLint page
imports.

Version: `https://registry.npmjs.org/prettier/latest` returns `"version":"3.9.9"`, matching the
pinned install command on Prettier's own Install page (`npm install --save-dev --save-exact
prettier@3.9.9`).

#### The hook tools, per their own documentation

**There is no official ranking of these tools.** Prettier's
[Pre-commit Hook](https://prettier.io/docs/precommit) page simply lists six options, of which the
first is lint-staged and the fifth is Lefthook:

> "You can use Prettier with a pre-commit tool. This can re-format your files that are marked as
> "staged" via `git add` before you commit."

> "**Use Case:** Useful for when you want to use other code quality tools along with Prettier (e.g.
> ESLint, Stylelint, etc.) or if you need support for partially staged files (`git add --patch`)."

The lint-staged recipe is on Prettier's Install page:

> "1. Install [husky](https://github.com/typicode/husky) and
> [lint-staged](https://github.com/okonet/lint-staged):
> ```
> npm install --save-dev husky lint-staged
> npx husky init
> node --eval "fs.writeFileSync('.husky/pre-commit','npx lint-staged\n')"
> ```"

> "2. Add the following to your `package.json`:
> ```
> {  "lint-staged": {    "**/*": "prettier --write --ignore-unknown"  }}
> ```"

> "note — If you use ESLint, make sure lint-staged runs it before Prettier, not after."

Lefthook's recipe on the same page, with the `stage_fixed` detail that matters when the hook rewrites
files:

> "Add a Prettier job to a `lefthook.yml` file at the repo root:"

> "```
> pre-commit:
>   jobs:
>     - name: prettier
>       run: npx prettier --ignore-unknown --write '{staged_files}'
>       stage_fixed: true
> ```"

> "`stage_fixed: true` re-stages files that Prettier modified so the formatted version is what gets
> committed."

> "Then install Lefthook and run `lefthook install` so Lefthook wires up the configured jobs into
> `.git/hooks`:"

**lint-staged** — its README ([lint-staged](https://github.com/lint-staged/lint-staged)) states both
the purpose and the monorepo model:

> "Run tasks like formatters and linters against staged git files and don't let 💩 slip into your code
> base!"

> "Code quality tasks like formatters and linters make more sense when running before committing your
> code. … But running a task on a whole project can be slow, and opinionated tasks such as linting can
> sometimes produce irrelevant results. Ultimately you only want to check files that will be
> committed."

> "### How to use `lint-staged` in a multi-package monorepo?
>
> Install _lint-staged_ on the monorepo root level and add separate configuration files in each
> package. _Lint-staged_ will find each config file and match staged files to the closest config. The
> directory of each config file will be used as the working directory for those tasks, unless `--cwd`
> option is used."

> "**Note**: _lint-staged_ does not merge config files, so if the closest config file to a staged file
> doesn't match it, the file will be ignored."

> "**Note**: If you want to run _lint-staged_ in only one package inside a monorepo, you can simply use
> the `--cwd` option (for example `lint-staged --cwd packages/frontend`)."

The README also documents the TypeScript trap that bears directly on "run types on changed files":

> "1. `lint-staged` automatically passes matched staged files as arguments to commands."
> "2. Certain input files can cause TypeScript to ignore `tsconfig.json`."

> "After: `module.exports = { '*.{ts,tsx}': [() => 'tsc --noEmit', 'prettier --write'], }`"

Version: `https://registry.npmjs.org/lint-staged/latest` returns `"version":"17.6.0"`,
`"engines":{"node":">=22.22.1"}`.

**Husky** — [Get Started](https://typicode.github.io/husky/get-started.html):

> "```
> npm install --save-dev husky
> ```
> (pnpm tab: `pnpm add --save-dev husky`)"

> "The `init` command simplifies setting up husky in a project. It creates a `pre-commit` script in
> `.husky/` and updates the `prepare` script in `package.json`."

> "```
> npx husky init
> ```"

> "Run the `husky` command once in your repo. Ideally, include it in the `prepare` script in
> `package.json` for automatic execution after each install (recommended)."

> "```json
> { "scripts": { "prepare": "husky" } }
> ```"

Husky's introduction describes its mechanism and lists monorepos as supported:

> "Uses new Git feature (`core.hooksPath`)"

> "Supports: macOS, Linux, Windows / Git GUIs, Node version managers, custom hooks directory, nested
> projects, monorepos / All 13 client-side Git hooks"

The official v9 deprecation notes concern the shell shim, and the official docs no longer document
`husky install` or `husky add` at all. The v9.1.2 release notes warn:

> "Show a message instead of automatically removing deprecated code. … **Hooks with these lines will
> fail in `v10.0.0`**"

Version: `https://registry.npmjs.org/husky/latest` returns `"version":"9.1.7"`,
`"engines":{"node":">=18"}`; the latest GitHub release is also `v9.1.7`, published `2024-11-18`.

**Lefthook** — [lefthook.dev](https://lefthook.dev/):

> "You — Create [`lefthook.yml`](./configuration/) configuration file — Run `lefthook install`"

> "Lefthook installs the configured hooks into `.git/hooks/`."

> "```
> pre-commit:
>   parallel: true
>   jobs:
>     - run: yarn run stylelint --fix '{staged_files}'
>       glob: "*.css"
>       stage_fixed: true
>
>     - run: yarn run eslint --fix '{staged_files}'
>       glob: "*.ts"
>       stage_fixed: true
> ```"

From [run](https://lefthook.dev/configuration/run/):

> "You can use files templates that will be substituted with the appropriate files on execution:"
> - "`{files}` - custom `files` command result."
> - "`{staged_files}` - staged files which you try to commit."
> - "`{push_files}` - files that are committed but not pushed."
> - "`{all_files}` - all files tracked by git."

> "Command line length has a limit on every system. If your list of files is quite long, lefthook
> splits your files list to fit in the limit and runs few commands sequentially."

From [parallel](https://lefthook.dev/configuration/parallel/): "**Default: `false`** … Lefthook runs
commands and scripts **sequentially** by default". From
[stage_fixed](https://lefthook.dev/configuration/stage_fixed/): "**Default: `false`** … Works **only**
for the `pre-commit` hook. When set to `true` lefthook will automatically call `git add` on files after
running the command".

There is **no dedicated monorepo page** in Lefthook's docs. The documented mechanism for a
multi-package layout is the [`root` option](https://lefthook.dev/configuration/root/):

> "You can change the CWD for the command you execute using `root` option."
>
> "This is useful when you execute some `npm` or `yarn` command but the `package.json` is in another
> directory."
>
> "For `pre-push` and `pre-commit` hooks and for the custom `files` command `root` option is used to
> filter file paths. If all files are filtered the command will be skipped."

Lefthook's install page carries a pnpm-specific requirement:

> "Note — If you use `pnpm` package manager make sure to update `pnpm-workspace.yaml`s
> `onlyBuiltDependencies` with `lefthook` and add `lefthook` to `pnpm.onlyBuiltDependencies` in your
> root `package.json`, otherwise the `postinstall` script of the `lefthook` package won't be executed
> and hooks won't be installed."

Version: `https://registry.npmjs.org/lefthook/latest` returns `"version":"2.1.17"`.

**`simple-git-hooks`** — its own README
([toplenboren/simple-git-hooks](https://github.com/toplenboren/simple-git-hooks)) is candid about the
scope:

> "`simple-git-hooks` works well for small-sized projects when you need quickly set up hooks and forget
> about it."
>
> "However, this package requires you to manually apply the changes to git hooks. If you update them
> often, this is probably not the best choice."
>
> "Also, this package allows you to set only one command per git hook."

Version: `https://registry.npmjs.org/simple-git-hooks/latest` returns `"version":"2.14.0"`; latest
GitHub release `2.14.0`, published `2026-08-28`.

**Native git hooks** — git's own documentation says the hook receives no file list at all
([githooks](https://git-scm.com/docs/githooks)):

> "This hook is invoked by [git-commit[1]](/docs/git-commit), and can be bypassed with the `--no-verify`
> option. It takes no parameters, and is invoked before obtaining the proposed commit log message and
> making a commit."

And `core.hooksPath` is the mechanism the hook managers use
([git-config](https://git-scm.com/docs/git-config#Documentation/git-config.txt-corehooksPath)):

> "By default Git will look for your hooks in the `$GIT_DIR/hooks` directory. Set this to different
> path, e.g. `/etc/git/hooks`, and Git will try to find your hooks in that directory …"

> "This configuration variable is useful in cases where you'd like to centrally configure your Git
> hooks instead of configuring them on a per-repository basis"

The consequence: **"changed files only" is a property of the runner, never of git.** Whichever tool is
chosen, the file list is computed by the tool.

#### Type-checking the changed files — the honest answer

The project's rule is "a git hook runs lint and types on the changed files". The lint half is
straightforward. The types half runs into a documented TypeScript limitation
([tsconfig.json](https://www.typescriptlang.org/docs/handbook/tsconfig-json.html)):

> "**When input files are specified on the command line, `tsconfig.json` files are ignored.**"

That is the whole problem in one line. `tsc --noEmit src/features/foo/x.ts` silently abandons the
project configuration — paths, `jsx`, `strict`, everything. TypeScript documents no
"check only these files" mode; a project is compiled as a project, with its file set coming from
`files`/`include`/`exclude`. `--incremental` accelerates repeated *full-project* checks and is not a
changed-files mechanism.

lint-staged's README documents both the trap and the official-shaped workaround (quoted above):
invoke `tsc` through a function so that **no filename arguments are passed**, and let TypeScript check
the project.

There is a community tool, `tsc-files` (registry: `"version":"1.1.4"`,
`"description":"A tiny tool to run tsc on specific files without ignoring tsconfig.json"`), whose
README says exactly that it exists because TypeScript ignores `tsconfig.json` when files are passed.
It is **not** a TypeScript-official tool, and it works by writing a temporary `tsconfig.json` whose
`files` array contains only the staged files — which by construction cannot catch a break introduced
in an unchanged importer.

#### The lint half of the hook: `--cache` and what it does not do

ESLint's own CLI documentation
([Command Line Interface](https://eslint.org/docs/latest/use/command-line-interface)) documents the
changed-files lever:

> "#### `--cache`
> Store the info about processed files in order to only operate on the changed ones. Enabling this
> option can dramatically improve ESLint's run time performance by ensuring that only changed files
> are linted.
> The cache is stored in `.eslintcache` by default."

And the caveat that matters precisely because this project wants typed linting **and** import-direction
rules:

> "ESLint determines whether a cached result is still valid from the file itself … and the
> configuration. It does not track dependencies between files, so a file's cached results are reused
> even when another file it depends on has changed. Rules whose results depend on other files can
> therefore report stale results. This includes type-aware rules, such as those in
> [`@typescript-eslint/eslint-plugin`](https://typescript-eslint.io/packages/eslint-plugin), and rules
> that resolve imports across modules, such as those in
> [`eslint-plugin-import`](https://github.com/import-js/eslint-plugin-import) and
> [`eslint-plugin-import-x`](https://github.com/un-ts/eslint-plugin-import-x)."

> "Run ESLint without `--cache` when you need accurate results from such rules."

`--concurrency` exists and defaults to `off`.

## Reasoning, and what this means for the project

The section above is reportage. This one is mine, and it is labelled as such.

### The test-runner question in ADR-0004 now has a clean answer

The ticket asks whether Vitest is confirmed for Next.js. It is: Next.js maintains a Vitest guide, an
official `with-vitest` example, and recommends Vitest together with React Testing Library. Vitest is
not a second-class citizen behind Jest — but the reason to choose it over Jest is not
"Next.js supports it". It is that Jest's advantage on this stack is one thing and Vitest's is another,
and they are not symmetrical:

| | Vitest | Jest |
|---|---|---|
| Officially documented by Next.js | Yes, own guide | Yes, own guide |
| `async` Server Components | Not supported; use E2E | Not supported; use E2E |
| Configuration | `vitest.config.mts`, `@vitejs/plugin-react`, `environment: 'jsdom'`, `vite-tsconfig-paths` | `jest.config.ts` through `next/jest` |
| CSS / image / `next/font` imports | Not handled; project writes the config | Auto-mocked by `next/jest` |
| The runner's own status | Not documented by Vitest as a Next.js runner at all | `next/jest` still labelled experimental on its own error page |
| Speed model | Vite, transform-on-demand, watch by default | SWC transform via `next/jest`, `--watch` |

My reading: **Vitest**, because the project's four levels are already split across two runners
(Vitest for unit and component, Playwright for E2E), so the only question is which one writes less
Next.js-specific config — and the answer depends on how much the project imports CSS modules and
`next/font` inside components under test. `next/jest`'s auto-mocking is real capability, not
marketing, and a component test that renders a component importing a CSS module does need that work
done somewhere. On the other hand, Vitest's config is a handful of lines in the official guide, runs
under the same Vite pipeline the project will already have through Next.js's tooling, and the
`next/jest` transformer is still documented as experimental more than four years after Next.js 12.
Either choice satisfies the rules; the decisive input is the CSS/font import surface, which this
project does not have yet.

**What is not a reason to choose either:** RSC testing. Neither runner is supported for `async`
Server Components, and Next.js's own recommendation is E2E for them. If the "about me" page or the
project list is an `async` Server Component — which, under the App Router, is the default shape —
then the **integration** level (a screen with the network substituted) cannot be a Vitest test
against that component. It has to render a *sync* component with the data passed in, or run against a
real server (Playwright). This is a design constraint on the screens, not a tooling choice, and it
should be settled before the first screen is written.

### The `create-next-app` finding is a convenience, not a constraint

`create-next-app` will not scaffold the test runner. The testing guide's own route is
`--example with-vitest`, which produces a pinned snapshot of an example app — and that example is
pinned to `vitest ^3.2.4`, behind the current Vitest 5 line. So the example is useful as a shape, not
as a source of versions. The project's own rule — versions pinned exactly, in the pnpm catalog — is
the right frame here: the example's config is worth copying, its versions are not.

### The `async` Server Component boundary is the real architectural finding

Three independent primary sources converge: Next.js's testing overview, its Vitest guide and its Jest
guide all say the same thing. For a project whose first screen is "an about me block, then a list of
projects" served from PostgreSQL, that means the integration level as specified — *a screen with the
network substituted by MSW* — must target a synchronous component whose data arrives as props or via
a client-side fetch. The tutorial-shaped test in Next.js's own Vitest guide, `render(<Page />)` where
`Page` is exported from `app/page.tsx`, works only because that example page is synchronous.

This does not invalidate the project's four levels. It defines where the seam is: the data-fetching
Server Component is exercised by Playwright, and everything below it — the presentational component,
its four states, the MSW-substituted screen — is exercised in Vitest. That split should be written
down, because it determines how the FSD `pages` layer is allowed to look.

### MSW on the App Router is the least confirmed area

MSW itself is exactly what the rules want: request-level interception, no module mocking, handlers
shared between the browser and tests, and the maintainer's own words that "your tests know _nothing_
about mocking". The Node setup is fully documented and works with Vitest verbatim
(`beforeAll`/`afterEach`/`afterAll` around `setupNode`). The **client-component** case is fully
documented too (`setupWorker`, `msw init`, await `worker.start()`).

What is *not* settled by primary sources is the **Server Component / RSC** case in a Next.js App
Router application. The facts as retrieved: the Next.js docs never mention MSW; MSW publishes no
Next.js integration guide; there is no merged Next.js example in the official examples repo, only an
open draft PR pinned to MSW 2.x; the tracking issue was closed as completed but its remaining blocker
is a Vercel-side HMR issue; and the only official reference implementation the maintainer points at
uses a `process.env.NEXT_RUNTIME === 'nodejs'` guard in a layout, with an explicit "must not use the
instrumentation hook" instruction.

I could not confirm from a primary source that MSW 3 works against server-side fetches under the App
Router as it stands today. That is an honest gap, and it is the one finding in this note most likely
to change the shape of the plan. It does not block the integration level as the rules define it —
that level renders a screen and intercepts its network, which is the Node path and is documented —
but it does mean the project should not assume MSW can intercept RSC data fetching until it is tried.
The mitigation is the same one the `async` finding implies: keep the fetching boundary thin and
behind the FSD `entities`/`features` public API, so a screen can be assembled with its data injected.

### The boundaries tool: `eslint-plugin-boundaries`, with `eslint-plugin-import` alongside

Of the candidates, only one has an official document that models a *layer direction with a policy*
and supports flat config:

- **`eslint-plugin-boundaries@7.2.0` (released 2026-08-09)** — README documents flat config, its own
  docs state ESLint v9+ compatibility, and its policy syntax expresses "an element of type
  `features` may import types `entities` and `shared`, nothing else". Its own docs recommend pairing
  it with `eslint-plugin-import` rather than replacing it. This is the recommendation.
- **`eslint-plugin-import` with `import/no-restricted-paths`** — resolves paths rather than matching
  specifier strings, so it handles `../../` correctly, but it is zone-shaped (target/from) and its
  ESLint peer range stops at `^9`. It is pulled in transitively by `eslint-config-next@16.4.0`
  anyway. Use it for what boundaries does not cover; do not make it the only gate.
- **Core `no-restricted-imports`** — cannot do the job alone, because it matches import strings and
  knows nothing about layers or resolution. Usable as a cheap backstop for absolute aliases.
- **`@feature-sliced/eslint-config@0.1.1`** — the FSD project's own package, and it should not be
  used: self-labelled WIP/beta, last release February 2022, documented in `.eslintrc` form only, and
  ESLint 10 does not support `.eslintrc` at all. Its own dependencies are `eslint-plugin-boundaries`
  and `eslint-plugin-import`, so it is a wrapper around an older version of the recommendation above.

Feature-Sliced Design itself offers no enforcement tool in its documentation — the linter page it
used to have is a 404. That is worth recording, because it means the project enforces FSD's rule with
a general-purpose architecture-boundaries plugin, and the mapping from FSD's six layers to boundary
element types is the project's own artefact, not something FSD or the plugin supplies.

### ESLint flat config: the stack is mid-transition, and the transition is visible

Three things changed together and any one of them breaks a 2024-era config:

1. `.eslintrc` is removed in ESLint 10 (`eslint@10.12.0` is current).
2. `next lint` is removed in Next.js 16; linting is `eslint .` through the CLI.
3. `typescript-eslint@8.71.1` is one package, and `tseslint.config(...)` is deprecated in favour of
   ESLint's own `defineConfig(...)`.

And a fourth that is not a change but a live friction: **ESLint 10 is out ahead of its ecosystem.**
Next.js documents this itself ("Some of the plugins included in `eslint-config-next` don't list ESLint
10 in their peer dependencies yet, so your package manager may show peer dependency warnings, or fail
when strict peer dependencies are enabled"), and `eslint-plugin-import@2.32.0` is the concrete
example. With pnpm's strict node_modules, that is worth planning for: either pin ESLint 9 for the
first frontend change, or accept and configure the peer warnings. The project's rule "a version bump
is a separate change, made after reading the release notes" argues for starting on a version whose
peer graph is actually consistent.

`parserOptions.projectService: true` is the setting that makes the project's prohibition on `any`
and type suppression enforceable by a rule rather than by review, and it should be on from the start —
retrofitting typed linting later means fixing a backlog.

### The hook: lefthook, and types cannot honestly be "changed files only"

The project's own monorepo layout decides this, and it is the reason I would not follow Prettier's
Option 1:

- **lint-staged's documented monorepo model is a config file per package** ("Install _lint-staged_ on
  the monorepo root level and add separate configuration files in each package. … _Lint-staged_ does
  not merge config files, so if the closest config file to a staged file doesn't match it, the file
  will be ignored"). For `apps/web` + `apps/<second-app>` + five packages that is a config file per
  workspace package, and the "does not merge" rule means a file that no config matches is silently
  ignored — which is the opposite of a quality gate.
- **Lefthook documents a single root config** with `{staged_files}` templates, `glob` per job,
  `root` to run a job in a subdirectory, sequential-or-parallel execution, and `stage_fixed` to
  re-stage what a formatter rewrote — which is exactly what a monorepo-wide `pre-commit` needs.
  `lefthook@2.1.17` is current and actively released.

So: **lefthook with a root `lefthook.yml`**, with a note about pnpm's `onlyBuiltDependencies`
requirement from Lefthook's own install page.

On "types on the changed files", I would not implement the rule literally, because the primary source
says the literal version is wrong: `tsc` ignores `tsconfig.json` when files are passed, so
"typecheck the changed files" either drops the project configuration or needs a third-party shim
(`tsc-files`, which cannot see cross-file breakage). The honest reading of the project's intent —
"a defect does not reach review" — is satisfied by running project-wide `tsc --noEmit` from the hook,
gated by a staged-file glob so it only runs when TypeScript files are staged. That is precisely what
lint-staged's own README recommends, and it is what Lefthook's `glob` + a job whose `run` does not
receive `{staged_files}` expresses. The hook stays fast because most commits touch no `.ts`/`.tsx`;
the check stays correct because when it runs, it runs over the project.

The same reasoning applies to `--cache`: ESLint's own documentation says cached results are stale for
exactly the two rule families this project cares most about (type-aware rules and import-resolving
rules). Using `--cache` in the hook to speed up lint would quietly weaken both the typed-linting gate
and the FSD boundary gate. Cache in CI for the non-typed pass if speed demands it; do not cache the
boundary check.

### Playwright: config in `apps/web`, CI at the workspace root

The official docs give no monorepo guidance, so any placement is an inference from two documented
primitives: `testDir` "relative to this configuration file", and the CLI default of
`playwright.config.ts` "in the current directory". The natural placement for a single Next.js app is
`apps/web/playwright.config.ts` with `testDir: './e2e'`, dependencies and the `@playwright/test`
version in the catalog, and the browser install plus test run executed from the workspace root with
`pnpm --filter web exec playwright test` — which is an inference, not an official recommendation, and
should be recorded as such when it is decided.

For CI, the official checklist is short and specific: install dependencies, `npx playwright install
--with-deps`, run `npx playwright test`, upload `playwright-report/`. The two official additions worth
taking: `workers: 1` in CI ("to prioritize stability and reproducibility"), and a Docker image pinned
to the exact Playwright version (`mcr.microsoft.com/playwright:v1.63.0-noble`) if a container is used,
because a version mismatch means Playwright cannot find its browsers. Sharding and the `blob`
reporter are the documented path when the suite outgrows one job; a first page with a handful of
critical paths does not need them yet.

Note that Next.js also has a Playwright guide, so the project can take both the framework's and the
tool's own instructions and they will agree on the essentials.

## Not confirmed from a primary source

Every item below is a gap in this note, not a decision deferred.

- **That MSW 3 intercepts server-side fetches under the Next.js App Router today.** No MSW
  documentation page covers it; the Next.js docs never mention MSW; the official examples repo has no
  Next.js example; the tracking issue closed with the blocker moved to a Vercel HMR issue; and the
  reference implementation the maintainer points at is an open draft PR pinned to MSW 2.x. The
  server-side integration working is asserted in a maintainer comment in an issue thread, which is not
  documentation.
- **Where Playwright's config should live in a pnpm monorepo.** No official Playwright page discusses
  monorepos, pnpm workspaces or `pnpm --filter`. The placement recommended above is my inference from
  `testDir`'s "relative to this configuration file" and the CLI's default config lookup.
- **Whether `next/jest` is deprecated.** No primary source says so. The only dedicated page still
  describes it as experimental and to-be-stabilised; the Jest guide still documents it as the way to
  configure Jest. A package being old and labelled experimental is not the same as being deprecated,
  and this note does not claim the latter.
- **A `create-next-app` test-runner prompt.** The current CLI reference documents none, and the
  in-repo README is visibly stale. I cannot prove the absence from the live binary.
- **Whether `vitest.workspace.ts` is *removed* in Vitest 5 as opposed to deprecated.** The projects
  guide says the rename happened in 3.2 and that `workspace` is deprecated; the current config
  reference lists `projects` and no `workspace`. I did not retrieve an explicit removal statement.
- **An official statement that `setupServer` requires native `fetch` interception.** The MSW docs
  document Node ≥ 22 and frame the failure as a `fetch`-not-defined error; `FetchInterceptor` is
  verifiably a default interceptor in MSW's `setup-server.ts`, but no docs sentence states it as a
  requirement.
- **`@feature-sliced/eslint-config` flat-config support.** Not documented anywhere in its README.
  Recorded as "flat-config support not documented", not as "broken".
- **An explicit husky announcement removing `husky install` / `husky add`.** The v9 release bodies
  retrieved document only the `husky.sh`/`~/.huskyrc` deprecations; the absence of the old commands
  from current docs is consistent with removal but is not an explicit statement.
- **Any official ranking of husky, lefthook, simple-git-hooks or native hooks.** None exists. The
  recommendation above is my reasoning over the monorepo behaviour each tool documents for itself.
- **Prettier config-format deprecations.** No deprecation is documented on the Configuration page; I
  did not retrieve a changelog, so "nothing is deprecated" is unverified in the general case.
- **Versions I did not retrieve during this research:** `jest-environment-jsdom`,
  `vite-tsconfig-paths`, `@vitejs/plugin-react`, `@typeScript-eslint/*` sub-packages,
  `eslint-config-prettier`, `eslint-plugin-react`, `eslint-plugin-react-hooks`,
  `@next/eslint-plugin-next`. Where an official example pins one (`@vitejs/plugin-react ^5.0.1`,
  `@testing-library/jest-dom 6.1.5` in the official `with-vitest`/`with-jest` examples, `jsdom
  ^26.1.0`), the pin is reported as the example's, not as the current release.
- **Point-in-time drift.** Every version and date above is what a primary source returned during this
  research session. They will move, and a version stated here is a snapshot, not a decision.
