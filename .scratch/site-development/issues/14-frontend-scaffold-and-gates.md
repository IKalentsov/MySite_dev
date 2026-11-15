# 14 — Скаффолд монорепы, гейты, CI фронтенда

Type: task
Status: resolved
Blocked by: 09, 10, 13, 17

## Как работать с этой задачей

**Правила работы с проектом — `.dsh/AGENT-TASKS.md`.** Прочитай его первым, до первой правки: там
команды, правила веток, среда и критерий готовности. Ниже — только специфика этой задачи.

- Ветка: `feature/frontend-scaffold-and-gates` от текущего `master`.
- Коммить в неё; **не пушить** и не трогать чужие задачи.
- Критерий готовности: `pnpm lint`, `pnpm typecheck`, `pnpm build` зелёные на пустом приложении, без
  `any`, без `@ts-expect-error`, без barrel-файлов. Версии пинуются точно и в одном месте — в
  pnpm-каталоге `pnpm-workspace.yaml` (решение тикета 17), не в отдельных `package.json`.
- Докер и миграции в этой задаче не нужны.

**Условие остановки:** `pnpm lint`, `pnpm typecheck` и `pnpm build` зелёные на пустом приложении; реальные команды гейтов дописаны в раздел Frontend файла `WORKFLOW.md`; CI фронтенда прогоняет их на пинованном pnpm.

## Question

Создать скелет фронтенда ровно в том виде, который выбран в тикетах 09, 10 и 13:

- `pnpm-workspace.yaml`, корневой `package.json` с точными версиями и скриптами.
- `apps/web` — приложение Next.js с App Router, пустая страница, `lang` из маршрута.
- `packages/ui`, `packages/api-client`, `packages/shared`, `packages/eslint-config`,
  `packages/tsconfig` — по правилам проекта.
- ESLint flat config + typescript-eslint, Prettier, `tsc --noEmit`, правило на границы слоёв FSD.
- Git-хук на изменённые файлы (lint + типы).
- Workflow CI для фронтенда: install → lint → typecheck → build, с пинованным pnpm.

Критерий готовности: `pnpm lint`, `pnpm typecheck`, `pnpm build` зелёные на пустом приложении,
без `any`, без `@ts-expect-error`, без barrel-файлов.

## Answer

