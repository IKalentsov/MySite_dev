# 14 — Скаффолд монорепы, гейты, CI фронтенда

Type: task
Status: open
Blocked by: 09, 10, 13, 17

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
