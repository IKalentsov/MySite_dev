# 13 — Где живут компоненты и токены, и коллизия `app`

Type: grilling
Status: resolved
Blocked by: 09

## Как работать с этой задачей

**Правила работы с проектом — `.dsh/AGENT-TASKS.md`.** Прочитай его первым, до первой правки: там
команды, правила веток, среда и критерий готовности. Ниже — только специфика этой задачи.

- Ветка: `feature/component-home-and-tokens` от текущего `master`.
- Коммить в неё; **не пушить** и не трогать чужие задачи.
- Решение принимается **до первого компонента** и фиксируется в `## Answer` с обоснованием: где
  живут shadcn/ui и токены, как разводятся `app` слоя FSD и `app/` маршрутов, что экспортирует
  `packages/ui` и как это стережёт линтер.
- Это тикет-решение: «зелёные тесты» к нему не применяются.

**Условие остановки:** Решение записано в файле тикета с обоснованием: где живут shadcn/ui и токены, как разводятся FSD-слой `app` и каталог маршрутов `app/`, что экспортирует `packages/ui` и чем это стережёт линтер.

## Question

Последний открытый вопрос ADR-0004 плюс два, которые выросли из выбора полной монорепы:

1. **Где живут shadcn/ui компоненты.** В `packages/ui` как дизайн-система или внутри FSD-слоёв
   `apps/web`. shadcn копирует код в репозиторий, то есть компоненты становятся кодом проекта и
   подпадают под гейты качества.
2. **Коллизия имён.** FSD называет верхний слой `app`, и Next.js тоже называет каталог маршрутов
   `app/`. Решение меняет каждый путь импорта, поэтому принимается до первого компонента.
3. **Токены.** Цвета, отступы, радиусы, шкала типографики: живут в `packages/ui` как Tailwind
   preset или в `apps/web`. Тёмная тема — часть дизайн-системы, а не задача «потом».
4. **Границы.** Что именно экспортирует `packages/ui` и как линтер запрещает импорт из глубины
   пакета.

Контекст: визуальный дизайн не начат, референсы владелец принесёт позже; до тех пор нужна
аккуратная типографическая база, а не выдуманный бренд.

## Answer

**Решение принято владельцем 2026-11-16 коротким интервью по четырём вопросам; кода в тикете нет —
это grilling/decision.** Ветка `feature/component-home-and-tokens` от свежего `develop` (`400b29a`);
изменены только этот файл и `docs/adr/0004-nextjs-app-router-and-fsd-frontend.md`.

### 1. shadcn/ui живёт в `packages/ui`

`packages/ui` — дизайн-система: скопированные shadcn-компоненты, `cn`, токены, стили.
App-специфичная композиция — в FSD-слоях `apps/web`, а не в пакете. Официальная поддержка монорепы у
shadcn (`apps/web` + `packages/ui`, `components.json` в каждом воркспейсе, установка компонентов в
пакет) подтверждает эту раскладку; `packages/ui` уже записан в раскладку `.dsh/AGENTS.md`, а решение
09 уже положило туда токены.

Что где:

- примитивы: `frontend/packages/ui/src/components/ui/*.tsx` (то, что создаёт `shadcn add`),
  `frontend/packages/ui/src/lib/utils.ts` (`cn`), при необходимости `src/hooks/`;
- публичный API: `frontend/packages/ui/src/index.ts` (см. §4);
- токены и стили: `frontend/packages/ui/src/styles/globals.css` (см. §3);
- композиция экрана — `apps/web/src/_pages/*`, блоки — `widgets`/`features`/`entities`;
  `shared/ui` — не второй UI-кит: туда попадает только app-специфичный презентационный компонент,
  переиспользуемый несколькими слайсами и не являющийся частью дизайн-системы (по умолчанию пусто).

**Цена:** пакетная граница (свои `package.json`, `components.json`, `tsconfig`, гейты), Tailwind
обязан сканировать оба дерева, изменение примитива — изменение пакета. **Альтернативы:** компоненты
в `apps/web/src/shared/ui` — проще, но дизайн-системы нет, второй app её не переиспользует, переезд
позже дорог; гибрид «примитивы в пакете, обёртки в shared» — два владельца компонентов и неочевидный
публичный API.

### 2. Коллизия `app`: маршруты Next — `apps/web/src/app`, FSD без слоя `app`, слой `pages` → `_pages`

