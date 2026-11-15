// lefthook discovers its configuration at the Git root, but this workspace's
// configuration lives in frontend/ (the repository also holds backend/). The
// hook below therefore lives in frontend/.githooks, knows where the config is,
// and this script points Git at it. It is idempotent and runs from `prepare`.
import { execFileSync } from "node:child_process";

try {
  execFileSync("git", ["config", "core.hooksPath", "frontend/.githooks"], { stdio: "inherit" });
} catch {
  console.warn("frontend: not a Git checkout, skipped core.hooksPath setup");
}
