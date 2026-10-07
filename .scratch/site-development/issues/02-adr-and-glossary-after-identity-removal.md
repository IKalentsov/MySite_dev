# 02 — Записать удаление identity и привести документы в соответствие с репозиторием

Type: task
Status: resolved
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

**6. `.dsh/AGENTS.md` — правит планер, не исполнитель.** Файл задаёт правила для всех агентов, и его
правки в задачу исполнителя не входят: строка `Backend stack` (убрать `JWT bearer + BCrypt`) и два
пункта в § Open questions → «Later» про users/JWT-код и про JWT-секрет в `launchSettings.json`.
**Исполнитель этот файл не трогает вовсе.**

**7. `frontend/README.md`:** говорит, что стек не решён, и приводит Vite — при том что ADR-0004 и
`.dsh/AGENTS.md` выбрали Next.js App Router, FSD и pnpm-монорепу. Привести в соответствие и
сослаться на ADR.

**Критерий готовности:** `git grep` по `JwtProvider`, `PasswordHasher`, `UsersService`,
`UserController`, `BCrypt`, `JwtBearer`, `LoginUserRequest` не находит совпадений в документах,
кроме каталога `docs/adr/` — там история, и её не вычищают. Ни один документ не утверждает ничего,
чего нет в коде.

## Answer

Выполнено. Ветка `docs/02-documents-truth-up`, коммит `91570ed`.

### Что сделано

**Новый ADR:** `docs/adr/0006-identity-stack-removed.md` — зафиксировано решение удалить
identity-стек целиком, что отменяет ADR-0003 (код, который нужно было хранить «спящим», больше не
существует), и описывает условие возвращения авторизации (первый write-сценарий админки).

**Устаревшие ADR:** в начало `0001-single-infrastructure-project.md` и
`0003-public-site-first-phase.md` добавлена одна строка о том, что запись устарела, со ссылкой на
ADR-0006. Текст не переписан.

**ADR-0002:** последствия исправлены — миграция сгенерирована (`20261006231005_ContentSchema`),
`snake_case` настроен через `EFCore.NamingConventions`.

**ARCHITECTURE.md:**
- Удалена строка Auth из таблицы стека; в Data добавлен `EFCore.NamingConventions` v10.0.1.
- Переписан § Current shape of the code: нет use case'ов для пользователей, DI-композиция через
  расширения по слоям (`AddProgramDependencies` → `AddWebDependencies` + `AddInfrastructure`),
  `Program.cs` — только конвейер; health checks разведены тегами.
- Удалена ссылка на таблицу `users` в § Content model.
- Из Known deviations удалены 4 строки: JWT-адаптеры в Postgres-проекте, snake_case не настроен,
  login как GET с телом, фабрики возвращают кортеж. Остались 3 актуальные (Result<T> для контента,
  plain Guid, IContentRepository без service layer).
- Из Known problems удалены 2 строки: «Authentication is not wired» и «A failure is an exception,
  not a result». Осталась одна: «There is no seed data».
- В § What is deliberately absent переписан пункт про Authentication (кода нет, будет написан с
  нуля), удалён пункт про `CONTEXT.md` (файл существует).

**WORKFLOW.md:** строка «The account endpoints are still not reachable» заменена на описание
плейсхолдера `ConnectionStrings:DefaultConnection` и проверки при старте.

**README.md:** «the database schema does not exist yet» → схема создаётся миграцией.

**CONTEXT.md:** из определения «Админка» убрана фраза «It is the reason authentication exists in
this codebase at all»; добавлена ссылка на ADR-0006.

**frontend/README.md:** стек Vite заменён на Next.js App Router + FSD согласно ADR-0004; убрano
упоминание нерешённого стека и harness base.

### Утверждения, удалённые как ложные

1. **«snake_case ненастроен»** (ARCHITECTURE.md Known deviations) — настроен: `EFCore.NamingConventions`
   в `Directory.Packages.props`, `UseSnakeCaseNamingConvention()` в `AddInfrastructure`, миграция
   `project_translations` и `created_at_utc` используют snake_case.
2. **`CONTEXT.md` не существует** (ARCHITECTURE.md What is deliberately absent) — файл есть и
   наполнен терминами.
3. **«the database schema does not exist yet»** (README.md) — схема создаётся миграцией
   `20261006231005_ContentSchema`.
4. **Mиграций нет** (ADR-0002 Consequences) — миграция есть и применяется.
5. **`snake_case` не настроен** (ADR-0002 Consequences) — настроен через `EFCore.NamingConventions`.
6. **«Authentication is not wired»** (ARCHITECTURE.md Known problems) — кода идентичности больше нет,
   вопрос снят.
7. **«A failure is an exception, not a result»** (ARCHITECTURE.md Known problems) — относится к
   удалённому `UsersRepository`/`UsersService`.

### Файлы, сознательно не тронутые

- **`docs/adr/0001-single-infrastructure-project.md`** и **`docs/adr/0003-public-site-first-phase.md`** —
  основной текст оставлен как история; добавлена только одна строка устаревания в начало.
- **`.dsh/AGENTS.md`** (§ Project facts, § Open questions) — правит планер, не исполнитель.
- **`.scratch/`** — каталог задач и промптов принадлежит планеру.

### Расхождения документ ↔ код, не вошедшие в задачу

Нет: все найденные расхождения учтены и исправлены в рамках этого тикета. Сборка чистая:
`dotnet build MySite.slnx -m:1` → 0 предупреждений, 0 ошибок.

### Критерий готовности

```
git grep -n -i -E 'JwtProvider|PasswordHasher|UsersService|UserController|BCrypt|JwtBearer|LoginUserRequest|RegisterUserRequest' ':!.scratch/'
```

находит совпадения только в `docs/adr/` (история, допустимо).

### Проверка и закрытие (планер)

**Ветка `docs/02-documents-truth-up`, коммит `91570ed` — проверен и принят.** Критерий выполнен: 6
совпадений, все в `docs/adr/`. Сборка воспроизведена: `dotnet build MySite.slnx -m:1` → 0
предупреждений, 0 ошибок. Содержание документов сверено с кодом, а не только с текстом задания:
DI-расширения, `UseSnakeCaseNamingConvention()`, плейсхолдер строки подключения, состав таблиц
миграции `20261006231005_ContentSchema`, отсутствие таблицы `users` — всё сходится.

**Два расхождения, найденные проверкой:**

1. `ARCHITECTURE.md` § Current shape of the code называл два response DTO, тогда как в
   `MySite.Contracts/Content/` их три (`OwnerProfileResponse`, `ProjectResponse`,
   `ProjectListResponse`).
2. `.dsh/AGENTS.md` § Project facts в строке `First phase` ссылался на ADR-0003 как на живое
   основание, хотя этот же тикет пометил ADR-0003 отменённым.

Оба исправлены в том же заходе, коммит `b0cd124` (ветка `docs/02-documents-truth-up`) — путь `fix`
по дисциплине ревью, `.dsh/AGENT-TASKS.md` § 12. Отдельного тикета-доработки не требуется.

**Не вошло и не должно:** пункт 6 задания (правки в `.dsh/AGENTS.md`, § Project facts и
§ Open questions) — правит планер; строка про JWT/BCrypt и два пункта про users/JWT в § Later
отсутствовали на момент проверки, то есть на момент написания задания уже были убраны.

Status: resolved