```
frontend/apps/web/src/
├── app/                  # только Next App Router: layout, providers, globals.css, error/loading, [lang]
├── _pages/               # FSD pages: один экран = слайс
├── widgets/
├── features/
├── entities/
└── shared/
```

- **Отдельного FSD-слоя `app` нет.** Его обязанности в App Router уже выполняет каталог маршрутов:
  `app/layout.tsx` (html lang, шрифты, `suppressHydrationWarning`), `app/providers.tsx` (клиентские
  провайдеры, в том числе `next-themes`), `app/globals.css`, `error.tsx`/`loading.tsx`/
  `not-found.tsx`, metadata, редирект `/` → `/[lang]`. Дублировать это слоем `app/` — церемония без
  содержания.
- **FSD-слой `pages` переименован в `_pages`.** Причина фактическая: `src/pages` — это каталог Pages
  Router, и Next.js прочитал бы слайсы `pages/*` как маршруты. `_pages` — официальная рекомендация
  FSD для Next.js (там же `_app`; у нас `_app` не нужен, см. выше).
- **Route-файлы тонкие:** только реэкспорт публичного API слайса и Next-специфика.

  ```tsx
  // app/[lang]/page.tsx
  export { HomePage as default, metadata } from "@/_pages/home";
  ```

  Логика, разметка и состояния живут в `_pages/home/ui`, ниже по слоям.
- **Импортная цепочка:** `app → _pages → widgets → features → entities → shared`. Слайсы `_pages`,
  `widgets`, `features`, `entities` одного слоя друг друга не импортируют; `shared` — исключение
  (он одновременно слой и слайс). Пакеты (`@mysite/ui`, `@mysite/api-client`, `@mysite/shared`) —
  ниже `shared`: их можно импортировать из любого слоя, но только через публичный API.
- **`apps/web/src/app` вместо корневого `apps/web/app`:** Next.js документирует оба варианта, а `src`
  держит весь код приложения в одном корне и не смешивает его с конфигами. FSD-гайд предпочитает
  корневой `app/`, но прямо говорит, что это не обязательно.

**Цена:** имя `_pages` вместо канонического `pages`; тонкие route-файлы; таблицу слоёв и открытые
вопросы в `.dsh/AGENTS.md` правит планер (см. «За рамками»). **Альтернативы:** FSD-гайд с корневым
`app/` и слоями `_app`/`_pages` — сохраняет шесть слоёв, но добавляет слой, дублирующий
Next-каталог, и два корня; FSD в `src/fsd/*` — канонические имена, но каждый импорт `@/fsd/...` и
нестандартная раскладка.

### 3. Токены — в `packages/ui/src/styles/globals.css`

Один источник: `frontend/packages/ui/src/styles/globals.css`.

- примитивы (палитра, шкала типографики, отступы, радиусы) — `@theme`;
- семантические переменные (`--background`, `--foreground`, `--primary`, …) — `:root` и `.dark`;
  мост к утилитам — `@theme inline` (паттерн shadcn для Tailwind 4), чтобы `bg-background` читал
  runtime-переменную, а тёмная тема была сменой значений токенов, а не россыпью `dark:` в
  компонентах;
- `@custom-variant dark (&:where(.dark, .dark *));` — в том же файле;
- `@source` — оба дерева: `@source "../../**/*.{ts,tsx}"` (пакет) и
  `@source "../../../../apps/web/src/**/*.{ts,tsx}"` (приложение). Это обязательная часть монорепы:
  без неё Tailwind не увидит классы, лежащие в `packages/ui`. У самого shadcn в шаблоне была ошибка
  в глубине `@source`; пути проверяются в 14 сборкой CSS, а не на глаз.

`frontend/apps/web/src/app/globals.css` — только вход:

```css
@import "tailwindcss";
@import "@mysite/ui/styles/globals.css";
```

`next-themes` 0.4.6 (решение 09) — `ThemeProvider` в `apps/web/src/app/providers.tsx` с
`attribute="class"`, `defaultTheme="system"`, `enableSystem`, `disableTransitionOnChange`;
`suppressHydrationWarning` на `<html>` в `app/layout.tsx`. Тёмная тема — часть дизайн-системы с
первого компонента, а не «потом».

