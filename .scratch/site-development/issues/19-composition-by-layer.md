# 19 — Композиция DI по слоям вместо `Program.cs`

Type: task
Status: resolved
Blocked by: 01

## Question

Владелец потребовал убрать из `Program.cs` висящие там константы `LivenessCheck`, `ReadinessCheck`,
`ConnectionStringName` и регистрацию DI, и повторить композицию по образцу проекта DirectoryService
(`H:\CSharp\SachkovTech\DirectoryService\backend\DirectoryService\src`): расширения по слоям вместо
свалки в точке входа.

## Answer

**Сделано.** Ветка `chore/remove-identity-stack`, коммит `9d7c74a`, 8 файлов, +136/−37.

Появилось:

- `MySite.Infrastructure.Postgres/DependencyInjectionExtension.cs` — `AddInfrastructure(IConfiguration)`:
  константа `ConnectionStringName`, `AddDbContext` с `UseNpgsql` и `UseSnakeCaseNamingConvention()`,
  `AddScoped<IContentRepository, ContentRepository>()`.
- `MySite.Web/DependencyInjectionExtension.cs` — `AddWebDependencies()` (контроллеры, OpenAPI,
  ProblemDetails, health checks) и `AddProgramDependencies(configuration)` = Web + Infrastructure.
- `MySite.Web/HealthChecks/HealthCheckNames.cs` и `HealthCheckTags.cs` — имена и теги проверок.
- `Program.cs` — только сборка приложения, конвейер, маршруты, `RunAsync`.

**Решения и почему так:**

- **`AddDbContext` уехал в Infrastructure.** В DirectoryService он остался в `Program.cs`, но это
  ровно то, что владелец просил убрать. Вместе с ним уехал и `UseSnakeCaseNamingConvention()`.
- **Строка подключения — `ConnectionStrings:DefaultConnection`**, читается внутри `AddInfrastructure`;
  в `appsettings.json` добавлен пустой плейсхолдер (секции не было вовсе, хотя правило проекта
  требует структуру в `appsettings.json`, а значение — в Development-файле). Пустая строка
  останавливает старт с сообщением, где её взять.
- **Health checks разведены тегами** `live`/`ready`, а не именами: вторая readiness-проверка
  добавляется тегом, а не правкой предиката маршрута.
- **Пакеты объявлены явно:** `Microsoft.Extensions.Configuration.Abstractions` и
  `Microsoft.Extensions.DependencyInjection.Abstractions` версии **10.0.12** — ровно те, что уже
  приходили транзитивно (взято из `project.assets.json`, не выдумано).
- **Scrutor не добавлен:** в DirectoryService он в пакетах есть, но не используется.
- **`AddApplication()` не создан:** в Application нечего регистрировать, правило проекта запрещает
  заводить сущности «на будущее». Появится вместе с первым `IContentService`.
- **`app.UseAuthorization()` удалён:** ни `[Authorize]`, ни `AddAuthorization`, ни аутентификации в
  проекте не осталось.

**От их образца сознательно не скопировано:** `DirectoryService.Web/DependencyInjectionExtension.cs`
объявляет `namespace DirectoryService.Core;`, хотя лежит в Web. Здесь неймспейс по месту.

**Проверка:** `dotnet build MySite.slnx -m:1` → 0 предупреждений, 0 ошибок. Первый прогон упал на
`CS1061` — `Program.cs` с top-level statements живёт в глобальном пространстве имён и не видел
extension-метод из `MySite.Web`; лечится `using MySite.Web;`.
