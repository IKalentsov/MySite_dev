# 08 — Залить заглушки контента

Type: task
Status: resolved
Blocked by: 04

## Как работать с этой задачей

**Правила работы с проектом — `.dsh/AGENT-TASKS.md`.** Прочитай его первым, до первой правки: там
команды, правила веток, среда и критерий готовности. Ниже — только специфика этой задачи.

- Ветка: `feature/seed-placeholder-content` от текущего `master`.
- Коммить в неё; **не пушить** и не трогать чужие задачи.
- Эта задача трогает **данные**, а не схему: миграция не генерируется. Но база должна быть поднята
  и накатана — `docker compose up -d`, `pg_isready`, `dotnet ef migrations list` без `(Pending)`.
- Повторный запуск сидирования обязан быть идемпотентным: прогони его дважды и покажи, что число
  строк не изменилось.

**Условие остановки:** Профиль и 3–5 проектов лежат в локальной базе; один проект намеренно без английского перевода, один снят с публикации; повторный запуск сидирования даёт тот же счёт строк.

## Question

Чтобы страница не была пустой и фронтенд можно было строить против живых данных, в базу
заливается временный контент: профиль и три-пять проектов, обе локали.

Требования:

- Источник — реальные репозитории владельца, а не выдуманные проекты: `DirectoryService`,
  `MySite`/`MySite_dev`, Harness и подобные. Описания черновые.
- Заглушки явно помечены, чтобы их нельзя было перепутать с финальными текстами тикета 07.
- Один проект намеренно оставлен **без английского перевода** — проверка fallback из ADR-0005 на
  живых данных.
- Один проект снят с публикации (`is_published = false`) — проверка, что он не попадает в выдачу.
- Повторный запуск сидирования не создаёт дубликатов.

## Answer

**Что сделано.** Реализовано решение тикета 04 — команда сидирования из ADR-0007 — и этим же PR в
локальную базу залиты заглушки: профиль и четыре проекта. Способ заливки, формат файла и правило
идемпотентности теперь не только записаны, но и работают.

Слои — как требует ADR: разбор аргументов в `Program.cs` (`seed` до старта веб-хоста, коды возврата
`0`/`1`/`2`), чистая валидация в `Application`, upsert в одной транзакции в `Infrastructure.Postgres`.
Профиль — синглтон: создаётся на пустой таблице, дальше обновляется на месте. Проект опознаётся по
`link`; его переводы и стек становятся ровно тем, что в файле; проект, которого в файле нет, не
удаляется, а попадает в отчёт. Замена строк идёт на месте, без delete+insert — иначе в одном
`SaveChanges` конфликтуют уникальные индексы `(project_id, locale)` и `(project_id, position)`.

Контент в `backend/content/site-content.json` (все тексты помечены `Заглушка:`/`Placeholder:`, чтобы
их нельзя было спутать с текстами тикета 07):

| Проект | Ссылка | Год | Публикация | Локали |
|---|---|---|---|---|
| DirectoryService | `github.com/IKalentsov/SachkovTech/tree/master/DirectoryService` | 2026 | да | ru, en |
| MySite | `github.com/IKalentsov/MySite_dev` | 2026 | да | ru, en |
| Harness | `github.com/IKalentsov/Harness` | 2026 | да | **только ru — намеренно** |
| Keycloak_Learn | `github.com/IKalentsov/Keycloak_Learn` | 2025 | **нет — намеренно** | ru, en |

**Чем подтверждено** (дословно).

Сборка:

```text
Сборка успешно завершена.
    Предупреждений: 0
    Ошибок: 0
```

Юнит-тесты (прямой запуск сборки):

```text
=== TEST EXECUTION SUMMARY ===
   MySite.UnitTests  Total: 15, Errors: 0, Failed: 0, Skipped: 0, Not Run: 0, Time: 0,255s
```

`dotnet test MySite.slnx` — 15 тестов пройдено, но общий код возврата **8**: проект `MySite.UnitTests`
пройден, а `MySite.ArchitectureTests` и `MySite.IntegrationTests` сообщают «запущено ноль тестов».
Это записанное отклонение (`WORKFLOW.md`, «Deviations», и `.dsh/AGENT-TASKS.md` §11); закрывает его
тикет 06. Прямой запуск тестовой сборки — код 0.

Сидирование, первый прогон (база была пуста):

```text
Seeding from 'H:\CSharp\MySite\MySite_dev\backend\content\site-content.json'.
Profile: created.
Projects: created 4, updated 0, unchanged 0.
Translation gaps (deliberate): https://github.com/IKalentsov/Harness (en)
Projects in the database but not in the file: none
```

Идемпотентность — второй прогон того же файла и счётчики строк до/после:

