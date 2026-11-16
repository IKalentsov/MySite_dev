import { defineConfig, devices } from "@playwright/test";

/**
 * Playwright configuration for the MySite web app.
 * Runs against `next start` on port 3100 so it does not conflict with dev server (3000).
 */
export default defineConfig({
  testDir: "./e2e",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: "html",
  use: {
    baseURL: "http://127.0.0.1:3100",
    trace: "on-first-retry",
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],
  webServer: {
    command: "pnpm exec next start -p 3100",
    port: 3100,
    reuseExistingServer: !process.env.CI,
  },
});
