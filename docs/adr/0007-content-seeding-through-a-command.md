# Content enters the database through an idempotent seed command

The page reads a profile and a list of projects, and until the administration panel exists nothing
writes them: the schema is there, the endpoints are there, the tables are empty. The content has to
come from somewhere that is reviewable, repeatable and needs no write surface on the API.

A text file in the repository is read by a command and written to the database in one transaction.

## Considered options

- **`HasData` in the EF model** — rejected. The project generates its migrations and treats one
  migration as one coherent change; under `HasData` every wording change to a project summary
  becomes a new migration, so content edits and schema history are welded together.
- **A development-only write endpoint** — rejected. ADR-0003 keeps the public phase free of write
  surfaces, and an unauthenticated endpoint that inserts content is exactly such a surface, "only
  in Development" or not.
- **A hand-written SQL script** — rejected. It validates neither locale codes nor required text,
  drifts silently from the schema, and duplicates the entity rules the domain already enforces.
- **A command that seeds from a versioned file** — chosen.

## Consequences

- **The texts live in `backend/content/site-content.json`.** Content is data of the product, not of
  a project, so it does not sit inside `src/`. The path is passed on the command line and resolved
  against the current directory, never against the Web project's content root: `dotnet run` starts
  the application with `src/MySite.Web` as its working directory, so from `backend/` the file is
  passed as `../../content/site-content.json`, and an absolute path works as well.
  `backend/content/README.md` documents the shape.
- **The command is `dotnet run --project src/MySite.Web -- seed --file <path>`**, dispatched from
  `Program.cs` before the web host starts serving. It is not an endpoint and not a separate
  executable: it needs the same DI composition as the application, nothing more.
- **A project's identity is its `link`.** The entity has no slug, and adding one is a schema change
  this decision deliberately excludes, so the file is the authority: `link` is required and unique
  within the file, and a second row with the same link is a validation error. A changed link is
  therefore a new project — acceptable while the page has no per-project route, and the reason a
  slug column is the follow-up if one appears.
- **The profile is a singleton.** A profile row is created when the table is empty and updated in
  place otherwise; the file cannot express two profiles.
- **A rerun converges, inside one transaction.** Each project is inserted or updated by link; that
  project's translation rows become exactly the locales present in the file, and its stack items
  become exactly the file's list. Removing `en` from a project removes its English row, which is how
  "this project is untranslated on purpose" survives a rerun.
- **A project missing from the file is left alone and reported, never deleted.** Entity-level
  deletion is out of scope: the command promises convergence of what it owns, not of the table.
- **An incomplete translation is the absence of a locale key.** No row is written, and the domain's
  `TranslationFor` already answers with the default language's text (ADR-0005), so the project stays
  visible rather than disappearing. The validator requires `ru` — the default language — and only
  reports a missing `en` in the summary, because an untranslated project is a state the seed has to
  be able to produce on purpose.
- **Two locales of one entity are two rows in the same transaction.** The file cannot express two
  texts for one locale: the translations are a JSON object keyed by locale, so a duplicate key is a
  parse error rather than a silent overwrite.
- **Validation is a pure function in `Application`, the upsert lives in `Infrastructure.Postgres`,
  and only the argument dispatch lives in `Web`.** The validator rejects an unknown locale, a
  duplicate link, a missing `ru` text and an empty required field; it is testable without a
  database. Exit codes: `0` seeded, `1` invalid input, `2` database failure.
- **The command reports what it did:** created, updated and unchanged counts, the deliberate
  translation gaps, and the projects present in the database but absent from the file.
- Content is reviewable in a pull request like code, and a wrong text is fixed by editing the file
  and rerunning — not by a migration and not by a script nobody keeps.