```text
Seeding from 'H:\CSharp\MySite\MySite_dev\backend\content\site-content.json'.
Profile: unchanged.
Projects: created 0, updated 0, unchanged 4.
```

```text
 profiles | profile_tr | projects | project_tr | stack
----------+------------+----------+------------+-------
        1 |          2 |        4 |          7 |    20
```

Эти же цифры (`1 / 2 / 4 / 7 / 20`) получены после первого прогона и после второго — дубликатов нет.
Сходимость проверена не только на «ничего не изменилось»: временно правил файл (менял заголовок,
убирал `en` у проекта, добавлял и убирал элемент стека) — каждая правка давала `updated 1..2`,
строки и значения сходились обратно, после восстановления файла снова `unchanged 4` и те же счётчики.

Коды возврата проверены все три: `0` — засеяно; `1` — нет файла и неверный контент
(`The profile contains an unknown locale 'de'. Supported locales are: ru, en.`); `2` — недоступная база
(`The seed failed while writing to the database: Failed to connect to 127.0.0.1:5999.`).

Живые данные через API (`dotnet run --project src/MySite.Web`, затем запросы):

- `GET /api/v1/projects?locale=en` — три опубликованных проекта; Harness отдаётся с **русским**
  заголовком, то есть fallback ADR-0005 работает на живых данных;
- Keycloak_Learn (`isPublished: false`) в выдаче отсутствует;
- `GET /api/v1/profile?locale=en` — английский заголовок профиля.

Схема не менялась: миграций нет, `dotnet ef migrations list` показывает только применённую
`20261006231005_ContentSchema`, `(Pending)` нет.

**Файлы, ветка, коммиты.**

Ветка `feature/seed-placeholder-content` от `develop` (`f12187f`, хеши локального и `origin/develop`
совпали перед созданием ветки). Коммиты: `c5f973b` (команда, домен, Application, Infrastructure, Web,
тесты), `fc5298e` (контент), `2b2cece` (документы).

Создано: `backend/src/MySite.Application/Content/Seeding/` (11 файлов — DTO файла, парсер, валидатор,
план, writer-абстракция, результат, сервис), `backend/src/MySite.Domain/Content/ProjectText.cs`,
`ProfileText.cs`, `backend/src/MySite.Infrastructure.Postgres/Persistence/Repositories/ContentSeedWriter.cs`,
`backend/src/MySite.Web/Seeding/SeedCommand.cs`, `backend/content/site-content.json`,
`backend/content/README.md`, `backend/tests/MySite.UnitTests/Content/Seeding/SeedContentServiceTests.cs`,
`backend/tests/MySite.UnitTests/Domain/Content/{ProjectTests,OwnerProfileTests}.cs`.
Изменено: `Project.cs`, `OwnerProfile.cs`, `ProjectTranslation.cs`, `OwnerProfileTranslation.cs`,
`ProjectStackItem.cs`, оба `DependencyInjectionExtension.cs`, `Program.cs`,
`backend/tests/MySite.UnitTests/MySite.UnitTests.csproj` (копирование файла контента в вывод тестов),
`ARCHITECTURE.md`, `WORKFLOW.md`, `README.md`. Удалённых файлов нет.

**Что осталось за рамками и что требует решения владельца.**

1. **Пример команды в ADR-0007 не работает как записан.** `dotnet run` запускает приложение с рабочим
   каталогом `src/MySite.Web`, поэтому `--file content/site-content.json` из `backend/` ищет файл в
   `backend/src/MySite.Web/content/`. Правило ADR соблюдено — путь разрешается от текущего каталога
   процесса, не от content root, — но в `WORKFLOW.md` и `backend/content/README.md` записана работающая
   форма: `--file ../../content/site-content.json` (годится и абсолютный путь). Текст ADR не правил:
   это решение владельца.
2. **У `ContentSeedWriter` нет автоматического теста.** Его путь проверен вживую (двойной прогон и
   сходимость выше), но контур интеграционных тестов — тикет 06.
3. **`ARCHITECTURE.md` называет DTO `ProjectListResponse`,** а класс — `PaginatedProjectsResponse`.
   Расхождение было до этого PR, не правил — вне объёма задачи.
4. **Ветка создана от `develop`, а не от `master`,** как написано в шапке тикета: контракт (§1, §3)
   главнее файла задачи, так же поступил тикет 04.
5. **Тест на поставляемый файл** проверяет «3–5 проектов, ровно один без `en`, ровно один
   неопубликованный». Когда тикет 07 заменит тексты и, возможно, состав проектов, эти проверки надо
   пересмотреть вместе с ним.

Pull request в `develop`: https://github.com/IKalentsov/MySite_dev/pull/12

