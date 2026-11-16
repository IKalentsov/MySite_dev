# 20 — Тестовый контур фронтенда

Type: task
Status: resolved
Blocked by: 10, 14, 18

## Как работать с этой задачей

**Правила работы с проектом — `.dsh/AGENT-TASKS.md`.** Прочитай его первым, до первой правки: там
команды, правила веток, среда и критерий готовности. Ниже — только специфика этой задачи.

- Ветка: `feature/frontend-test-contour` от свежего `develop`; push ветки и PR в `develop` — работа
  исполнителя, слияние — владельца.
- Критерий готовности: раннер установлен и настроен, `pnpm test` зелёный на настоящем тесте, CI
  прогоняет его, версии — только в pnpm-каталоге. Без `any`, без `@ts-expect-error`, без
  `export *`.
- Версии из решения 10: Vitest 5.0.3, `@testing-library/react` 16.3.3,
  `@testing-library/dom` 10.4.2, `@testing-library/jest-dom` 7.0.1,
  `@testing-library/user-event` 14.6.7, jsdom 30.1.2, MSW 3.0.2, Playwright 1.63.0.
  `vite`, `@vitejs/plugin-react` и `vite-tsconfig-paths` в 10 не запинованы — взять точный текущий
  stable из реестра и перечислить каждый новый пин в `## Answer`.
- Тестовый контур не должен требовать тестирования `async` Server Components: по решению 18
  тестируются синхронный view на пропсах, клиентский слой и e2e.

**Условие остановки:** `pnpm test` (Vitest) и smoke Playwright зелёные, CI прогоняет их, конфиги и
версии лежат в монорепе; продуктовый код страницы не менялся.

## Question

Поставить тестовый контур, выбранный в тикете 10, и подключить его к гейтам:

- **Vitest 5** в `apps/web`: `vitest.config.mts` по официальному гайду Next.js —
  `@vitejs/plugin-react`, `vite-tsconfig-paths`, `environment: "jsdom"`, `globals: true`,
  `setupFiles`. Setup-файл импортирует `@testing-library/jest-dom/vitest`.
- **React Testing Library** + `user-event`: первый настоящий тест на синхронный компонент
  (например, `HomePageView` из тикета 16 или `Button` из `packages/ui` — компонент, который уже
  есть), проверка через роль и текст, без снимков и внутреннего состояния.
- **MSW 3**: `setupServer` в тестовом setup, `server.listen()`/`resetHandlers()`/`close()`;
  один тест, который реально подменяет запрос на уровне сети (можно на маленькой обёртке над
  `fetch`/клиентом; ждать 15 не нужно).
- **Playwright 1.63**: `apps/web/playwright.config.ts`, `testDir: "./e2e"`, `webServer` с
  `next start` на отдельном порту; smoke-тест: `/` приводит на `/ru`, у документа `lang="ru"`.
  Без базы и без API — на пустой странице этого достаточно.
- **Скрипты и CI**: `apps/web` получает `test` (`vitest run`) и `test:e2e` (`playwright test`);
  корневой `test` гоняет их в нужном порядке. `.github/workflows/frontend.yml` получает шаги
  тестов после `build`; Playwright ставит браузеры (`pnpm exec playwright install --with-deps
  chromium`). Порядок в CI: install → lint → typecheck → build → test → e2e.
- **Границы**: тесты живут рядом с кодом или в `e2e/`; `async` Server Components не тестируются
  Vitest'ом (18); MSW не подменяет RSC.

Критерий: `pnpm test` и `pnpm test:e2e` зелёные локально, CI зелёный, версии только в каталоге.

## Уточнения после исследования 11 (2026-11-16)

- **Клиент тестируется через MSW на уровне запроса.** Обработчики — на три пути схемы:
  `/api/v1/home`, `/api/v1/profile`, `/api/v1/projects` (плюс 400 на неизвестную локаль, если
  проверяется маппинг ошибки). Фикстуры типизируются типами из `@mysite/api-client`; `any` в
  обработчиках и фикстурах нет. MSW 3: `onUnhandledFrame`, а не `onUnhandledRequest`.
- **Сгенерированный файл не тестируется.** Тестируется публичный API пакета: `getHome` /
  `getProfile` / `getProjects`, маппинг `ProblemDetails` в `ApiError`, проброс `cache: "no-store"`.
  Отдельного юнита на `src/generated/openapi.d.ts` нет — его гарантирует `tsc` и `git diff` из 15.
