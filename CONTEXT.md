# CONTEXT.md

The project's vocabulary. A glossary and nothing else: no implementation notes, no spec, no
scratch pad. A term appears here once it has been settled in conversation, never in advance.

Terms are written in the language the site itself uses — Russian, the site's primary language —
with the explanation in English. The word in bold is the canonical one; "not" lines name what the
same word is wrongly used for in this repository.

## The site

**Сайт** — one developer's public site: who he is, what he builds, and what he has built. Russian
by default, with English behind a switch ([ADR-0003](docs/adr/0003-public-site-first-phase.md)).
It is deliberately built as a small thing that can become a larger product — not as a one-page
brochure that would have to be thrown away. Everything public is open: there is no sign-in in the
first phase.

## People

**Владелец** — the developer the site is about. He is also the only person who will ever sign in,
and only to the administration panel. Not "the user": the public side has no accounts.

**Посетитель** — anyone reading the public site. No account, no session, nothing to sign in to.
The only identity the site has for him is what he chooses to send through a contact form.

## Content

**Проект** — a piece of work the owner built and presents on the site: what a visitor reads about.
Not the repository you are working in, and not the `MySite.*` .NET solutions. The single word is
ambiguous here, so be explicit: **портфолио-проект** for the content, **репозиторий** or the
project's own name for the code.

**Страница «обо мне»** — the landing content: who the owner is and what he is like as a developer.
Not a CV and not a list of technologies; the list of work is its own thing.

**Админка** — the future private interface where the owner manages the site's content. Authentication
will be written for it from scratch ([ADR-0006](docs/adr/0006-identity-stack-removed.md)).
