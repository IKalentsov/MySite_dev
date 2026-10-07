# 14 — Скаффолд монорепы, гейты, CI фронтенда

Type: task
Status: open
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

<!-- заполняется при закрытии -->
