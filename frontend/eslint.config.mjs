import { defineConfig, globalIgnores } from "eslint/config";

import { config } from "@mysite/eslint-config";

export default defineConfig([
  globalIgnores(["**/.next/**", "**/node_modules/**", "**/next-env.d.ts", "pnpm-lock.yaml"]),
  ...config,
]);
