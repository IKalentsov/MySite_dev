# Журнал прогона плагина Architect + Qwen — тикет 16 «Страница целиком»

Время — Europe/Moscow, 2026-11-16. Ведёт архитектор (`lead`), сессия открыта специально для
отладки плагина.

## 1. Точный текст задачи, переданный в `subagent`

```text
Workspace: H:\CSharp\MySite\MySite_dev (абсолютный путь). Не читай и не пиши ничего за его пределами.

Goal: Реализовать тикет 16 — публичную страницу MySite на Next.js App Router: блок «обо мне», список проектов, контактная заглушка, две локали из URL (`/ru`, `/en`), все четыре обязательных состояния экрана (loading, empty, error, success плюс частичный результат), WCAG 2.2 AA, тёмная тема и адаптивность — с тестами Vitest/RTL на синхронный view и Playwright на серверный путь данных. Ветку уже создал архитектор, рабочее дерево чистое. Git — не твой: ни одной git-команды.

Read first (в этом порядке):
- `.scratch/site-development/issues/16-the-page-itself.md` — сама задача, включая раздел «Уточнения после исследования 11»: это и есть задание, дублировать его не нужно.
- `.dsh/AGENT-TASKS.md` — стоячий контракт: команды, границы, критерий готовности, §13 «как закрыть файл задачи».
- `WORKFLOW.md` — разделы Frontend и Definition of done.
- `.scratch/site-development/map.md` — «Decisions so far»: записи 03, 12, 13, 14, 15, 18, 20.
- Уже существующий код: `frontend/apps/web/src/app/[lang]/layout.tsx`, `frontend/apps/web/src/app/[lang]/page.tsx`, `frontend/apps/web/src/_pages/home/**`, `frontend/packages/api-client/src/index.ts`, `frontend/packages/ui/src/index.ts`, `frontend/packages/ui/src/styles/globals.css`, `frontend/packages/eslint-config/src/index.mjs` (границы FSD и `no-restricted-imports` — их нельзя нарушать), `frontend/apps/web/e2e/home.spec.ts`, `frontend/apps/web/playwright.config.ts`, `frontend/apps/web/vitest.config.mts`.

Skills (загрузи инструментом `skill` все, до первой правки):
- `qwen-executor` — твой контракт исполнителя: рамки, отчёт, запреты.
- `vercel-react-best-practices` — страница живёт на Server Components: важно, что исполняется на сервере, что уезжает в клиентский бандл и как компоновать данные.
- `better-accessibility` — WCAG 2.2 AA в проекте жёсткое требование, а не пожелание: семантика, фокус, подписи, размеры целей.
- `better-layout` — это композиция одного экрана: группировка, порядок чтения, иерархия заголовков.
- `tdd` — четыре состояния и есть критерий приёмки: сначала падающий тест на состояние, потом реализация.

Почему именно эти четыре: задача одновременно про RSC-композицию, про доступность и раскладку одного экрана, и про состояния, которые проверяются тестами. `better-*` для типографики, цвета, текста и сводный разбор оставлены архитектору на приёмку — тебе не нужны.

Commands (все из `frontend/`; ставить ничего не нужно, `pnpm install` не запускай):
```
pnpm lint
pnpm typecheck
pnpm --filter @mysite/web test
pnpm build
pnpm --filter @mysite/web test:e2e
```
Среда уже поднята архитектором: PostgreSQL в Docker, миграции накатаны, заглушки засеяны (тикет 08), локальный API работает. Проверь: `Invoke-RestMethod 'http://localhost:5054/api/v1/home?locale=en'` → `profile.locale = en`, 3 проекта, у Harness русский заголовок (fallback по ADR-0005), Keycloak_Learn отсутствует. Docker, миграции, seed и API не запускай и не останавливай — они не твои. Если API недоступен: сделай всё, кроме e2e, и напиши об этом в отчёте.

Change — решения приняты, не пересматривай их; если считаешь решение неверным, остановись и задай вопрос в отчёте.

1. Локали и словарь.
- `frontend/apps/web/src/shared/config/locales.ts`: `SUPPORTED_LOCALES = ["ru", "en"] as const`, `type Locale`, `DEFAULT_LOCALE = "ru"`, `isLocale(value: string): value is Locale`, `otherLocale(locale: Locale): Locale`.
- `frontend/apps/web/src/shared/i18n/home-text.ts`: `getHomeText(locale: Locale)` возвращает типизированный объект строк интерфейса RU/EN (RU первичен): заголовки разделов, пустое состояние и подсказка, отсутствующий профиль, отсутствующие проекты, метка частичной загрузки, заголовок/текст/кнопка ошибки, «загрузка», подписи ссылок контактов и полные названия языков. Контент (профиль, проекты) уже приходит из API в нужной локали — его не переводи.
- `layout.tsx`: замени локальный список локалей на импорт из `shared/config/locales.ts` (app → shared разрешено). Метаданные и `notFound()` не меняй.

2. Роут.
- `app/[lang]/page.tsx` остаётся однострочным реэкспортом `HomePage` как default.
- `HomePage` принимает `{ params }: { params: Promise<{ lang: string }> }`, берёт локаль, проверяет её через `isLocale` и для неподдерживаемой вызывает `notFound()` (иначе `/de` уйдёт в error boundary с 400 от API). Затем `const content = await getHomeContent(lang)` и рендерит `HomePageView`.
- `app/[lang]/loading.tsx` — скелет на месте контента: те же ширины колонок и высоты блоков (без сдвига вёрстки), контейнер с `aria-busy="true"`, сами скелет-блоки `aria-hidden="true"`. Без единого «пустого белого экрана».
- `app/[lang]/error.tsx` — клиентский error boundary (`"use client"`): понятный пользователю текст и кнопка «Повторить», вызывающая `reset`. Техническую подробность не показывай; `error` залогируй один раз через `useEffect` + `console.error` (иначе `error` окажется неиспользованным и линт упадёт). Кнопку бери из `@mysite/ui` (`Button`), не пиши свою.

3. Композиция (FSD, слои только вниз, слайсы одного слоя друг друга не импортируют):
- `shared/config/contact-links.ts` — `CONTACT_LINKS = { telegram: "https://t.me/placeholder", email: "mailto:placeholder@example.com" }` с коротким комментарием, что это заглушка и значения заменятся вместе с текстами тикета 07.
- `entities/project/ui/project-card.tsx` + `entities/project/index.ts` — `ProjectCard({ project, locale })`: `<article>` с `<h3>` названием, описанием, стеком (список/чипы), годом и ссылкой. Ссылка имеет осмысленный текст, а не «здесь»; внешняя — `rel="noreferrer noopener"`, `target="_blank"`.
- `features/locale-switch/ui/locale-switch.tsx` + `index.ts` — `<nav aria-label>` с двумя ссылками на `/ru` и `/en`; активная — `aria-current="page"`; у ссылок `lang="ru"`/`lang="en"`, видимая подпись «RU»/«EN», доступное имя — полное название языка из словаря.
- `widgets/site-header/{ui/site-header.tsx,index.ts}` — `<header>` с `<h1>MySite</h1>` (ссылка на `/{lang}`) и переключателем локали.
- `widgets/profile-about/{ui/profile-about.tsx,index.ts}` — `<section aria-labelledby>` с `<h2>`, headline и about; если профиля нет — заметка «текст "обо мне" пока не заполнен» вместо текста.
- `widgets/project-list/{ui/project-list.tsx,index.ts}` — `<section aria-labelledby>` с `<h2>` и `<ul>`/`<li>` из `ProjectCard`; если проектов нет — заметка «проектов пока нет».
- `widgets/contact-links/{ui/contact-links.tsx,index.ts}` — `<section aria-labelledby>` с `<h2>` и двумя ссылками (Telegram, почта) из `CONTACT_LINKS`; цели не меньше 24×24 CSS px, фокус видимый.
- `_pages/home/ui/home-page-view.tsx` — синхронный, на пропсах `{ lang: Locale; content: HomeContent }`, только данные и разметка: header → main (profile-about, project-list) → contact-links.
- `_pages/home/ui/home-page.tsx` — тонкий async Server Component (см. п. 2).
- `_pages/home/index.ts` — обнови экспорты (HomePage, HomePageView, getHomeContent, HomeContent). `_pages/home/api/get-home-content.ts` не меняй; типы бери из `@mysite/api-client`.
- Никаких `export *`, только именованные экспорты; `packages/ui` не трогай (Button уже есть).

4. Состояния (единственное прочтение, которое принимается):
- профиль есть и проекты есть → success;
- профиля нет и проектов нет → empty: вместо секций «Обо мне» и «Проекты» один блок «Пока ничего нет» + подсказка, что здесь появится; шапка и контакты остаются;
- ровно одной части нет → partial: пришедшее показывается, отсутствующая часть заменяется своей заметкой, а в начале `main` видна метка «Страница загружена не полностью» (`<p role="note">`). Частичный результат — не ошибка;
- запрос бросил исключение (`ApiError` и любое другое) → `app/[lang]/error.tsx` с повтором. Внутри view никаких try/catch.

5. Требования к вёрстке и доступности.
- Только токены и утилиты Tailwind из `packages/ui/src/styles/globals.css` (`bg-background`, `text-foreground`, `text-muted-foreground`, `border-border` и т.д.); никаких жёстких цветов, инлайн-стилей, `!important` и отдельных CSS-файлов. Тёмная тема — это смена значений токенов, отдельных `dark:`-костылей не нужно.
- Адаптивность: мобильный первый; на ширине 360 px нет горизонтальной прокрутки, длинные слова и длинные описания переносятся (`break-words`), никаких `truncate`, `line-clamp`, фиксированных высот и `overflow-hidden` на тексте.
- Порядок заголовков: один `<h1>` (в шапке), разделы `<h2>`, проекты `<h3>`.
- Контраст: `text-muted-foreground` на `bg-background` уже AA в токенах — не подменяй его на самодельный серый.
- «Состояние, которое должно шариться ссылкой» — это локаль: она в URL, отдельного клиентского состояния нет.

6. Тесты.
- `_pages/home/ui/testing/home-content.ts` — типизированные фикстуры-пропсы (`satisfies HomeContent`), без `any`, без приведения через `unknown`; MSW-обработчики (`_pages/home/api/testing/handlers.ts`) не меняй.
- `_pages/home/ui/home-page-view.test.tsx` (Vitest + RTL, через роли и текст, без снимков): success (h1, оба `<h2>`, headline, названия/описания/стек/год/href проектов, ссылки контактов); empty; partial-1 (нет профиля, есть проекты → метка частичной загрузки + заметка о профиле + проекты видны); partial-2 (есть профиль, нет проектов → заметка о проектах); переключатель локали (href, `aria-current`, английские заголовки на `en`); очень длинное описание (строка 400+ символов одним словом) присутствует в документе целиком.
- `src/__tests__/error-boundary.test.tsx`: рендерится понятный текст и кнопка «Повторить»; клик вызывает `reset`; текст исключения (`error.message`) на экране не появляется; `console.error` вызван (через `vi.spyOn`).
- `frontend/apps/web/e2e/page.spec.ts` — Playwright против живого API и `next start` (конфиг не меняй, порт 3100):
  - `/ru`: `<h1>` MySite; видны три проекта (DirectoryService, MySite, Harness); текста Keycloak нет нигде на странице; у проекта есть рабочая ссылка на github.com; контакты видны;
  - `/en`: у Harness русский заголовок (fallback по ADR-0005), у MySite английский; Keycloak отсутствует;
  - переключатель: с `/ru` клик по «EN» приводит на `/en`; на `/en` у «EN» `aria-current="page"`, у «RU» — нет; обратно на `/ru` работает;
  - 360×800: `document.documentElement.scrollWidth <= window.innerWidth` на `/ru` и на `/en`;
  - тёмная тема: при `colorScheme: "dark"` на `<html>` появляется класс `dark` (`next-themes`, `defaultTheme="system"`).
  - `e2e/home.spec.ts` не трогай: смоук остаётся как есть.

7. Документация.
- `WORKFLOW.md`, раздел Frontend: добавь в таблицу команд `pnpm test` и `pnpm test:e2e`; опиши предпосылки e2e (нужен `pnpm build`, живой API на `http://localhost:5054`, база с заглушками тикета 08; смоук `home.spec.ts` базы не требует). Замени устаревшую фразу «Tests are not part of the gate yet: ticket 20 installs…» на фактическое состояние: контур установлен тикетом 20, CI гоняет `test` и `e2e`.
- `.scratch/site-development/issues/16-the-page-itself.md`: заполни `## Answer` (что сделано → чем подтверждено, дословные команды и вывод → файлы → что осталось за рамками и вопросы владельцу) и переведи шапку `Status: open` в `Status: resolved` по §13. Только если все гейты зелёные; иначе оставь `open` и напиши причину и вопрос.

Do not touch: `backend/**`; `.dsh/**`; `.scratch/site-development/map.md`, `.scratch/site-development/plugin-run-16.md` и чужие тикеты; `frontend/pnpm-workspace.yaml` (каталог версий); `frontend/eslint.config.mjs`; `frontend/packages/eslint-config/**`; все `tsconfig*`; `frontend/apps/web/vitest.config.mts`; `frontend/apps/web/playwright.config.ts`; `frontend/packages/api-client/src/generated/**`; `.github/**`; `frontend/apps/web/e2e/home.spec.ts`; `frontend/apps/web/src/_pages/home/api/**` (кроме ничего — этот каталог не меняется).

Done when: `pnpm lint`, `pnpm typecheck`, `pnpm --filter @mysite/web test`, `pnpm build`, `pnpm --filter @mysite/web test:e2e` — все с кодом 0; четыре состояния реализованы и покрыты тестами; `/ru` и `/en` открываются на живом API; на `/en` у Harness русский текст, Keycloak_Learn не показывается; на 360 px нет горизонтальной прокрутки; `WORKFLOW.md` обновлён; `## Answer` заполнен дословным выводом команд, шапка `Status: resolved`.

Stop when: API или Docker не отвечают; или гейт падает по причине окружения, а не кода; или задача требует решения, которого нет в этом задании (новая зависимость, правка `packages/ui`, изменение контракта API, конфликт правил) — тогда не расширяй объём, а напиши вопрос в отчёте.

Never: git (никаких команд, даже чтения); файлы за пределами workspace; новые пакеты; `any`, `@ts-expect-error`, `@ts-ignore`, `export *`, подавления линта или типов; правка сгенерированных файлов; правка перечисленного в «Do not touch»; «заодно заметил и починил».

Report (по §13, коротко и с числами): первой строкой — загруженные скиллы и по одной строке, что каждый изменил в твоём подходе; затем что сделано, дословный вывод команд (включая числа тестов и код возврата), список файлов, что осталось за рамками, открытые вопросы и точные места, где задание расходилось с кодом.
```

## 2. Ребёнок и время старта

- Имя ребёнка: `qwen-page` (единственный исполнитель в сессии).
- Старт: 2026-11-16 20:20:24 +03:00 (`subagent` вернул имя в этот момент).
- Ветка `feature/the-page-itself` создана архитектором до задачи; рабочее дерево было чистым;
  Qwen получил готовую ветку и запрет на любые git-команды.

## 3. Скиллы и почему именно они

Названы: `qwen-executor`, `vercel-react-best-practices`, `better-accessibility`, `better-layout`,
`tdd`.

- `qwen-executor` — контракт исполнителя: без него ребёнок не знает своих рамок (git, отчёт,
  запреты), а протокол требует называть его всегда.
- `vercel-react-best-practices` — страница на Server Components: цена клиентского кода, граница
  сервер/клиент, композиция данных; именно здесь легче всего «утащить» страницу в клиент.
- `better-accessibility` — WCAG 2.2 AA в проекте жёсткое требование; состояния и ошибки без
  семантики и фокуса не принимаются.
- `better-layout` — экран собирается из блоков впервые: порядок чтения, группировка и иерархия
  заголовков — предмет скилла, а не вкуса.
- `tdd` — приёмка сформулирована состояниями: тест на состояние раньше реализации.

`better-typography`, `better-colors`, `better-ui`, `better-writing` и сводный `better-interface`
намеренно не даны исполнителю: это моя приёмка (ревью), и отдавать её целиком ребёнку нельзя.
