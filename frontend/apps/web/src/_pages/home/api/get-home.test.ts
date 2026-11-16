import { expect, test } from "vitest";
import { http, HttpResponse } from "msw";
import { server } from "./testing/vitest.setup";
import { getHome, ApiError } from "@mysite/api-client";

test("getHome returns HomeResponse on success", async () => {
  const result = await getHome("ru");

  expect(result.profile).not.toBeNull();
  expect(result.profile?.locale).toBe("ru");
  expect(result.projects.items.length).toBe(1);
  expect(result.projects.items.at(0)?.title).toBe("MySite");
  expect(result.projects.hasMore).toBe(false);
});

test("getHome throws ApiError on non-2xx response", async () => {
  server.use(
    http.get("http://localhost:5054/api/v1/home", () => {
      return HttpResponse.json(
        { status: 400, title: "Bad Request", detail: "Invalid locale" },
        { status: 400 },
      );
    }),
  );

  await expect(getHome("xx")).rejects.toThrow(ApiError);
});
