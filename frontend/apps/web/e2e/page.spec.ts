import { expect, test } from "@playwright/test";

/**
 * The data path of the home page against the real local API and PostgreSQL: the page
 * must show the seeded content, drop unpublished projects, fall back to Russian where
 * the English translation is missing (ADR-0005) and switch locale through the URL.
 * Prerequisites: `pnpm build`, a running API on :5054 and the seeded database
 * (ticket 08); the exact commands are in WORKFLOW.md.
 */
const publishedProjects = ["DirectoryService", "MySite", "Harness"];

/**
 * The data path needs the seeded database and the API on :5054, which the CI job has
 * neither of (ticket 20 kept CI on the stack-free smoke path). The smoke, responsive
 * and theme specs run everywhere.
 */
const skipWithoutLocalStack = () =>
  test.skip(
    Boolean(process.env.CI),
    "The data path needs the local API and database: see WORKFLOW.md, End-to-end tests.",
  );

test.describe("home page /ru", () => {
  test("shows the profile, three published projects and the contacts", async ({ page }) => {
    skipWithoutLocalStack();

    await page.goto("/ru");

    await expect(page.getByRole("heading", { level: 1, name: "MySite" })).toBeVisible();
    await expect(page.getByRole("heading", { level: 2, name: "Обо мне" })).toBeVisible();
    await expect(page.getByRole("heading", { level: 2, name: "Проекты" })).toBeVisible();

    // The profile comes from the seeded database, not from the test itself.
    await expect(
      page.getByText("Заглушка: .NET-разработчик, восемь лет в промышленной разработке"),
    ).toBeVisible();

    for (const project of publishedProjects) {
      const card = page.getByRole("article").filter({ hasText: project });
      await expect(card.getByRole("heading", { level: 3, name: new RegExp(project) })).toBeVisible();
      await expect(
        card.getByRole("link", { name: new RegExp(`^Открыть проект: .*${project}`) }),
      ).toHaveAttribute("href", /^https:\/\/github\.com\//);
      await expect(card.getByText("2026")).toBeVisible();
    }

    await expect(page.getByRole("link", { name: "Telegram" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Почта" })).toBeVisible();
    await expect(page.locator("body")).not.toContainText("Keycloak");
  });
});

test.describe("home page /en", () => {
  test("shows English text, the Russian fallback for Harness and no unpublished project", async ({
    page,
  }) => {
    skipWithoutLocalStack();

    await page.goto("/en");

    await expect(page.getByRole("heading", { level: 2, name: "About me" })).toBeVisible();
    await expect(page.getByRole("heading", { level: 2, name: "Projects" })).toBeVisible();

    // Harness has no English translation, so ADR-0005 answers with the Russian title.
    await expect(
      page.getByRole("heading", {
        level: 3,
        name: "Заглушка: Harness — база для настройки агентной среды",
      }),
    ).toBeVisible();
    await expect(
      page.getByRole("heading", { level: 3, name: "Placeholder: MySite — this site" }),
    ).toBeVisible();

    await expect(page.locator("body")).not.toContainText("Keycloak");
    await expect(page.getByRole("link", { name: /^Open project: Placeholder: MySite/ })).toBeVisible();
  });
});

test.describe("locale switch", () => {
  test("moves between /ru and /en through the URL and marks the current locale", async ({
    page,
  }) => {
    skipWithoutLocalStack();

    await page.goto("/ru");

    const ruPageNav = page.getByRole("navigation", { name: "Переключатель языка" });
    await expect(ruPageNav.getByRole("link", { name: "Русский" })).toHaveAttribute(
      "aria-current",
      "page",
    );

    await ruPageNav.getByRole("link", { name: "English" }).click();
    await expect(page).toHaveURL(/\/en$/);

    const enPageNav = page.getByRole("navigation", { name: "Language switch" });
    await expect(enPageNav.getByRole("link", { name: "English" })).toHaveAttribute(
      "aria-current",
      "page",
    );

    await enPageNav.getByRole("link", { name: "Russian" }).click();
    await expect(page).toHaveURL(/\/ru$/);
  });
});

test.describe("responsive layout", () => {
  // 320px is the WCAG reflow width; 360px is the common small phone.
  for (const path of ["/ru", "/en"]) {
    for (const width of [320, 360]) {
      test(`has no horizontal scroll at ${width}px on ${path}`, async ({ page }) => {
        await page.setViewportSize({ width, height: 800 });
        await page.goto(path);

        const { scrollWidth, innerWidth } = await page.evaluate(() => ({
          scrollWidth: document.documentElement.scrollWidth,
          innerWidth: window.innerWidth,
        }));

        expect(scrollWidth).toBeLessThanOrEqual(innerWidth);
      });
    }
  }
});

test.describe("theme", () => {
  /** The token is authored in oklch, so the canvas normalises it to sRGB for a numeric check. */
  const bodyLuminance = (page: import("@playwright/test").Page) =>
    page.evaluate(() => {
      const color = getComputedStyle(document.body).backgroundColor;
      const canvas = document.createElement("canvas");
      canvas.width = 1;
      canvas.height = 1;
      const context = canvas.getContext("2d");

      if (!context) {
        throw new Error("A 2D canvas context is required to read the theme's colour.");
      }

      context.fillStyle = color;
      context.fillRect(0, 0, 1, 1);
      const [r = 0, g = 0, b = 0] = context.getImageData(0, 0, 1, 1).data;
      return 0.299 * r + 0.587 * g + 0.114 * b;
    });

  test("follows the system preference: dark tokens in dark mode, light in light mode", async ({
    page,
  }) => {
    await page.emulateMedia({ colorScheme: "dark" });
    await page.goto("/ru");
    await expect(page.locator("html")).toHaveClass(/dark/);
    expect(await bodyLuminance(page)).toBeLessThan(64);

    await page.emulateMedia({ colorScheme: "light" });
    await page.goto("/ru");
    expect(await bodyLuminance(page)).toBeGreaterThan(200);
  });
});
