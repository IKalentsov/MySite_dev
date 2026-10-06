# Bilingual content lives in per-locale translation tables

Russian is the site's primary language and English sits behind a switch, so every piece of content
the owner writes has up to two texts, edited independently — a Russian text can be rewritten while
the English one waits. Each translatable entity therefore keeps its language-independent columns in
its own table, and its texts in a `<entity>_translations` table keyed by `(entity_id, locale)`, with
`locale` a short code (`ru`, `en`). The evidence behind this is in
[`docs/research/i18n-content-storage.md`](../research/i18n-content-storage.md).

## Considered options

- **Per-language columns** (`title_ru`, `title_en`) — the simplest thing that works for exactly two
  permanent locales, and rejected only because the site is meant to grow: every new language would
  become a migration across every content table. Partial translation also spreads across nullable
  columns, so "which languages exist for this row" stops being one fact.
- **A `jsonb` document holding every locale** — rejected on PostgreSQL's own guidance that a JSON
  document should be "an atomic datum that business rules dictate cannot reasonably be further
  subdivided into smaller datums that could be modified independently": two independently edited
  translations are precisely such datums. The structure is also unenforced, so nothing would prevent
  a locale key from being misspelled or missing.
- **A translations table per translatable entity** — chosen.

## Consequences

- Reading content costs one join. For a page with a profile and a list of projects that is two joins,
  and it is the price of locales being data rather than schema.
- Adding a language is inserting rows, not running a migration.
- **The fallback belongs to the application layer.** A query returns the requested locale and the
  default one; the application picks the requested text and falls back to Russian when the
  translation is absent. Keeping it out of SQL means the rule is testable without a database, and an
  untranslated project shows its Russian text rather than disappearing.
- **The API takes an explicit `locale`, and ignores `Accept-Language`.** The site's locale is a fact
  about the URL, not about the browser: after a visitor flips the switch to English, their browser
  still advertises Russian. An explicit parameter is also cacheable per URL and trivial to test.
- **The public routes are `/[lang]/...`.** That is the shape the Next.js guide recommends for
  internationalised routing, with the locale detected from `Accept-Language` on the way in and every
  route nested under `app/[lang]/`.
- The administration panel owes the owner per-locale editing: two text fields for a project, and a
  visible way to see which translations are still missing.
