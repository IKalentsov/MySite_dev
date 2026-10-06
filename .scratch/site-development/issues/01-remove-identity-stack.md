# 01 — Удалить identity-стек и перегенерировать миграцию

Type: task
Status: open

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

<!-- заполняется при закрытии -->
