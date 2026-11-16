import { expect, test } from "@playwright/test";

test("GET / redirects to /ru and the document has lang=\"ru\"", async ({ page }) => {
  await page.goto("/");

  // The redirect should land us on /ru
  await expect(page).toHaveURL(/\/ru$/);

  // The html element should have lang="ru"
  const html = page.locator("html");
  await expect(html).toHaveAttribute("lang", "ru");
});
