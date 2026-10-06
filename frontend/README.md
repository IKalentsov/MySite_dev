# frontend — reserved

This directory is reserved for the site's client side. It holds no code and no configuration on
purpose: the stack has not been decided yet, and inventing `tsconfig`, ESLint, Prettier or root
scripts would produce files that diverge from reality at the first install.

The harness base's own `frontend/templates/` is empty for exactly this reason.

The direction the harness requires is stated in the Frontend section of `.dsh/AGENTS.md`
(Vite + React + TypeScript, Tailwind, a query library, a client generated from the backend
OpenAPI schema, WCAG 2.2 AA). Stack, versions and whether the site needs SSR are still open
project decisions.
