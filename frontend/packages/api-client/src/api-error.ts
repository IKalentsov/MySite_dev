import type { components } from "./generated/openapi";

type ProblemDetails = components["schemas"]["ProblemDetails"];

/** A non-2xx answer from the API, carrying the RFC 7807 document it sent. */
export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails | undefined;

  constructor(status: number, problem?: ProblemDetails) {
    super(problem?.title ?? problem?.detail ?? `The API answered ${String(status)}.`);
    this.name = "ApiError";
    this.status = status;
    this.problem = problem;
  }
}
