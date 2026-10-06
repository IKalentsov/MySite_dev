# Bilingual content storage — the evidence

The question: how should Russian and English content be stored in PostgreSQL for this site, and how
does that pair with Next.js App Router internationalisation? This note holds the evidence and the
sources; the decision itself is in
[ADR-0005](../adr/0005-bilingual-content-storage.md).

## What the primary sources actually say

### PostgreSQL on JSON documents

From the official documentation, [§8.14.2 "Designing JSON Documents"](https://www.postgresql.org/docs/current/datatype-json.html):

> "The structure is typically unenforced (though enforcing some business rules declaratively is
> possible), but having a predictable structure makes it easier to write queries that usefully
> summarize a set of 'documents' (datums) in a table."

> "Consider limiting JSON documents to a manageable size in order to decrease lock contention among
> updating transactions. Ideally, JSON documents should each represent an atomic datum that business
> rules dictate cannot reasonably be further subdivided into smaller datums that could be modified
> independently."

Two things follow that are easy to get wrong:

- **The usual objection to `jsonb` is wrong.** It is not that `jsonb` cannot be indexed — it can:
  the same page documents GIN indexes over `jsonb`, with `jsonb_ops` and `jsonb_path_ops` operator
  classes. The weakness is that the *structure is unenforced*.
- **The real objection is the atomicity guidance.** A profile's Russian text and its English text
  are edited independently — one can be rewritten without touching the other. That is exactly the
  case the documentation says should be "subdivided into smaller datums that could be modified
  independently", which points away from holding both languages in one document.

`jsonb` also does not preserve object key order or duplicate keys, and stores no information about
which keys *should* exist — so it cannot express "this project has a Russian title and no English
one yet" as anything other than an absent key.

### Next.js on locale routing

From the official guide, [Internationalization](https://nextjs.org/docs/app/guides/internationalization):

> "It's recommended to use the user's language preferences in the browser to select which locale to
> use. Changing your preferred language will modify the incoming `Accept-Language` header to your
> application."

> "Routing can be internationalized by either the sub-path (`/fr/products`) or domain
> (`my-site.fr/products`). With this information, you can now redirect the user based on the locale
> inside Proxy."

> "Finally, ensure all special files inside `app/` are nested under `app/[lang]`. This enables the
> Next.js router to dynamically handle different locales in the route, and forward the `lang`
> parameter to every layout and page."

So the framework's own pattern is: **locale as a URL sub-path**, detected from `Accept-Language` on
the way in, with every route nested under `app/[lang]/`. The guide also shows `generateStaticParams`
returning one entry per locale for static rendering, and reading the locale anywhere on the server
through `next/root-params`.

One consequence for this project: the toggle is an explicit user choice, while `Accept-Language`
reflects the *browser*, not the choice. Once somebody has switched to English, their browser still
advertises Russian. The URL is therefore the authority on the public site, not the header.

## The three shapes, stated fairly

| | Per-language columns (`title_ru`, `title_en`) | `jsonb` document per row | Translations table |
|---|---|---|---|
| Adding a third locale | Schema change on every translatable table | Data only | Data only |
| Partial translation | Spread across nullable columns; "which languages exist" is not a single fact | Absent keys; nothing enforces which locales may exist | A missing row; the set of locales is data |
| Constraints | `NOT NULL` per column | None on structure or locale | `UNIQUE (entity_id, locale)`, `NOT NULL`, FK — all enforced by the database |
| Reads | No join | No join | One join |
| Writes | Row update | Whole-document update, row lock on the whole row | Row insert/update per locale |

The per-language columns shape is not ridiculous — for exactly two permanent locales it is simpler
and faster, and it is what a two-language site can ship with. It is rejected here for one reason:
the owner's stated intent is that this site grows, and every new locale would then be a migration
across every content table instead of a row.

## Not confirmed from a primary source

- No official PostgreSQL page states a "best practice for localized content" as such; the guidance
  above is the JSON-document atomicity rule applied to this case, plus the ordinary relational
  argument. That reasoning is mine, and it is labelled as such.
- EF Core's JSON-column mapping (`ToJson()`) was not consulted, because the chosen shape does not use
  it. Nothing here should be read as a claim about its behaviour.
