# 02 — Записать удаление identity и привести документы в соответствие с репозиторием

Type: task
Status: open
Blocked by: —

## Question

Тикет 01 выполнен, следом сделан рефакторинг композиции DI (тикет 19). Документы проекта
описывают код, которого больше нет, и вдобавок содержат утверждения, ложные ещё до этих изменений.
**Тикет 05 поглощён этим тикетом**: обе задачи правят один и тот же `ARCHITECTURE.md`, и делать это
двумя последовательными заходами бессмысленно.

**1. Новый ADR.** Решение удалить identity-стек целиком: что удалено, почему, что это отменяет в
ADR-0003, при каком условии авторизация вернётся (первый write-сценарий админки). ADR-0001 и
ADR-0003 не переписываются — их текст остаётся историей, но в начало каждого добавляется строка о
том, что запись устарела, со ссылкой на новый ADR. Формат — по `ADR-FORMAT.md` скилла
`domain-modeling`.

**2. `ARCHITECTURE.md`** — самая большая часть работы:

| Место | Что не так |
|---|---|
| § Stack | строка `| Auth | JWT bearer + BCrypt password hashing |` — пакеты удалены тикетом 01 |
| § Current shape of the code | абзац «Two use cases exist, both about users: register and log in…» — в проекте не осталось ни одного use case. Сюда же добавить, где теперь живёт композиция DI: расширения по слоям (`AddProgramDependencies` → `AddWebDependencies` + `AddInfrastructure`), а `Program.cs` — только конвейер |
| § Content model | «That migration also creates the `users` table» — таблицы нет, миграция перегенерирована |
| § Known deviations | строка про `snake_case` **ложна**: конвенция настроена (`EFCore.NamingConventions` в пакетах, `UseSnakeCaseNamingConvention()` теперь в `AddInfrastructure`, в миграции `project_translations`, `created_at_utc`). Строки про JWT-адаптеры, про login как GET с телом и про фабрики-кортежи `(User user, string Error)` **потеряли предмет** — уходят |
| § Known problems | пункты «Authentication is not wired» и «A failure is an exception, not a result» уходят вместе с кодом, к которому они относились. «There is no seed data» остаётся |
| § What is deliberately absent | пункт про Authentication переписывается: кода идентичности больше нет, есть только будущая админка. Пункт про `CONTEXT.md` **ложен** — файл существует и наполнен |
| остальное | сверить таблицу стека, § Projects, § Testing с фактическим состоянием |

**3. `WORKFLOW.md`:** строка «The account endpoints are still not reachable» уходит. Добавить, что
в `appsettings.json` лежит пустой плейсхолдер `ConnectionStrings:DefaultConnection`, а старт
останавливается с понятным сообщением, если строка не задана.

**4. `README.md`:** «the database schema does not exist yet» — **ложь**, схема создаётся миграцией.

**5. `CONTEXT.md`:** в термине «Админка» убрать «It is the reason authentication exists in this
codebase at all» — авторизации в коде нет. Правка минимальная: определение панели остаётся.

**6. `.dsh/AGENTS.md`, § Project facts:** убрать `JWT bearer + BCrypt` из строки Backend stack;
убрать из «Later» открытый вопрос «Whether the existing users and JWT code becomes the panel's
foundation or is replaced» и упоминание JWT-секрета в `launchSettings.json` (секрет удалён).

**7. `frontend/README.md`:** говорит, что стек не решён, и приводит Vite — при том что ADR-0004 и
`.dsh/AGENTS.md` выбрали Next.js App Router, FSD и pnpm-монорепу. Привести в соответствие и
сослаться на ADR.

**Критерий готовности:** `git grep` по `JwtProvider`, `PasswordHasher`, `UsersService`,
`UserController`, `BCrypt`, `JwtBearer`, `LoginUserRequest` не находит совпадений в документах,
кроме каталога `docs/adr/` — там история, и её не вычищают. Ни один документ не утверждает ничего,
чего нет в коде.

## Answer

<!-- заполняется при закрытии -->
