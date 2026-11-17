# 16 — Страница целиком

Type: task
Status: resolved
Blocked by: 03, 08, 12, 13, 14, 15, 18

## Как работать с этой задачей

**Правила работы с проектом — `.dsh/AGENT-TASKS.md`.** Прочитай его первым, до первой правки: там
команды, правила веток, среда и критерий готовности. Ниже — только специфика этой задачи.

- Ветка: `feature/the-page-itself` от свежего `develop`; push ветки и PR в `develop` — работа
  исполнителя, слияние — владельца.
- Критерий готовности: гейты фронтенда и тесты (Vitest/Playwright из тикета 20) зелёные, все четыре
  состояния экрана реализованы и проверены, проверки скиллами из раздела «Приёмка» пройдены и их
  вывод приложен к отчёту.
- Для сквозной проверки нужны поднятая база, накатанные миграции и залитые заглушки (тикет 08):
  `docker compose up -d`, `pg_isready`, `dotnet ef migrations list` без `(Pending)`.

**Условие остановки:** Публичная страница собрана и открывается на `ru` и `en`: все четыре состояния реализованы и проверены, прогоны скиллов из раздела «Приёмка» приложены, гейты фронтенда зелёные.

## Question

Собрать публичную страницу: блок «обо мне», список проектов, контакт-заглушка. Языков две, локаль
берётся из URL.

Обязательные состояния (правило проекта — все четыре, даже если «пустого не бывает»):

| Состояние | Что показываем |
|---|---|
| Loading | Скелет на месте контента, без сдвига вёрстки |
| Empty | «Пока ничего нет» плюс что изменится; не белый экран |
| Error | Понятный текст и кнопка повтора; техническая подробность — в лог |
| Success | Данные; частичный результат помечается отдельно |

Сквозные случаи: проект без английского перевода (fallback на русский по ADR-0005), проект снят с
публикации (не показывается), API недоступен (error + retry), очень длинное описание.

Требования: WCAG 2.2 AA, тёмная тема, адаптивность; состояние, которое должно шариться ссылкой,
воспроизводится из URL.

Приёмка: гейты фронтенда из `WORKFLOW.md`; проверка скиллами `better-accessibility`,
`better-layout`, `better-typography`, `better-colors`, `better-ui`, `better-writing` и сводный
разбор `better-interface`; прогон `architecture-drift-check` по итогам изменения структуры.

## Уточнения после исследования 11 (2026-11-16)

- **Источник данных.** `getHomeContent` из 15 вызывает публичные функции `@mysite/api-client`;
  страница не знает ни про `openapi-fetch`, ни про URL. Если 15 выбирает составной
  `GET /api/v1/home`, `HomeContent` — это `{ profile: OwnerProfileResponse | null, projects:
  PaginatedProjectsResponse }`; частичное состояние — «профиль пуст, проекты есть» (`profile:
  null`), полный отказ — исключение и `error.tsx`. Если 15 выбирает два запроса, форма —
  `{ profile, projects, failed }` по 18; выбор фиксируется в 15, страница подстраивается под него.
- **Границы не двигаются.** `home-page.tsx` — тонкий async Server Component, `HomePageView` —
  синхронный на пропсах; `cache: "no-store"`, без query-библиотеки, без `generateStaticParams`.
- **Состояния.** Loading — `app/[lang]/loading.tsx`; Error — `app/[lang]/error.tsx` с retry;
  Empty — `profile: null` и/или пустой `projects`; Success — данные; Partial — то, что пришло,
  с пометкой. `profile: null` — не ошибка (03), поэтому это empty/partial, а не error.
- **Тесты.** Синхронный view — Vitest + RTL (тикет 20), данные — фикстуры-пропсы; серверный путь
  данных — Playwright против `next start` и локального API (20). `async` RSC Vitest'ом не
  тестируется (18).

## Answer

**Что сделано.** Публичная страница собрана целиком: `/[lang]` с шапкой и переключателем языка,
блоком «Обо мне», списком проектов и контактами. Четыре состояния реализованы и покрыты тестами;
данные приходят с живого API (`GET /api/v1/home`), в тексте — заглушки тикета 08.

- **Композиция (FSD).** `_pages/home/ui/home-page.tsx` — тонкий async Server Component: берёт локаль,
  проверяет её через `shared/config/locales.ts` (неподдерживаемая — `notFound()`), зовёт
  `getHomeContent` и рендерит синхронный `home-page-view.tsx` на пропсах. Ниже — виджеты
  `site-header`, `profile-about`, `project-list`, `contact-links`, фича `locale-switch` и сущность
  `project` с карточкой; у каждого слайса свой `index.ts`. В `shared/` лежит то, что читают все:
  локали, интерфейсные тексты (`shared/i18n/home-text.ts`, RU/EN) и контактная заглушка
  (`shared/config/contact-links.ts`).