**Цена:** `@source`-пути хрупкие (относительные, ломаются при переезде каталога), CSS обязан
экспортироваться пакетом, тема принадлежит пакету. **Альтернативы:** токены в `apps/web` — проще, но
дизайн-система не самодостаточна и расходится с 09; «палитра в пакете, семантика в приложении» — два
слоя имён и два места для тёмной темы.

### 4. Публичный API `packages/ui` — только индекс; глубина запрещена тремя гейтами

`frontend/packages/ui/package.json` — ключевая часть манифеста:

```json
{
  "name": "@mysite/ui",
  "private": true,
  "type": "module",
  "exports": {
    ".": "./src/index.ts",
    "./styles/globals.css": "./src/styles/globals.css"
  },
  "imports": {
    "#components/*": "./src/components/*.tsx",
    "#lib/*": "./src/lib/*.ts",
    "#hooks/*": "./src/hooks/*.ts"
  }
}
```

- `src/index.ts` — единственный публичный API: **явные named-экспорты**
  (`export { Button } from "#components/ui/button";`), никаких `export *`. Внутренние пути
  (`#components`, `#lib`, `#hooks`) — только для кода самого пакета; `exports` их не публикует.
- `./styles/globals.css` — единственный разрешённый subpath: CSS-импорт из
  `apps/web/src/app/globals.css`. Это не API компонентов.
- Внутренние импорты пакета — через `package.json#imports` (`#lib/utils`), как документирует shadcn
  для кода, установленного внутрь `packages/ui`; `tsconfig` пакета — `moduleResolution: "bundler"`,
  `resolvePackageJsonImports: true`.

**Чем стережёт линтер** (в `frontend/packages/eslint-config/`, подключается корневым
`frontend/eslint.config.mjs`):

1. **`exports` пакета** — жёсткий гейт резолвинга: `@mysite/ui/components/button` или
   `@mysite/ui/src/index` не разрешатся ни Next.js, ни `tsc`.
2. **`no-restricted-imports`** (ESLint core, flat config) для `apps/**` и
   `packages/{api-client,shared}/**`: `patterns: [{ group: ["@mysite/ui/*"],
   message: "Import from @mysite/ui root; deep imports are not public API" }]`. Правило ловит и
   алиасы, и `import type`, и даёт внятное сообщение вместо «module not found».
3. **`boundaries/dependencies`** (`eslint-plugin-boundaries`, решение 10) — направление слоёв и
   изоляция слайсов; `boundaries/elements` описывает `app`, `_pages`, `widgets`, `features`,
   `entities`, `shared`, `ui-package`, `api-client`, `shared-package`; `default: "disallow"`,
   разрешены только внутренние зависимости слайса и импорт вниз по цепочке.
   `import/no-restricted-paths` (решение 10) остаётся вторым, резолвящим пути, поясом для тех же зон.

**Цена:** `src/index.ts` поддерживается руками — после `shadcn add` надо добавить named-экспорт;
если блок ставится CLI в приложение, его импорты-субпути (`@workspace/ui/components/button` —
документированный стиль shadcn) придётся переписать на корень; `exports` расширяется только
осознанно. CLI для примитивов
запускается **в `packages/ui`**, а не в `apps/web`: так генератор пишет файлы в пакет и использует
`#`-алиасы, не расширяя публичный API. `components.json` приложения остаётся только для
app-локальных блоков, и его `ui`-алиас смотрит внутрь `apps/web`, а не в пакет. Точная раскладка
алиасов проверяется в 14.

**Альтернативы:** subpath-exports `./components/*`, `./lib/*`, `./hooks/*` — нулевое трение с CLI и
чище tree-shaking, но публичный API перестаёт быть одним файлом; гибрид «subpath для CLI, index для
app» — два API и два правила, которые надо держать согласованными.

### Где решение живёт в файлах (создаётся в 14, не в этом тикете)

