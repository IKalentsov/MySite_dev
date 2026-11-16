# 20 — Тестовый контур фронтенда

Type: task
Status: open
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

<!-- заполняется при закрытии -->
