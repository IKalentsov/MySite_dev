# MySite

The personal site of Илья Каленцов: a page about him as a developer, and the projects he has
built. Russian by default, English behind a switch.

It is built to grow. It starts as one public page, laid out so that the administration panel and
more content can follow without being rebuilt from scratch.

## Where things are

| Path | What |
|---|---|
| `backend/` | The .NET 10 API — `src/MySite.{Domain,Application,Contracts,Infrastructure.Postgres,Web}` plus `tests/` |
| `frontend/` | Reserved for the Next.js app; not started |
| `.dsh/` | Agent instructions and the skills that go with them |
| `ARCHITECTURE.md` | The stack, the projects and the direction dependencies may point |
| `WORKFLOW.md` | Build, test and run commands, and what "done" means |
| `CONTEXT.md` | The project's vocabulary |
| `docs/adr/` | Decisions that are hard to reverse, and why |

## Building and running

Commands are in `WORKFLOW.md`. Today the API builds clean and its three test projects are empty
skeletons; the database schema is created by a single migration, so the API can run against a fresh
database once the connection string is set, and the frontend is not written.
