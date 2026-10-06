# Задача для исполнителя: убрать из проекта авторизацию целиком

Ты работаешь в репозитории `H:\CSharp\MySite\MySite_dev` — личный сайт владельца: ASP.NET Core Web
API на .NET 10 плюс PostgreSQL, фронтенд ещё не начат. Репозиторий на GitHub, но **пушить будешь не
ты** (см. раздел «Git»).

## Прочитай перед началом — это не опционально

1. `.dsh/AGENTS.md` — правила проекта. Обязательны разделы **Backend** и **Toolchain**: они задают
   архитектуру, дисциплину анализаторов и то, как здесь собирают.
2. `WORKFLOW.md` — команды сборки и тестов и раздел «Definition of done».
3. `.scratch/site-development/issues/01-remove-identity-stack.md` — тикет этой задачи.
4. `docs/adr/0003-public-site-first-phase.md` — решение, часть которого ты отменяешь. Новый ADR
   писать **не нужно**: это отдельная задача.
5. `ARCHITECTURE.md` — текущее устройство проекта; в конце есть список известных отклонений.

Правила из `.dsh/AGENTS.md` важнее любых твоих привычек: это не «рекомендации», а контракт
репозитория.

## Задача

Владелец решил удалить всю старую авторизацию: к текущей задаче она не относится. Удаление
отменяет часть ADR-0003, поэтому новое решение будет записано отдельной задачей — **не тобой**.

Удалить ровно это, ничего не пропустив и ничего лишнего не тронув:

| Слой | Что удаляется |
|---|---|
| Domain | `Models/User.cs`, `Common/Enums/UserRight.cs` |
| Application | `Services/UsersService.cs`, `Interfaces/IUsersRepository.cs`, `Interfaces/Auth/IJwtProvider.cs`, `Interfaces/Auth/IPasswordHasher.cs` |
| Contracts | `LoginUserRequest.cs`, `RegisterUserRequest.cs` |
| Infrastructure | `Entities/UserEntity.cs`, `Persistence/Configurations/UserConfiguration.cs`, `Persistence/Repositories/UsersRepository.cs`, `Services/JwtProvider.cs`, `Services/PasswordHasher.cs`, `Services/JwtOptions.cs` |
| Web | `Controllers/UserController.cs` |
| Данные | `DbSet<UserEntity> Users` в `MySiteDbContext`, таблица `users` в миграции |
| Пакеты | `Microsoft.AspNetCore.Authentication.JwtBearer` и `BCrypt.Net-Next` — из `backend/Directory.Packages.props` |
| Секреты | JWT-секрет из `Properties/launchSettings.json` |

В `Program.cs` уходят регистрации, оставшиеся без потребителей: `IUsersRepository`, `IJwtProvider`,
`IPasswordHasher`, `UsersService`. Регистрация `IContentRepository` **остаётся**.

Из `Content.http` (или любого `.http` в проекте) удали запросы к `/api/v1/register` и
`/api/v1/login`, если они там есть.

**Обязано остаться нетронутым:** `Domain/Interfaces/IAuditable.cs` (его используют сущности
контента), `CorrelationIdMiddleware`, `PostgresHealthCheck`, `ContentController`,
`Persistence/LocaleConversion.cs`, файл профиля запуска в `launchSettings.json` (удаляется только
JWT-секрет внутри него).

## Миграция

**Править файлы миграции руками нельзя** — это прямое правило проекта. Единственная миграция
перегенерируется:

1. Удалить `20261006221740_ContentSchema.cs`, `20261006221740_ContentSchema.Designer.cs` и
   `MySiteDbContextModelSnapshot.cs`.
2. Из каталога `backend/` выполнить:

```powershell
dotnet ef migrations add ContentSchema -p src/MySite.Infrastructure.Postgres -s src/MySite.Web
```

