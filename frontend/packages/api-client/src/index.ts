import type { components } from "./generated/openapi";
import { ApiError } from "./api-error";
import { createApiClient } from "./client";

export { ApiError } from "./api-error";
export { createApiClient } from "./client";

export type OwnerProfile = components["schemas"]["OwnerProfileResponse"];
export type Project = components["schemas"]["ProjectResponse"];
export type HomeResponse = components["schemas"]["HomeResponse"];

/** The home page content: the owner's profile, which may be absent, and the published projects. */
export async function getHome(locale: string): Promise<HomeResponse> {
  const { data, response, error } = await createApiClient().GET("/api/v1/home", {
    params: { query: { locale } },
    cache: "no-store",
  });

  if (!data) {
    throw new ApiError(response.status, error);
  }

  return data;
}
