# 10 — Тест-контур и качество фронтенда

Type: research
Status: resolved

## Question

ADR-0004 зафиксировал открытый вопрос: «The test runner is still listed as Vitest, which is
unconfirmed for Next.js». Правила проекта требуют уровней «юнит / компонентный / интеграционный с
MSW / e2e Playwright», ESLint (flat config) + typescript-eslint, Prettier и git-хук на изменённые
файлы.

Выяснить по первоисточникам:

- Vitest против Jest в проекте на Next.js App Router сегодня: что поддерживается официально, как
  настраивается, что с серверными компонентами и с `next/jest`.
- React Testing Library: проверка поведения через роли и текст.
- MSW: подстановка сети на уровне запроса, не через мокинг модулей.
- Playwright: где живёт конфигурация в монорепе, что запускается в CI.
- ESLint flat config + typescript-eslint + `eslint-plugin-boundaries` или аналог для проверки
  направления импортов между слоями FSD (правило проекта: границу проверяет линтер, а не
  дисциплина).
- Prettier и git-хуки: что запускается на коммите.

Результат — `docs/research/frontend-quality-contour.md` со ссылками на источники.

## Answer

Разобрано: **`docs/research/frontend-quality-contour.md`**. Открытый вопрос ADR-0004 («The test
runner is still listed as Vitest, which is unconfirmed for Next.js») закрыт.

**Решения:**

- **Раннер — Vitest 5.0.3** (нужны Node ≥22.12.0 и Vite ≥6.4.0 как обязательный peer). Конфиг по
  официальному гайду Next.js: `vitest.config.mts` + `@vitejs/plugin-react` + `vite-tsconfig-paths`,
  `environment: 'jsdom'`. Jest документирован не хуже; его единственное реальное преимущество —
  `next/jest` с автомоками CSS, картинок и `next/font`, но сам `next/jest` помечен
  экспериментальным на своей же странице ошибки.
- **RTL 16.3.3** + `@testing-library/dom` 10.4.2, `jest-dom` 7.0.1 (импорт
  `@testing-library/jest-dom/vitest`), `user-event` 14.6.7, jsdom 30.1.2. Нужен `globals: true`,
  иначе автоочистка не работает.
- **MSW 3.0.2** — ESM-only; в третьей версии `onUnhandledRequest` переименован в
  `onUnhandledFrame`.
- **Playwright 1.63.0**; конфиг в `apps/web/playwright.config.ts` — это вывод, официального гайда по
  монорепе не существует.
- **ESLint 10.12.0** — только flat config, `.eslintrc` удалён из v10; `next lint` удалён в
  Next.js 16, линт идёт через `eslint .`. `typescript-eslint 8.71.1`, типизированный линт через
  `configs.recommendedTypeChecked` + `parserOptions.projectService: true`.
- **Границы FSD — `eslint-plugin-boundaries` 7.2.0** плюс `import/no-restricted-paths` (он резолвит
  пути, а не сравнивает строки импорта). `@feature-sliced/eslint-config` не годится: бета,
  последний релиз 2022, только `.eslintrc` — под ESLint 10 мёртв. В документации FSD правило слоёв
  описано, но инструмента принуждения там нет вовсе.
- **Хуки — lefthook 2.1.17**, один корневой `lefthook.yml`. lint-staged отвергнут: его
  документированная модель в монорепе — конфиг на каждый пакет, и он «не сливает конфиги», то есть
  часть staged-файлов молча выпадает из проверки.
- Prettier 3.9.9, `eslint-config-prettier/flat`, отдельно от ESLint.

**Два вывода, которые меняют план:**

1. **`async` Server Components не тестируются ни в Vitest, ни в Jest** — оба гайда Next.js говорят
   это прямо и советуют для них e2e. Значит уровень «интеграционный с MSW» не может быть тестом на
   async-странице: границу данных надо спроектировать так, чтобы экран собирался из синхронной
   презентационной части. Это ограничение на слой `pages` в FSD, а не выбор инструмента. Вынесено в
   тикет 18.
2. **MSW для серверных/RSC-запросов первоисточниками не подтверждён.** Next.js про MSW не пишет,
   официального примера нет, трекинг-ишью закрыт без решения. Смягчение — держать границу данных
   тонкой, за публичным API сущности или фичи, чтобы данные можно было подставить.

**Третье, что важнее инструментов:** «хук проверяет типы только изменённых файлов» —
**невыполнимо**. Документация TypeScript: «When input files are specified on the command line,
`tsconfig.json` files are ignored». Режима частичной проверки не существует, а сторонние обёртки
вроде `tsc-files` по построению пропускают поломки между файлами. Правило в `.dsh/AGENTS.md`
приводится в соответствие: хук запускается по glob staged-файлов, но гоняет `tsc --noEmit` по
проекту целиком. Отдельно: кеш ESLint для типизированного линта и для проверки границ включать
нельзя — документация ESLint предупреждает об устаревших результатах именно для type-aware и
path-resolving правил.
