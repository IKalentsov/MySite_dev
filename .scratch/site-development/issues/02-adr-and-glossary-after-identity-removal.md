# 02 — Записать отмену части ADR-0003 и убрать противоречия в документах

Type: task
Status: open
Blocked by: 01

## Question

После удаления identity-стека документы проекта описывают код, которого больше нет. Правки:

- **Новый ADR** (следующий номер в `docs/adr/`) по формату из скилла `domain-modeling`: что
  удалено, почему, что это отменяет в ADR-0003 и при каком условии identity вернётся (первый
  write-сценарий админки). ADR-0003 не переписывается — он остаётся историей, новый ADR на него
  ссылается.
- **`CONTEXT.md`**: «Админка — It is the reason authentication exists in this codebase at all»
  перестаёт быть правдой. Глоссарий правится минимально: определение панели остаётся, ссылка на
  существующую авторизацию уходит.
- **`ARCHITECTURE.md`**: абзац про `UserController` → `UsersService` → `IJwtProvider`, пункт
  «Authentication is not wired» в § Known problems, строка про `users` в § Content model.
- **`WORKFLOW.md`**: «The account endpoints are still not reachable».
- **`README.md`**: упоминания аккаунтов, если остались.
- **`.dsh/AGENTS.md`**, раздел Project facts: строки «Whether the existing users and JWT code
  becomes the panel's foundation or is replaced» и «Planned».

Проверка результата: `git grep` по `Jwt`, `User`, `auth` не находит в документах того, чего нет
в коде.

## Answer

<!-- заполняется при закрытии -->
