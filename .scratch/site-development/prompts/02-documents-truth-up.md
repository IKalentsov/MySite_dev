# Задача 02: привести документы в соответствие с репозиторием

**Сначала прочитай `.scratch/site-development/prompts/00-executor-brief.md`** — там стоячие
правила: как создавать ветку, почему нельзя пушить, какие скиллы загружать, какие команды
запускать, в каком виде сдавать отчёт. Дальше — только специфика этой задачи.

## Ветка

`docs/02-documents-truth-up`

## Скиллы, которые нужно загрузить до начала работы

1. **`architecture-drift-check`** — основной скилл этой задачи: он ровно про то, чтобы сверить
   кодовую базу с `ARCHITECTURE.md` и привести документ в актуальное состояние.
2. **`domain-modeling`** — для нового ADR и правки `CONTEXT.md`. У него в каталоге лежат
   `ADR-FORMAT.md` и `CONTEXT-FORMAT.md`; формат ADR берётся оттуда, а не выдумывается.

## Что делать

Полный чек-лист — в файле задачи
`.scratch/site-development/issues/02-adr-and-glossary-after-identity-removal.md`. Прочитай его
целиком, там семь пунктов и таблица расхождений по `ARCHITECTURE.md`. Коротко о сути:

- Написать новый ADR: identity-стек удалён целиком, что это отменяет в ADR-0003, при каком условии
  авторизация вернётся.
- Пометить ADR-0001 и ADR-0003 как устаревшие — **одной строкой в начале**, не переписывая их
  текст.
- Починить `ARCHITECTURE.md`: таблицу стека, § Current shape of the code, § Content model,
  § Known deviations, § Known problems, § What is deliberately absent.
- Починить `WORKFLOW.md`, `README.md`, `CONTEXT.md`, `.dsh/AGENTS.md` (§ Project facts) и
  `frontend/README.md`.

## На что смотреть особенно внимательно

**Часть расхождений не связана с удалением кода — они были ложными уже до него.** Владелец про них
знает, ты их просто чинишь:

- `ARCHITECTURE.md` § Known deviations объявляет `snake_case` ненастроенным. Настроен:
  `EFCore.NamingConventions` в `Directory.Packages.props`, `UseSnakeCaseNamingConvention()` в
  `AddInfrastructure`, в миграции `project_translations` и `created_at_utc`.
- `ARCHITECTURE.md` § What is deliberately absent объявляет `CONTEXT.md` несуществующим. Файл есть
  и наполнен.
- `README.md` утверждает «the database schema does not exist yet». Схема создаётся миграцией
  `20261006231005_ContentSchema`.
- `docs/adr/0002-migrations-removed.md` в Consequences говорит, что миграций нет и `snake_case` не
  настроен. И то и другое неверно.

**Также учти изменения, которых не было в исходном тикете:** композиция DI уехала из `Program.cs` в
расширения по слоям (`AddProgramDependencies` → `AddWebDependencies` + `AddInfrastructure`), health
checks разведены тегами `live`/`ready`, а в `appsettings.json` появился пустой плейсхолдер
`ConnectionStrings:DefaultConnection` со проверкой на старте. Это стоит отразить в `ARCHITECTURE.md`
и `WORKFLOW.md`.

## Границы: что тебе можно менять

**Ты выполняешь ровно один тикет — 02.** Никаких других тикетов карты, никаких «попутно заметил и
починил».

| Что можно | Что именно |
|---|---|
| `docs/adr/0006-<slug>.md` | **создать** — один новый ADR, номер следующий свободный |
| `docs/adr/0001-*.md`, `docs/adr/0003-*.md` | добавить по **одной строке** о том, что запись устарела, со ссылкой на новый ADR |
| `docs/adr/0002-migrations-removed.md` | поправить утверждения о текущем состоянии |
| `ARCHITECTURE.md`, `WORKFLOW.md`, `README.md`, `CONTEXT.md`, `frontend/README.md` | по чек-листу тикета |
| `.scratch/site-development/issues/02-adr-and-glossary-after-identity-removal.md` | `## Answer` и `Status: resolved` в самом конце |

**Запрещено, даже если кажется, что так будет лучше:**

- `.dsh/AGENTS.md` — правит планер, не ты. Это файл, который задаёт правила для всех агентов.
- `.dsh/skills/**`, `prompts/**`, `map.md`, любые другие файлы тикетов.
- `Directory.Build.props`, `Directory.Packages.props`, `.globalconfig`, `global.json`, `.gitignore` —
  конфигурация сборки и харнесса.
- **Любой файл под `backend/`** — код в этой задаче не меняется вообще.
- Новые файлы, кроме одного ADR.

Если для выполнения тикета тебе понадобилось что-то из запрещённого списка — **остановись и напиши об
этом в отчёте**. Рамки себе не расширяй.

## Метод

- **Не трогать код.** Это документационная задача. Если найдёшь расхождение «код врёт, а документ
  прав» — не правь код, напиши об этом в отчёте.
- **Не переписывать историю в `docs/adr/`.** Решения и рассмотренные альтернативы остаются как
  есть; меняются только утверждения о текущем состоянии, и то пометкой «устарело», а не
  замазыванием.
- **Не вычищать упоминания удалённого из `docs/adr/`.** Старые ADR ссылаются на `JwtProvider` и
  `PasswordHasher` законно — это история. Критерий ниже их исключает.
- Не добавлять ADR «на будущее» и не заводить документы помимо одного нового ADR.

## Критерий готовности

```
git grep -n -i -E 'JwtProvider|PasswordHasher|UsersService|UserController|BCrypt|JwtBearer|LoginUserRequest|RegisterUserRequest'
```

не находит совпадений **вне каталога `docs/adr/`**. Ни один документ не утверждает ничего, чего нет
в коде. Сборка при этом остаётся чистой: `dotnet build MySite.slnx -m:1` → 0 предупреждений, 0
ошибок (документы на сборку не влияют, но проверить надо — правило проекта).

## Отчёт

По формату из брифа, плюс отдельно перечисли:

1. Какие утверждения ты **удалил как ложные** и почему.
2. Какие файлы ты **сознательно не тронул**, хотя они упоминают удалённый код.
3. Всё, где документ и код расходятся, но чинить это — не твоя задача.