```
frontend/
├── pnpm-workspace.yaml          # 17: catalog; packages: apps/*, packages/*
├── package.json                 # корневые скрипты
├── eslint.config.mjs            # подключает packages/eslint-config
├── packages/
│   ├── eslint-config/           # boundaries + no-restricted-imports + import/no-restricted-paths
│   ├── ui/
│   │   ├── package.json         # @mysite/ui: exports ".", "./styles/globals.css"; imports "#…"
│   │   ├── components.json      # #-алиасы; tailwind.css = src/styles/globals.css
│   │   ├── tsconfig.json
│   │   └── src/
│   │       ├── index.ts         # публичный API
│   │       ├── components/ui/   # shadcn-примитивы
│   │       ├── lib/utils.ts     # cn
│   │       ├── hooks/
│   │       └── styles/globals.css
│   └── tsconfig/
└── apps/web/
    ├── package.json             # web; "@mysite/ui": "workspace:*"
    ├── components.json          # только app-локальные блоки
    ├── next.config.ts
    ├── tsconfig.json            # "@/*": ["./src/*"]
    ├── playwright.config.ts
    └── src/
        ├── app/                 # маршруты Next
        │   ├── layout.tsx
        │   ├── providers.tsx
        │   ├── globals.css
        │   ├── page.tsx         # редирект "/" → "/[lang]"
        │   └── [lang]/{layout.tsx,page.tsx}
        ├── _pages/home/{index.ts,ui/home-page.tsx}
        ├── widgets/ features/ entities/ shared/
```

### Совместимость с 09, 10, 17

- **09:** `next` 16.4.0, `react`/`react-dom` 19.3.0, `typescript` 6.0.3, `tailwindcss`/
  `@tailwindcss/postcss` 4.3.3, `shadcn` 4.21.3, `next-themes` 0.4.6; `transpilePackages` не нужен —
  Turbopack транспилирует workspace-пакеты; токены в `@theme` в `packages/ui`; тёмная тема —
  `@custom-variant dark` + `next-themes`. Решение ничего из этого не меняет.
- **10:** ESLint 10 только flat config; типизированный линт; `eslint-plugin-boundaries` +
  `import/no-restricted-paths`; lefthook; кеш ESLint для boundaries не включается. Решение добавляет
  к этому только `no-restricted-imports` на субпути пакета и `exports` как резолвинг-гейт.
- **17:** версии — только в `catalog:` `pnpm-workspace.yaml`; `package.json` пакетов ссылаются
  `"catalog:"`. Решение не вводит ни одной версии.
- **FSD:** `src/pages` — это Pages Router, поэтому `_pages`; `src/app` для маршрутов Next
  документирован; route-файлы — реэкспорты; при появлении server-only модулей в слайсе —
  `index.server.ts` (гайд FSD), это забота 18/16, не 13.

### За рамками и что требует решения владельца/планера

- **`.dsh/AGENTS.md` не трогался** (правит планер): таблица слоёв всё ещё содержит `app/` и `pages/`;
  открытые вопросы «где shadcn/ui» и «коллизия app» закрыты этим тикетом и должны быть убраны;
  формулировку «shared — UI kit» стоит привести к «дизайн-система в `packages/ui`, shared —
  app-специфичное». Это единственное место, где документ теперь расходится с решением.
- **Тикет 14** создаёт монорепу, `components.json`, `exports`, ESLint-правила и проверяет
  `pnpm lint`/`typecheck`/`build`; там же эмпирически проверяются `@source`-пути и резолвинг
  `exports` на `.ts`-исходниках.
- **Тикеты 15/16/18** — клиент, страница и граница данных; тёмная тема и четыре состояния экрана
  используют это решение, но не решаются здесь.
- **shadcn-блоки** (`add login-01` и подобные) в этой раскладке ставятся в приложение, а не в
  пакет; их импорты должны идти через `@mysite/ui`, а не через субпути. Если 14/16 упрётся в CLI,
  это отдельное уточнение, а не повод расширять `exports`.
- **Кода нет.** Сборка и тесты к тикету-решению не применяются (условие тикета); backend не
  менялся. В рабочем дереве остался чужой незакоммиченный
  `backend/tests/MySite.ArchitectureTests/MySite.ArchitectureTests.csproj` (удалён `ProjectReference`
  на негативную фикстуру) — он не мой и в коммит не попал.

### Git

`git fetch origin` (ключ `~/.ssh/my_ssh`), `develop` fast-forward с `3bed360` на `400b29a`;
`git rev-parse develop origin/develop` совпал (`400b29a9dea2c822a0c706ed71401e3d842c6bf9`) — PR #13
(тикет 06) к моменту работы уже был влит, поэтому ветка `feature/component-home-and-tokens` создана
от `400b29a`, а не от `3bed360`. Входное состояние «develop = 3bed360, PR #13 может быть не влит»
устарело на момент выполнения.