- **Данные.** Единственная точка — `getHomeContent` (`_pages/home/api/get-home-content.ts`) поверх
  `@mysite/api-client`; `OwnerProfile` и `Project` берутся из сгенерированного клиента и
  реэкспортируются оттуда. Слои ниже `pages` клиент не импортируют: карточка принимает структурные
  пропсы, и компилятор сверяет их с `Project` в точке вызова — контракт остаётся один.
- **Состояния.** Loading — `app/[lang]/loading.tsx`: скелет тех же ширин и высот, `aria-busy`,
  декоративные блоки `aria-hidden`. Error — `app/[lang]/error.tsx` (`"use client"`): понятный текст,
  кнопка «Повторить» → `reset`, ошибка уходит в `console.error`, техническая подробность на экран не
  попадает. Empty — «Пока ничего нет» с подсказкой вместо секций «Обо мне» и «Проекты», шапка и
  контакты остаются. Success — профиль и проекты. Partial — пришедшее показывается, отсутствующая
  часть заменяется своей заметкой, в начале `main` — «Страница загружена не полностью.».
- **Доступность и вёрстка.** Только токены `packages/ui`; ссылки всегда подчёркнуты, поэтому
  различаются без цвета; фокус — видимое кольцо `ring-2 ring-foreground` на каждой интерактивной
  цели; цели не меньше 24×24 CSS px; один `h1`, разделы `h2`, проекты `h3`; длинная строка ограничена
  `65ch`, длинные слова переносятся; тёмная тема — смена значений токенов, следует системной
  настройке (`next-themes`, `defaultTheme="system"`).
- **Тесты.** Vitest + RTL: `home-page-view.test.tsx` (success, empty, оба partial, переключатель,
  длинное описание), `error-boundary.test.tsx` (текст и retry, лог, отсутствие утечки технического
  текста, английский вариант), `loading.test.tsx` (aria-busy и скелет). Playwright:
  `e2e/page.spec.ts` против живого API — профиль и три проекта, скрытый Keycloak_Learn, русский
  fallback Harness на `/en`, переключение локали, отсутствие горизонтальной прокрутки на 320 и
  360 px, тёмная и светлая тема.

**Чем подтверждено** (из `frontend/`):

```text
$ pnpm lint
$ eslint .
EXIT=0

$ pnpm typecheck
6 of 7 workspace projects, apps/web: next typegen + tsc --noEmit
EXIT=0

$ pnpm --filter @mysite/web test
 ✓ src/_pages/home/api/get-home.test.ts (2 tests)
 ✓ src/__tests__/loading.test.tsx (1 test)
 ✓ src/__tests__/button.test.tsx (3 tests)
 ✓ src/__tests__/error-boundary.test.tsx (4 tests)
 ✓ src/_pages/home/ui/home-page-view.test.tsx (9 tests)

 Test Files  5 passed (5)
      Tests  19 passed (19)
EXIT=0

$ pnpm build
✓ Compiled successfully / Finished TypeScript / 2 static pages
Route (app)  ┌ ○ /_not-found  └ ƒ /[lang]
EXIT=0

$ pnpm --filter @mysite/web test:e2e
Running 9 tests using 6 workers
  9 passed (3.2s)
EXIT=0
```

**CI.** Первый прогон CI по PR поймал реальную вещь: в workflow фронтенда нет ни базы, ни API,
поэтому три спеки, читающие данные, падали (страница в CI честно показывала error-состояние).
По решению тикета 20 CI гоняет путь без стека, поэтому эти три спеки пропускаются при `CI`
(`skipWithoutLocalStack` в `e2e/page.spec.ts`), а smoke, адаптивность и тема идут всегда.
Проверено локально: с живым API — 9 из 9, с `CI=true` — 6 прошли, 3 пропущены, код 0. Условие
пропуска записано в `WORKFLOW.md`, раздел «End-to-end tests».

Среда сквозного прогона:

```text
docker compose ps      → mysite_postgres … Up (0.0.0.0:5433->5432/tcp)
pg_isready             → /var/run/postgresql:5432 - accepting connections
dotnet ef migrations list → 20261006231005_ContentSchema   (Pending нет)
dotnet run … seed      → Profile: unchanged. Projects: created 0, updated 0, unchanged 4.
                         Translation gaps (deliberate): https://github.com/IKalentsov/Harness (en)
GET /api/v1/home?locale=en → profile.locale=en, 3 проекта, Harness с русским заголовком
```

Пустое и частичное состояния проверены на временном стабе API (`profile: null`, пустой `projects`):
страница показала «Пока ничего нет» и, во втором прогоне, «Страница загружена не полностью.» с
заметкой о профиле и карточкой проекта; стаб после проверки удалён, в коммиты не попал.

