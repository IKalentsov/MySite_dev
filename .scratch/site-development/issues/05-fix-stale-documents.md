# 05 — Починить устаревшие документы

Type: task
Status: open

## Question

Три документа противоречат коду. Это не решения, а долг — спрашивать владельца тут нечего.

1. **`ARCHITECTURE.md`**, § Known deviations: строка «Table and column names are not configured as
   `snake_case` … Add the convention together with the first migration». Конвенция уже есть:
   `Program.cs:38` вызывает `UseSnakeCaseNamingConvention()`, пакет `EFCore.NamingConventions
   10.0.1` стоит в `Directory.Packages.props`, в миграции — `project_translations`,
   `created_at_utc`.
2. **`docs/adr/0002-migrations-removed.md`**: заголовок и Consequences утверждают, что миграций
   нет и схема начинается с нуля. Миграция `20261006221740_ContentSchema` существует, схема
   создаётся ею, `WORKFLOW.md` описывает `dotnet ef database update`. Второй устаревший пункт
   того же ADR — про ненастроенный `snake_case`.
3. **`frontend/README.md`**: «the stack has not been decided yet… Vite + React + TypeScript», хотя
   ADR-0004 и `.dsh/AGENTS.md` давно выбрали Next.js App Router + FSD + pnpm-монорепу. Файл должен
   говорить, что стек решён, и указывать на ADR.

Заодно сверить `ARCHITECTURE.md` § Known problems со списком из этой карты: пункт «There is no seed
data» закрывается тикетом 08, пункты про авторизацию — тикетом 02.

## Answer

<!-- заполняется при закрытии -->