- **Порядок.** Сам контур ставится независимо от 15; клиентские тесты через MSW добавляются после
  того, как 15 сгенерирует пакет. Если 20 идёт раньше, MSW проверяется на маленькой обёртке или
  существующем компоненте (как в исходном вопросе), а не на `@mysite/api-client`. Шаг 15
  (`generate` + `git diff --exit-code -- packages/api-client`) встаёт в CI до `test`.
- **Playwright для данных.** Smoke из 20 остаётся без базы и API; сквозной путь «страница →
  локальный API» — в 16, с поднятой базой и залитыми заглушками (08). `next start` + отдельный
  порт; `reuseExistingServer` не в CI.
- **Никаких `any`/`@ts-expect-error`/`export *`** в тестовом коде; фикстуры — через `satisfies`
  или явный тип из пакета, без приведения через `unknown`.

## Answer

### Что сделано

1. **Версии в каталоге.** Добавлены 11 новых пинов в `catalog:` в `frontend/pnpm-workspace.yaml`:
   - vitest: 5.0.3 (из тикета 10)
   - @testing-library/react: 16.3.3 (из тикета 10)
   - @testing-library/dom: 10.4.2 (из тикета 10)
   - @testing-library/jest-dom: 7.0.1 (из тикета 10)
   - @testing-library/user-event: 14.6.7 (из тикета 10)
   - jsdom: 30.1.2 (из тикета 10)
   - msw: 3.0.2 (из тикета 10)
   - @playwright/test: 1.63.0 (из тикета 10)
   - vite: 8.3.8 (текущий stable из реестра, Vitest 5 требует peer `vite >=6.4.0`)
   - @vitejs/plugin-react: 6.3.0 (текущий stable из реестра)
   - vite-tsconfig-paths: 6.1.1 (текущий stable из реестра)

2. **Vitest конфиг.** `frontend/apps/web/vitest.config.mts` — по официальному гайду Next.js:
   `@vitejs/plugin-react`, `vite-tsconfig-paths`, `environment: "jsdom"`, `globals: true`,
   `setupFiles`, `exclude: ["**/e2e/**"]`.

3. **Setup-файл.** `frontend/apps/web/src/_pages/home/api/testing/vitest.setup.ts` — импортирует
   `@testing-library/jest-dom/vitest`, поднимает MSW (`setupServer`), `listen()` с
   `onUnhandledFrame: "bypass"` в `beforeAll`, `resetHandlers()` в `afterEach`, `close()` в
   `afterAll`.

4. **MSW-обработчики.** `frontend/apps/web/src/_pages/home/api/testing/handlers.ts` — один
   обработчик на `GET http://localhost:5054/api/v1/home`; фикстуры типизированы `HomeResponse`,
   `OwnerProfile` и `Project` из `@mysite/api-client`, дублей типов нет. Обработчики лежат рядом
   с границей данных: политика ESLint разрешает импорт клиента только слою `pages`.

5. **Компонентный тест.** `frontend/apps/web/src/__tests__/button.test.tsx` — три теста на
   `Button` из `@mysite/ui`: рендер по роли и тексту, вызов `onClick`, проверка CSS-класса
   варианта `destructive`. Без снимков и внутреннего состояния.

6. **MSW-тест.** `frontend/apps/web/src/_pages/home/api/get-home.test.ts` — два теста: успешный
   вызов `getHome("ru")` с проверкой полей ответа, и проверка что `getHome("xx")` при ошибке 400
   бросает `ApiError`. Используется `server.use()` для переключения обработчика.

7. **Playwright конфиг.** `frontend/apps/web/playwright.config.ts` — `testDir: "./e2e"`,
   `webServer` с командой `pnpm exec next start -p 3100` (не 3000), `reuseExistingServer:
   !process.env.CI`, один проект `chromium`.

8. **E2E-тест.** `frontend/apps/web/e2e/home.spec.ts` — запрос `/` приводит на `/ru`, у
   документа `<html lang="ru">`. Без базы и без API.

9. **Скрипты.** В `apps/web/package.json` добавлены `test: "vitest run"` и
   `test:e2e: "playwright test"`. В корневом `frontend/package.json` добавлен
   `test: "pnpm --filter @mysite/web test && pnpm --filter @mysite/web test:e2e"`.

