# 01 — Удалить identity-стек и перегенерировать миграцию

Type: task
Status: resolved

## Question

Владелец решил удалить всю старую авторизацию: она к текущей задаче не относится. Это отменяет
часть ADR-0003, который как раз отклонял удаление («код уже есть и является семенем админки»),
поэтому новое решение записывается отдельно — тикет 02.

Что уходит из репозитория:

| Слой | Что именно |
|---|---|
| Domain | `Models/User.cs`, `Common/Enums/UserRight.cs` |
| Application | `Services/UsersService.cs`, `Interfaces/IUsersRepository.cs`, `Interfaces/Auth/IJwtProvider.cs`, `Interfaces/Auth/IPasswordHasher.cs` |
| Contracts | `LoginUserRequest.cs`, `RegisterUserRequest.cs` |
| Infrastructure | `Entities/UserEntity.cs`, `Persistence/Configurations/UserConfiguration.cs`, `Persistence/Repositories/UsersRepository.cs`, `Services/{JwtProvider,PasswordHasher,JwtOptions}.cs` |
| Web | `Controllers/UserController.cs` |
| Данные | `DbSet<UserEntity> Users`, таблица `users` в миграции |
| Пакеты | `Microsoft.AspNetCore.Authentication.JwtBearer`, `BCrypt.Net-Next` из `Directory.Packages.props` |
| Секреты | JWT-секрет в `Properties/launchSettings.json` |

Миграция перегенерируется, а не правится руками: удалить `20261006221740_ContentSchema` вместе с
`Designer` и снапшотом, затем `dotnet ef migrations add ContentSchema` заново. Данных нет,
локальная база одноразовая — цена нулевая.

`IAuditable` остаётся: его используют сущности контента. `CorrelationIdMiddleware`,
`PostgresHealthCheck` и `ContentController` не затрагиваются.

## Answer

Ветка: `chore/remove-identity-stack`
Коммит: `9105f91` — "chore: удалить identity-стек (JWT, BCrypt, UserEntity, UsersService) и перегенерировать миграцию"

Удалено 15 файлов:
- `backend/src/MySite.Domain/Models/User.cs`
- `backend/src/MySite.Domain/Common/Enums/UserRight.cs`
- `backend/src/MySite.Application/Services/UsersService.cs`
- `backend/src/MySite.Application/Interfaces/IUsersRepository.cs`
- `backend/src/MySite.Application/Interfaces/Auth/IJwtProvider.cs`
- `backend/src/MySite.Application/Interfaces/Auth/IPasswordHasher.cs`
- `backend/src/MySite.Contracts/LoginUserRequest.cs`
- `backend/src/MySite.Contracts/RegisterUserRequest.cs`
- `backend/src/MySite.Infrastructure.Postgres/Entities/UserEntity.cs`
- `backend/src/MySite.Infrastructure.Postgres/Persistence/Configurations/UserConfiguration.cs`
- `backend/src/MySite.Infrastructure.Postgres/Persistence/Repositories/UsersRepository.cs`
- `backend/src/MySite.Infrastructure.Postgres/Services/JwtProvider.cs`
- `backend/src/MySite.Infrastructure.Postgres/Services/PasswordHasher.cs`
- `backend/src/MySite.Infrastructure.Postgres/Services/JwtOptions.cs`
- `backend/src/MySite.Web/Controllers/UserController.cs`

Изменено 5 файлов:
- `backend/Directory.Packages.props` — удалена группа "Identity and security" (JwtBearer, BCrypt.Net-Next)
- `backend/src/MySite.Infrastructure.Postgres/MySite.Infrastructure.Postgres.csproj` — удалены PackageReference на BCrypt.Net-Next и JwtBearer
- `backend/src/MySite.Infrastructure.Postgres/Persistence/MySiteDbContext.cs` — удалён `DbSet<UserEntity> Users` и using для `Entities`
- `backend/src/MySite.Web/Program.cs` — удалены DI-регистрации IUsersRepository, IJwtProvider, IPasswordHasher, UsersService и соответствующие using; оставлена IContentRepository
- `backend/src/MySite.Web/Properties/launchSettings.json` — удалён блок `JwtOptions`

Перегенерирована миграция:
- Удалена старая: `20261006221740_ContentSchema.cs`, `.Designer.cs`, `MySiteDbContextModelSnapshot.cs`
- Создана новая: `20261006231005_ContentSchema.cs` (без таблицы `users`)

Сборка: `dotnet build MySite.slnx -m:1` → "Сборка успешно завершена. Предупреждений: 0, Ошибок: 0"
Тесты: `dotnet test MySite.slnx` → «Запущено ноль тестов» в каждом из трёх проектов, код завершения **5**. (В первой редакции ответа стояло «8» — это неверно; проверено повторно, 5 — ожидаемое поведение `Microsoft.Testing.Platform` при нуле тестов, см. `WORKFLOW.md`.)
git grep по всем удалённым именам → ни одного совпадения.

`.globalconfig` не изменён. `Content.http` не содержит запросов к `/api/v1/register` или `/api/v1/login` — не тронут.

Осталось за пределами задачи:
- ADR-0003 и `ARCHITECTURE.md` не обновлены — это отдельная задача (тикет 02).
- Docker-том с PostgreSQL нужно пересоздать: миграция имеет новый идентификатор (`20261006231005` вместо `20261006221740`), старый том несовместим.
- `backend/.env` и `appsettings.Development.json` с PostgreSQL-учётными данными не тронуты.