Error проверен в браузере на `next start` с `API_BASE_URL=http://127.0.0.1:5999`: `<h1>Не удалось
загрузить страницу`, текст «Проверьте соединение и попробуйте ещё раз.», кнопка «Повторить»; утечек
технического текста (`ECONNREFUSED`, `fetch failed`, стек) в DOM нет; на `/en` — «The page could not
be loaded» и «Try again». В серверном логе — `TypeError: fetch failed … ECONNREFUSED 127.0.0.1:5999`,
то есть подробность ушла в лог.

Контраст измерен в браузере (значения ok/oklch нормализованы через canvas). Светлая тема: основной
текст 19.8:1, muted 4.74:1, чипы 16.4:1, ссылки 17.9:1. Тёмная: 18.97:1, 7.66:1, 14.5:1, 15.7:1.
Все пары не ниже 4.5:1 (WCAG AA).

**Приёмка скиллами — что нашлось и что исправлено.** Прогнаны `better-accessibility`,
`better-layout`, `better-typography`, `better-colors`, `better-ui`, `better-writing`, сводный
`better-interface` и `architecture-drift-check`. До исправлений:

1. ссылки в тексте отличались от обычного текста только цветом (а `text-primary` почти совпадает с
   основным) — добавлено постоянное подчёркивание;
2. у ссылки-названия сайта в `h1` не было видимого индикатора фокуса — добавлено кольцо, как у
   остальных ссылок;
3. у ссылок переключателя языка и карточки не было цели 24×24 px — добавлен `min-h-6`;
4. `h1` был 24 px при `h2` 20 px — поднят до 30 px, чтобы уровни читались;
5. строка «обо мне» была длиннее 65 символов — ограничена `65ch`;
6. переносы заголовков и описаний — добавлены `text-balance`/`text-pretty`.

Фокус проверен обходом Tab по восьми целям: у каждой видимое кольцо. `architecture-drift-check`:
`ARCHITECTURE.md` приведён в соответствие — описан состав экрана и слайсов, обновлены раздел о
тестах (контур установлен), врата фронтенда и раздел «What is deliberately absent» (локализация и
первый экран больше не «не решены»).

**Файлы.**

Созданы: `frontend/apps/web/src/shared/config/locales.ts`, `shared/config/contact-links.ts`,
`shared/i18n/home-text.ts`, `entities/project/index.ts`, `entities/project/ui/project-card.tsx`,
`features/locale-switch/{index.ts,ui/locale-switch.tsx}`,
`widgets/site-header/{index.ts,ui/site-header.tsx}`, `widgets/profile-about/…`,
`widgets/project-list/…`, `widgets/contact-links/…`, `_pages/home/ui/home-page-view.tsx`,
`_pages/home/ui/home-page-view.test.tsx`, `_pages/home/ui/testing/home-content.ts`,
`app/[lang]/loading.tsx`, `app/[lang]/error.tsx`, `src/__tests__/error-boundary.test.tsx`,
`src/__tests__/loading.test.tsx`, `e2e/page.spec.ts`.

Изменены: `_pages/home/api/get-home-content.ts` (типы — из сгенерированного клиента),
`_pages/home/index.ts`, `_pages/home/ui/home-page.tsx` (async RSC с проверкой локали),
`app/[lang]/layout.tsx` (локали из `shared`), `ARCHITECTURE.md`, `WORKFLOW.md` (команды `test` и
`test:e2e`, предпосылки e2e, Definition of done).

Удалены: `.gitkeep` в `entities/`, `features/`, `widgets/`, `shared/` — слои заполнены.

**Ветка и коммиты.** `feature/the-page-itself` от `develop` `80415a839ac74d75748b6d8ae33934fd9ebb591e`
(хеши локального и `origin/develop` совпали; в истории есть PR #21). Коммиты: `911ad22` (журнал
прогона плагина), `e26adc9` (страница, тесты, документы), `c792039` (закрытие тикета). PR в
`develop`: https://github.com/IKalentsov/MySite_dev/pull/22.

**Вопросы владельцу.**

1. **Имени владельца нет в контракте профиля.** `OwnerProfileResponse` — это `locale`, `headline`,
   `about`; поэтому `h1` — название сайта, а не имя. Если имя нужно на странице, его надо добавить в
   профиль (контракт и таблицы) — это отдельное изменение.
2. **Контакты — заглушка в коде** (`shared/config/contact-links.ts`: `t.me/placeholder`,
   `mailto:placeholder@example.com`): контактов нет ни в API, ни в seed. Значения заменит владелец
   вместе с текстами тикета 07.
3. **Тёмная тема следует системной настройке**; переключателя темы на странице нет — если он нужен,
   это отдельная задача (провайдер `next-themes` уже готов).
4. **Сквозной e2e требует живого стека** (база + seed + API на `:5054` + собранный `next start`).
   Это записано в `WORKFLOW.md`; в CI e2e поднимает только приложение, поэтому путь данных проверяется
   локально, как и просил тикет.
5. **Тексты — заглушки тикета 08**, включая намеренно отсутствующий английский перевод у Harness:
   после тикета 07 длину строк и переносы стоит перепроверить на реальных текстах.
