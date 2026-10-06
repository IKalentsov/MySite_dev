# 10 — Тест-контур и качество фронтенда

Type: research
Status: open

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

<!-- заполняется при закрытии -->