**Тикет выполнен:** монорепа `frontend/` создана, `pnpm lint`, `pnpm typecheck` и `pnpm build`
зелёные на пустом приложении, ветка запушена, PR открыт в `develop`. Решение тикета 13 взято из
`develop` (PR #14 влит, `b10b2f4`), а не из его ветки.

### Что сделано

- `frontend/pnpm-workspace.yaml` — `packages: apps/*, packages/*`, `catalog:` со всеми точными
  версиями, `allowBuilds` для pnpm 12 и `peerDependencyRules` для `eslint-plugin-import`.
- `frontend/package.json` — скрипты `lint`, `typecheck`, `build`, `dev`, `format`, `format:check`,
  `prepare`; `devEngines.packageManager` = `pnpm@12.9.1` (`onFail: download`) и `engines.node >= 24`.
  Внутренние зависимости — `workspace:*`; версии — только `"catalog:"`.
- `apps/web` — Next.js 16.4.0 App Router. Маршруты в `src/app`, FSD-слои в
  `src/{_pages,widgets,features,entities,shared}`, отдельного слоя `app` нет.
  `src/app/[lang]/layout.tsx` — корневой layout с `<html lang={lang} suppressHydrationWarning>`;
  `src/app/providers.tsx` — `next-themes` с `attribute="class"`, `defaultTheme="system"`,
  `enableSystem`, `disableTransitionOnChange`; `src/app/globals.css` — только
  `@import "tailwindcss"` и `@import "@mysite/ui/styles/globals.css"`; `/` редиректит на `/ru`.
  `_pages/home` — пустая страница, route-файл `[lang]/page.tsx` — реэкспорт её публичного API.
- `packages/ui` — дизайн-система по 13: `src/index.ts` с явными named-экспортами (`Button`,
  `buttonVariants`), `src/styles/globals.css` (`@theme`, `@theme inline`, `:root`/`.dark`,
  `@custom-variant dark`, `@source` на `packages/ui` и `apps/web/src`), `src/lib/utils.ts` (`cn`),
  `components.json` с `#`-алиасами, `exports` только `"."` и `"./styles/globals.css"`, `imports`
  для `#components/#lib/#hooks`. `Button` добавлен `shadcn add button` в пакет и затем явно
  экспортирован из `index.ts`.
- `packages/api-client` и `packages/shared` — скелеты с явным `export {}`; сгенерированный клиент не
  создавался и никуда не подключён (11/15).
- `packages/eslint-config` — общий flat config: `@eslint/js` + `typescript-eslint` с типизированным
  линтом (`parserOptions.projectService: true`), `eslint-plugin-boundaries` с политикой
  `boundaries/dependencies` (элементы `route`, `pages`, `widgets`, `features`, `entities`, `shared`,
  `ui-package`, `api-client`, `shared-package`; `partialMatch: false`; внутренние зависимости
  разрешены), `import/no-restricted-paths` как второй пояс, `no-restricted-imports` на
  `@mysite/ui/*` и `eslint-config-prettier/flat`.
- `packages/tsconfig` — `base.json`, `next.json`, `react-library.json`.
- `frontend/lefthook.yml` + `frontend/.githooks/pre-commit` — lint по staged-файлам и
  `tsc --noEmit` по проекту при staged `.ts`/`.tsx`, без `--cache`.
- `.github/workflows/frontend.yml` — пинованный pnpm 12.9.1 и Node 24.18.0:
  `pnpm install --frozen-lockfile` → `pnpm lint` → `pnpm typecheck` → `pnpm build`.

### Версии

Пины 09/10 сохранены: `next` 16.4.0, `react`/`react-dom` 19.3.0, `typescript` 6.0.3,
`typescript-eslint` 8.71.1, `eslint` 10.12.0, `prettier` 3.9.9, `tailwindcss` и
`@tailwindcss/postcss` 4.3.3, `shadcn` 4.21.3, `next-themes` 0.4.6, `pnpm` 12.9.1, `lefthook`
2.1.17.

Новые пины (точный текущий stable из реестра на 2026-11-16, если не сказано иное):

| Пакет | Версия | Зачем |
|---|---|---|
| `eslint-plugin-boundaries` | **7.4.4** | Замена пина 7.2.0 из 10 — см. ниже |
| `@eslint/js` | 10.0.1 | `eslint:recommended` для flat config |
| `eslint-config-prettier` | 10.1.8 | Отключает правила, принадлежащие Prettier (`/flat`) |
| `eslint-import-resolver-typescript` | 4.4.5 | Резолвит `@/*` и workspace-пакеты для boundaries и `import/*` |
| `eslint-plugin-import` | 2.32.0 | Даёт `import/no-restricted-paths` (10) |
| `globals` | 17.14.0 | Браузерные и Node-глобалы для ESLint |
| `@types/node` | **24.19.2** | Текущий stable линии 24; npm `latest` — 26.6.6, а проект объявляет Node 24 в `engines` и CI |
| `@types/react` | 19.3.0 | Совпадает с `react` 19.3.0 |
| `@types/react-dom` | 19.3.0 | Совпадает с `react-dom` 19.3.0 |
| `clsx` | 2.1.1 | `cn` |
| `tailwind-merge` | 3.7.0 | `cn` |
| `class-variance-authority` | 0.7.1 | Варианты `Button` |
| `@radix-ui/react-slot` | 1.5.0 | `Button asChild` |

**Почему `eslint-plugin-boundaries` 7.4.4, а не 7.2.0.** Пин 7.2.0 в ответе 10 — снимок на
2026-08-09; текущий stable того же мажора — 7.4.4 (peer `eslint >=6.0.0`, `engines.node >=18.18`).
7.2.0 против ESLint 10.12.0 не проверялся и текущим stable не является; 7.4.4 прошёл все гейты и
проверен на реальном нарушении границы. В тикете 10 был указан 7.2.0, в задании 14 — 7.4.1;
реестр сейчас отдаёт 7.4.4, поэтому записан он.

**`eslint-plugin-import` 2.32.0 против ESLint 10.** Его peer-диапазон заканчивается на
`^9`, но нужное правило `import/no-restricted-paths` работает на 10 (проверено). Чтобы установка
не спорила с peer-диапазоном, в `pnpm-workspace.yaml` стоит
`peerDependencyRules.allowedVersions: { "eslint-plugin-import>eslint": "10" }`. Это не отключение
правила, а разрешение устаревшего peer-объявления.

### Отступления и находки

1. **lefthook ищет конфиг в корне Git, а не в `frontend/`.** Первый `pnpm install` создал
   `lefthook.yml` в корне репозитория — вне разрешённого списка файлов задачи. Поэтому конфиг
   остался в `frontend/lefthook.yml`, а `frontend/scripts/setup-git-hooks.mjs` (из `prepare`)
   ставит `core.hooksPath=frontend/.githooks`; закоммиченный `frontend/.githooks/pre-commit`
   передаёт `LEFTHOOK_CONFIG` и запускает lefthook. Хук проверен настоящим коммитом. Если владелец
   предпочтёт стандартную схему, достаточно перенести `lefthook.yml` в корень репозитория и
   запустить `lefthook install`.
2. **pnpm 12 переименовал `onlyBuiltDependencies` в `allowBuilds`.** Первый install упал с
   `ERR_PNPM_IGNORED_BUILDS: lefthook@2.1.17`; lefthook'у запрещён postinstall
   (`allowBuilds: { lefthook: false }`), потому что хук ставит скрипт из п. 1.
3. **pnpm 12 сам дописал `minimumReleaseAgeExclude`** для четырёх пакетов, опубликованных внутри
   окна release-age (`@boundaries/*`, `eslint-plugin-boundaries`). Оставлено: иначе следующий
   install добавит блок заново; lockfile держит точные версии.
4. **`shadcn add button` 4.21.3 с `#`-алиасами сгенерировал неверные импорты:**
   `import { cn } from "cn"` и `import { Slot } from "radix-ui"`, добавив в `package.json` пакеты
   `cn` и `radix-ui` с диапазонами. Исправлено на `#lib/utils` и `@radix-ui/react-slot`
   (`catalog:`), лишние зависимости удалены. Это дефект CLI, а не решение; при следующем
   `shadcn add` проверять импорты обязательно.
5. **Корневой layout — `app/[lang]/layout.tsx`, а не `app/layout.tsx` + `app/page.tsx`.** `<html
   lang>` должен знать локаль, а layout вне `[lang]` её не получает. `/` → `/ru` сделан
   `redirects()` в `next.config.ts` (307). Окончательный статус и поведение `/` — тикет 12; здесь
   рабочая заглушка. `app/page.tsx` из эскиза 13 по этой же причине не создан.
6. **`next-env.d.ts` ссылается на сгенерированные `.next/types/*`.** Поэтому `typecheck` в
   `apps/web` — `next typegen && tsc --noEmit`, а `next-env.d.ts` в `.gitignore`: иначе `tsc` на
   свежем клоне падал бы до первого build.
7. **`eslint-import-resolver-typescript` предупреждал «Multiple projects found».** Добавлено
   `noWarnOnMultipleProjects: true`: это отключение рекомендации резолвера, а не правила линтера —
   в монорепе по определению tsconfig на каждый пакет.
8. **`eslint-config-next` не добавлен.** Тикет требует ESLint + typescript-eslint; Next-плагин
   тянет peer-предупреждения на ESLint 10. Место для него — отдельный тикет, когда появится
   React-код.

### Подтверждение (дословно)

`pnpm install --frozen-lockfile` (из `frontend/`):

```
Scope: all 7 workspace projects
✓ Lockfile passes supply-chain policies (verified 2m ago)
Lockfile is up to date, resolution step is skipped
Done in 37ms using pnpm v12.9.1
EXIT=0
```

`pnpm lint`:

```
$ eslint .
EXIT_CODE=0
```

Ни одного замечания и ни одного предупреждения.

`pnpm typecheck`:

```
$ pnpm --recursive --if-present run typecheck
Scope: 6 of 7 workspace projects
packages/ui typecheck$ tsc --noEmit
packages/api-client typecheck$ tsc --noEmit
packages/shared typecheck$ tsc --noEmit
packages/api-client typecheck: Done
packages/shared typecheck: Done
packages/ui typecheck: Done
apps/web typecheck$ next typegen && tsc --noEmit
apps/web typecheck: Generating route types...
apps/web typecheck: ✓ Types generated successfully
apps/web typecheck: Done
EXIT_CODE=0
```

`pnpm build`:

```
▲ Next.js 16.4.0 (Turbopack)
✓ Running next.config.ts took 24ms

  Creating an optimized production build ...
✓ Compiled successfully in 346ms
  Running TypeScript ...
  Finished TypeScript in 1042ms ...
  Collecting page data using 4 workers ...
  Generating static pages using 4 workers (0/2) ...
✓ Generating static pages using 4 workers (2/2) in 375ms
  Finalizing page optimization ...

Route (app)
┌ ○ /_not-found
└ ƒ /[lang]

○  (Static)   prerendered as static content
ƒ  (Dynamic)  server-rendered on demand

EXIT_CODE=0
```

**`@source` проверен сборкой, а не на глаз.** В собранном
`apps/web/.next/static/chunks/25_-vlbozwfjg.css` найдены классы из `packages/ui` (`bg-primary`,
`focus-visible\:ring-ring`), классы из `apps/web` (`min-h-svh`, `text-3xl`) и токены
(`--color-paper`, `.dark`).

**`exports` проверены.** Временный файл с `import { Button } from "@mysite/ui"` прошёл
`tsc --noEmit`. Временный файл с `import { Button } from "@mysite/ui/components/ui/button"` упал
и в линтере, и в `tsc`:

```
1:1  error  '@mysite/ui/components/ui/button' import is restricted ... no-restricted-imports
src/shared/deep.ts(1,24): error TS2307: Cannot find module '@mysite/ui/components/ui/button'
```

**Границы FSD проверены на реальном нарушении.** Временный импорт `shared → features` дал:

```
2:23  error  Unexpected path "@/features/probe" imported in restricted zone  import/no-restricted-paths
2:23  error  FSD boundary violated: shared may not import features           boundaries/dependencies
4:12  error  Unexpected any. Specify a different type                        @typescript-eslint/no-explicit-any
```

Временные файлы удалены; в коммит они не попали. `any`, `@ts-expect-error` и `export *` в коде нет
(проверено поиском; единственное совпадение — слово `export *` в комментарии `index.ts`).

**Маршруты проверены на `next start -p 3100`:** `/` → `307`, `Location: /ru`; `/ru` отдаёт
`<html lang="ru">`; `/en` — `<html lang="en">`; `/de` → `404`.

**Git-хук проверен настоящим коммитом:** lefthook выполнил lint по staged-файлам и typecheck по
проекту, `✓ lint`, `✓ typecheck`, коммит создан.

### Файлы, ветка, коммиты

Создано 50 файлов. Группами:

- `frontend/pnpm-workspace.yaml`, `pnpm-lock.yaml`, `package.json`, `eslint.config.mjs`,
  `lefthook.yml`, `.gitattributes`, `.gitignore`, `.prettierignore`, `.prettierrc.json`,
  `README.md`, `scripts/setup-git-hooks.mjs`, `.githooks/pre-commit`.
- `frontend/apps/web/**` — приложение и FSD-слои.
- `frontend/packages/{ui,api-client,shared,eslint-config,tsconfig}/**`.
- `.github/workflows/frontend.yml`.
- Изменены: `WORKFLOW.md` (раздел Frontend и Definition of done), `ARCHITECTURE.md` (стек,
  раскладка, раздел Frontend, «What is deliberately absent»), `frontend/README.md` (больше не
  «reserved»), файл этого тикета.
- Удалений нет.

Ветка `feature/frontend-scaffold-and-gates` от `develop` `b10b2f49aea7571ecc5bca6eb72c71096d7b927c`
(после `git fetch origin`, `git switch develop`, `git merge --ff-only origin/develop`; хеши
`develop` и `origin/develop` совпали). Коммиты: `6d7bb34856e8fea86b692f0cd7c75ae19692162d`
(скаффолд, гейты, CI, документация) и docs-коммит, закрывающий этот тикет, — в той же ветке.
PR в `develop`: https://github.com/IKalentsov/MySite_dev/pull/15. CI фронтенда
(`.github/workflows/frontend.yml`): https://github.com/IKalentsov/MySite_dev/actions/runs/43776656354 —
install, lint, typecheck и build зелёные.

### За рамками и вопросы владельцу

- **`.dsh/AGENTS.md` не трогался** (правит планер): в «Project facts» фронтенд всё ещё «reserved»,
  в таблице слоёв остались FSD `app/` и `pages/`, открытые вопросы 13 не убраны. Это теперь
  расходится с репозиторием.
- **Журнал отклонений 2026-10-07** (фронтенд-гейты не записаны) не правился, как и в тикете 06;
  строка устарела и её закрывает планер.
- **Тикет 12 открыт**: окончательный статус редиректа `/`, переключатель языка, metadata и OG —
  его решения; здесь только рабочая заглушка `/` → `/ru` (307).
- **Тикеты 11/15** — генерация API-клиента; `packages/api-client` — скелет.
- **Тикеты 16/18** — страница и граница данных; `_pages/home` пустая.
- **Тестовый раннер не добавлялся** (Vitest/RTL/MSW/Playwright) — по условию 14; место — 15/16.
- **`eslint-config-next`** — отдельное решение, когда появится React-код (см. п. 8 выше).
- **Backend не трогался.** В рабочем дереве остался чужой незакоммиченный
  `backend/tests/MySite.ArchitectureTests/MySite.ArchitectureTests.csproj` (удалён
  `ProjectReference` на негативную фикстуру) — он не мой и в коммит не попал.