10. **CI.** `.github/workflows/frontend.yml` — после шага `Build` идут
    `Install Playwright browsers` → `Test` (`pnpm --filter @mysite/web test`) → `E2E`
    (`pnpm --filter @mysite/web test:e2e`). Порядок: install → generate/client diff → lint →
    typecheck → build → install browsers → test → e2e. Корневой `pnpm test` по-прежнему гоняет
    оба локально, поэтому в CI взяты фильтрованные команды — без двойного прогона e2e.

11. **tsconfig.** В `apps/web/tsconfig.json` добавлены `"types": ["vitest/globals",
    "@testing-library/jest-dom"]` для глобальных функций Vitest и matchers jest-dom.

### Правки после проверки архитектором

Проверка нашла три дефекта, исправленных до PR; после правок все гейты перепрогнаны.

- тест импортировал `./vitest.setup`, а файл лежит в `./testing/vitest.setup` — прогон был красным,
  импорт исправлен;
- в setup стоял `eslint-disable-next-line @typescript-eslint/no-unsafe-call` (глушить правило
  нельзя) и неиспользуемый тип `SetupServer` — заменено на `setupServer(...handlers)` без
  подавлений;
- в `apps/web/package.json` был добавлен ненужный `"type": "module"` — убран.

### Подтверждение (команды и вывод)

**`pnpm install --frozen-lockfile`** — проходит, lockfile обновлён командой `pnpm install
--lockfile-only`.

**`pnpm lint`** — чисто: `$ eslint .` без ошибок.

**`pnpm typecheck`** — чисто: все 6 пакетов проходят `tsc --noEmit`.

**`pnpm build`** — чисто: Next.js 16.4.0, TypeScript пройден, 2 страницы сгенерированы.

**`pnpm --filter @mysite/web test`** (Vitest):
```
 ✓ src/_pages/home/api/get-home.test.ts (2 tests) 42ms
 ✓ src/__tests__/button.test.tsx (3 tests) 111ms

 Test Files  2 passed (2)
      Tests  5 passed (5)
```

**`pnpm --filter @mysite/web test:e2e`** (Playwright, сервер поднимается сам на порту 3100):
```
[1/1] [chromium] › e2e\home.spec.ts:3:5 › GET / redirects to /ru and the document has lang="ru"
  1 passed (1.8s)
```

### Список файлов

**Созданы:**
- `frontend/apps/web/vitest.config.mts` — конфиг Vitest по гайду Next.js
- `frontend/apps/web/src/_pages/home/api/testing/vitest.setup.ts` — jest-dom + жизненный цикл MSW
- `frontend/apps/web/src/_pages/home/api/testing/handlers.ts` — MSW-обработчик на `/api/v1/home`
- `frontend/apps/web/src/_pages/home/api/get-home.test.ts` — MSW-тест `getHome` (2 теста)
- `frontend/apps/web/src/__tests__/button.test.tsx` — компонентный тест на Button (3 теста)
- `frontend/apps/web/playwright.config.ts` — конфиг Playwright, порт 3100
- `frontend/apps/web/e2e/home.spec.ts` — smoke-тест: `/` → `/ru`, `lang="ru"`

**Изменены:**
- `frontend/pnpm-workspace.yaml` — 11 пинов в `catalog:`
- `frontend/apps/web/package.json` — скрипты test/test:e2e и devDependencies
- `frontend/package.json` — корневой `test` (vitest + e2e)
- `frontend/apps/web/tsconfig.json` — `types` для vitest/jest-dom
- `.github/workflows/frontend.yml` — установка браузера, Test, E2E после Build

### Что осталось за рамками

- Тесты на `getProfile`/`getProjects` — в пакете пока нет публичных функций для них (только
  `getHome`). Добавляются при появлении API.
- Не менялся продуктовый код (`home-page.tsx`, `get-home-content.ts`) — только ради тестов.

### Открытые вопросы

- Vite 8 резолвит tsconfig paths нативно и при запуске подсказывает убрать `vite-tsconfig-paths`;
  плагин оставлен, потому что его называет тикет. Замена на нативную опцию — отдельное изменение.
- Локально браузер Playwright был установлен вручную (`npx playwright install --with-deps chromium`);
  в CI это делает автоматический шаг. Зависимости ОС (libglib, libnss и т.д.) на Windows не требуются.
