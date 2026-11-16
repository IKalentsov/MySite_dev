import createClient from "openapi-fetch";

import type { paths } from "./generated/openapi";

// The API the site runs against locally; any other environment sets API_BASE_URL.
const defaultBaseUrl = "http://localhost:5054";

/** Builds a typed client for one request; the caller decides its lifetime. */
export function createApiClient(baseUrl: string = process.env.API_BASE_URL ?? defaultBaseUrl) {
  return createClient<paths>({ baseUrl });
}
