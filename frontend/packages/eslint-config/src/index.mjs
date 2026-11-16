import { resolve } from "node:path";

import js from "@eslint/js";
import prettier from "eslint-config-prettier/flat";
import boundaries from "eslint-plugin-boundaries";
import importPlugin from "eslint-plugin-import";
import globals from "globals";
import tseslint from "typescript-eslint";

const monorepoRoot = resolve(import.meta.dirname, "../../..");

const tsFiles = ["**/*.{ts,tsx}"];

// Feature-Sliced Design layers of apps/web plus the workspace packages below
// them. Patterns are anchored at the frontend root (`partialMatch: false`).
const elements = [
  { type: "route", pattern: "apps/web/src/app", partialMatch: false },
  { type: "pages", pattern: "apps/web/src/_pages/*", partialMatch: false },
  { type: "widgets", pattern: "apps/web/src/widgets/*", partialMatch: false },
  { type: "features", pattern: "apps/web/src/features/*", partialMatch: false },
  { type: "entities", pattern: "apps/web/src/entities/*", partialMatch: false },
  { type: "shared", pattern: "apps/web/src/shared", partialMatch: false },
  { type: "ui-package", pattern: "packages/ui", partialMatch: false },
  { type: "api-client", pattern: "packages/api-client", partialMatch: false },
  { type: "shared-package", pattern: "packages/shared", partialMatch: false },
];

// A layer may import only the layers strictly below it. Packages are below
// `shared`; `shared` and `ui-package` are slices as well as layers, so their
// internal imports are covered by the "internal" policy at the end.
//
// `api-client` is allowed from `pages` only: a slice's server-side data
// function is the single caller of the API (ticket 18), so a widget, feature,
// entity or shared module importing the client is a boundary violation.
const allowedImporters = [
  {
    from: "route",
    to: ["pages", "widgets", "features", "entities", "shared", "ui-package", "shared-package"],
  },
  {
    from: "pages",
    to: ["widgets", "features", "entities", "shared", "ui-package", "api-client", "shared-package"],
  },
  {
    from: "widgets",
    to: ["features", "entities", "shared", "ui-package", "shared-package"],
  },
  {
    from: "features",
    to: ["entities", "shared", "ui-package", "shared-package"],
  },
  {
    from: "entities",
    to: ["shared", "ui-package", "shared-package"],
  },
  {
    from: "shared",
    to: ["ui-package", "shared-package"],
  },
  {
    from: "api-client",
    to: ["shared-package"],
  },
];

const boundariesPolicies = [
  ...allowedImporters.map(({ from, to }) => ({
    from: { element: { type: from } },
    allow: { to: { element: { types: to } } },
  })),
  // Slices inside one layer may compose their own files freely; crossing into
  // a sibling slice is what the policy above forbids.
  { allow: { dependency: { relationship: { to: "internal" } } } },
];

// The second belt, from ticket 10: `import/no-restricted-paths` resolves the
// imported file instead of matching the import string, so `../../` is caught.
const restrictedPathZones = [
  {
    target: "./apps/web/src/shared",
    from: [
      "./apps/web/src/app",
      "./apps/web/src/_pages",
      "./apps/web/src/widgets",
      "./apps/web/src/features",
      "./apps/web/src/entities",
    ],
  },
  {
    target: "./apps/web/src/entities",
    from: [
      "./apps/web/src/app",
      "./apps/web/src/_pages",
      "./apps/web/src/widgets",
      "./apps/web/src/features",
    ],
  },
  {
    target: "./apps/web/src/features",
    from: ["./apps/web/src/app", "./apps/web/src/_pages", "./apps/web/src/widgets"],
  },
  {
    target: "./apps/web/src/widgets",
    from: ["./apps/web/src/app", "./apps/web/src/_pages"],
  },
  {
    target: "./apps/web/src/_pages",
    from: "./apps/web/src/app",
  },
  {
    target: "./packages",
    from: "./apps",
  },
];

const scopedTsConfigs = [js.configs.recommended, ...tseslint.configs.recommendedTypeChecked].map(
  (config) => ({ ...config, files: tsFiles }),
);

export const config = [
  ...scopedTsConfigs,
  {
    files: tsFiles,
    languageOptions: {
      globals: {
        ...globals.browser,
        ...globals.node,
      },
      parserOptions: {
        projectService: true,
        tsconfigRootDir: monorepoRoot,
      },
    },
    plugins: {
      boundaries,
      import: importPlugin,
    },
    settings: {
      "boundaries/root-path": monorepoRoot,
      "boundaries/elements": elements,
      "import/resolver": {
        typescript: {
          alwaysTryTypes: true,
          // A monorepo has one tsconfig per package by design (ticket 14's
          // layout); the resolver's advisory about that is not a finding.
          noWarnOnMultipleProjects: true,
          project: [
            "apps/web/tsconfig.json",
            "packages/ui/tsconfig.json",
            "packages/api-client/tsconfig.json",
            "packages/shared/tsconfig.json",
          ],
        },
      },
    },
    rules: {
      "boundaries/dependencies": [
        "error",
        {
          default: "disallow",
          message:
            "FSD boundary violated: {{from.element.types}} may not import {{to.element.types}}.",
          policies: boundariesPolicies,
        },
      ],
      "import/no-restricted-paths": [
        "error",
        { basePath: monorepoRoot, zones: restrictedPathZones },
      ],
    },
  },
  {
    // @mysite/ui exposes one entry point and one CSS subpath (ticket 13);
    // @mysite/api-client exposes one entry point and hides the generated types
    // (ticket 15).
    files: [
      "apps/**/*.{ts,tsx}",
      "packages/api-client/**/*.{ts,tsx}",
      "packages/shared/**/*.{ts,tsx}",
    ],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            {
              group: ["@mysite/ui/*"],
              message: "Import from @mysite/ui root; deep imports are not public API.",
            },
            {
              group: ["@mysite/api-client/*"],
              message: "Import from @mysite/api-client root; deep imports are not public API.",
            },
          ],
        },
      ],
    },
  },
  prettier,
];
