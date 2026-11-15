# `backend/content/` — the site's texts

`site-content.json` holds the site's content: the owner's profile and the projects shown on the
page. It is versioned next to the code and written to the database by the seed command, so a text
change is a reviewed file change rather than a migration or a one-off script
([ADR-0007](../../docs/adr/0007-content-seeding-through-a-command.md)).

## Writing the file into the database

From `backend/`:

```powershell
docker compose up -d
dotnet ef database update -p src/MySite.Infrastructure.Postgres -s src/MySite.Web
dotnet run --project src/MySite.Web -- seed --file ../../content/site-content.json
```

The path is resolved against the process's current directory. `dotnet run` starts the application
with `src/MySite.Web` as its working directory, so from `backend/` the file is
`../../content/site-content.json`; an absolute path works too. It is not resolved against the Web
project's content root. The command is not an endpoint and not a migration: it needs the same DI
composition as the application, and it runs before the web host starts serving. It prints what it
did and exits with `0` (seeded), `1` (invalid input — the reasons go to stderr) or `2` (database
failure).

## Shape

```json
{
  "profile": {
    "translations": {
      "ru": { "headline": "…", "about": "…" },
      "en": { "headline": "…", "about": "…" }
    }
  },
  "projects": [
    {
      "link": "https://github.com/IKalentsov/MySite_dev",
      "year": 2026,
      "sortOrder": 2,
      "isPublished": true,
      "stack": [".NET 10", "PostgreSQL"],
      "translations": {
        "ru": { "title": "…", "summary": "…" },
        "en": { "title": "…", "summary": "…" }
      }
    }
  ]
}
```

| Field | Required | Rules |
|---|---|---|
| `profile` | yes | the profile is a singleton: the file cannot express two |
| `profile.translations` | yes | an object keyed by locale; at least one entry |
| `profile.translations.<locale>.headline` | yes | non-empty, at most 200 characters |
| `profile.translations.<locale>.about` | yes | non-empty |
| `projects` | yes | the list of projects; the file may contain none, but then nothing is seeded |
| `projects[].link` | yes | the project's identity — a link to the repository or the demo; unique within the file, at most 500 characters |
| `projects[].year` | yes | the year the list is ordered by: newest first |
| `projects[].sortOrder` | yes | the position inside a year |
| `projects[].isPublished` | yes | `false` keeps the project out of the public list |
| `projects[].stack` | no | technology names in display order; each non-empty and at most 100 characters |
| `projects[].translations` | yes | an object keyed by locale; `title` at most 200 characters and `summary` at most 500, both non-empty |

An unknown top-level field is ignored; an unknown locale, a duplicate `link`, a missing `ru` text and
an empty required field are validation errors and nothing is written.

## Locales

Only `ru` and `en` exist. `ru` is the default language and is required for the profile and for every
project. `en` is optional: a project without an `en` key gets no English row, and the API answers
with the Russian text ([ADR-0005](../../docs/adr/0005-bilingual-content-storage.md)) — that is the
deliberate "this project is untranslated" state, and the seed report lists it as a translation gap.

Two texts for one locale cannot be expressed: the translations are a JSON object keyed by locale, so
a duplicated key is a parse error rather than a silent overwrite.

## A rerun converges

The command is idempotent, inside one transaction:

- the profile is created when the table is empty and updated in place on later runs;
- a project is identified by its `link`: present in the database — updated, absent — inserted;
- a project's translation rows become exactly the locales present in the file, so removing `en`
  deletes the English row;
- a project's stack becomes exactly the list in the file;
- a project present in the database but missing from the file is reported and left alone, never
  deleted.

A second run over an unchanged file therefore changes no rows, and the report says
`Projects: created 0, updated 0, unchanged N`.

## The current texts are placeholders

Every text in `site-content.json` today starts with `Заглушка:` (Russian) or `Placeholder:`
(English), so it cannot be mistaken for the final copy. They exist so the page is not empty and the
frontend can be built against live data; ticket 07 replaces them with the owner's own texts. Two
projects are deliberately unusual and must stay that way while the placeholders live:

- **Harness** has no `en` translation — it is the live check of the fallback to Russian;
- **Keycloak_Learn** has `isPublished: false` — it is the live check that an unpublished project
  does not appear in the list.