Флаг `-s src/MySite.Web` обязателен: `dotnet ef` читает профиль запуска из стартового проекта.
Если `dotnet ef` недоступен или команда падает — **остановись и сообщи об этом**, не выдумывай
миграцию вручную и не копируй её из старой.

Локальная база после перегенерации станет несовместимой с томом docker (у миграции новый
идентификатор). **Docker не трогай и контейнеры не запускай** — это работа владельца; просто
напиши об этом в отчёте.

## Ветка и git

- Создай ветку от текущего `master`: `git checkout -b chore/remove-identity-stack`.
- Коммить **только в неё**. Сообщения коммитов — как в истории репозитория: по-русски, с
  префиксом (`feat:`, `chore:`, `docs:`).
- **Никогда не выполняй `git push`.** Не трогай `origin`, не делай `--force`, не перебазируй
  `master`, не создавай pull request, не трогай другие ветки. Пуш делает владелец сам.
- У `master` сейчас расходится история с `origin/master` (ahead 8, behind 1). **Это забота
  владельца, тебя она не касается**: ничего не «исправляй», ничего не сливай.
- Коммить только файлы своей задачи. Если в рабочем дереве лежат чужие незакоммиченные изменения —
  не добавляй их в свои коммиты.
- Каталог `.scratch/` тебе не принадлежит. Единственное исключение — в самом конце ты дописываешь
  ответ в файл **своего** тикета `.scratch/site-development/issues/01-remove-identity-stack.md`.
  Файл `.scratch/site-development/map.md` не редактируй.

## Критерий готовности

1. Из каталога `backend/`: `dotnet build MySite.slnx -m:1` → **0 ошибок и 0 предупреждений**.
   В песочнице флаг `-m:1` обязателен: многопроцессная сборка открывает именованные каналы, которые
   песочница блокирует.
2. Из каталога `backend/` (только оттуда, там лежит `global.json`): `dotnet test MySite.slnx`.
   Сегодня тестов нет, поэтому раннер сообщит «zero tests» и вернёт код 5 — **это ожидаемо и не
   ошибка**. Главное, что сборка тестовых проектов проходит.
3. `git grep` не находит ни одного упоминания удалённого: `UsersService`, `IUsersRepository`,
   `IJwtProvider`, `IPasswordHasher`, `JwtProvider`, `JwtOptions`, `UserEntity`, `UserRight`,
   `LoginUserRequest`, `RegisterUserRequest`, `BCrypt`, `JwtBearer`.
4. В `backend/.globalconfig` не появилось ни одной новой строки: глушить анализатор, чтобы сборка
   прошла, запрещено. Анализатор почти всегда прав — чини код.

Особенность этой машины: в режиме песочницы `workspace-write` команды `pwsh` падают с
`SetNamedSecurityInfoW failed (Win32 5): grantWrite(...)`. Если это случится — запроси расширение
доступа; это не ошибка твоей команды.

## Чего делать нельзя

- Править `ARCHITECTURE.md`, `README.md`, `WORKFLOW.md`, `CONTEXT.md`, `.dsh/AGENTS.md` — это
  отдельная задача. В этом изменении документация **намеренно остаётся устаревшей**, и это
  записано в карте.
- Писать ADR.
- Чинить Result-паттерн, переписывать `ContentController`, менять схему контента.
- Добавлять пакеты, менять версии, менять `.globalconfig`.
- Запускать docker, деплоить, пушить.

## Отчёт в конце

Верни коротко и по делу:

1. Имя ветки и хеши коммитов.
2. Список удалённых и изменённых файлов.
3. Дословный вывод `dotnet build` (строка про предупреждения и ошибки).
4. Вывод проверки `git grep`.
5. Что осталось за пределами задачи: в частности, что локальный docker-том надо пересоздать.
6. Всё, что оказалось неочевидным или пошло не по плану.

Затем допиши раздел `## Answer` в файле тикета 01 и поставь в нём `Status: resolved`.
