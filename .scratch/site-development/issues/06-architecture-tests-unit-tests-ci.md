# 06 — Архитектурные тесты, юнит на локаль, CI бэкенда

Type: task
Status: resolved
Blocked by: 01

## Как работать с этой задачей

**Правила работы с проектом — `.dsh/AGENT-TASKS.md`.** Прочитай его первым, до первой правки: там
команды, правила веток, среда и критерий готовности. Ниже — только специфика этой задачи.

- Ветка: `feature/architecture-tests-unit-tests-ci` от текущего `master`.
- Коммить в неё; **не пушить** и не трогать чужие задачи.
- Здесь особенно важно: **тесты — часть критерия готовности, а не приложение к ней**. Первый
  настоящий тест в проекте появляется именно этой задачей, а вместе с ним `dotnet test
  MySite.slnx` перестаёт возвращать «ноль тестов» и обязан возвращать код 0 при ненулевом числе
  пройденных тестов.
- Накатывание миграций и докер в этой задаче не нужны: схема не меняется.

**Условие остановки:** Архитектурная фитнес-функция с негативной фикстурой и юнит на правило локали зелёные, CI прогоняет сборку и тесты из `backend/`, `dotnet test MySite.slnx` возвращает код 0 при ненулевом числе пройденных тестов.

## Question

Три тест-проекта — пустые скелеты, а definition of done требует покрытия. Первый срез:

- **Архитектурные фитнес-функции** (`MySite.ArchitectureTests`, ArchUnitNET уже в пакетах):
  направление зависимостей `Domain` ← `Application` ← `Infrastructure.Postgres` ← `Web`,
  отсутствие ссылок из `Domain` куда-либо, запрет `async void`, обязательные суффиксы. Правила
  проекта требуют **негативную фикстуру** — сборку, на которой проверка обязана упасть; без неё
  проверка не считается работающей.
- **Юнит на правило локали** (`MySite.UnitTests`): `TranslationFor(locale)` отдаёт текст
  запрошенной локали; при её отсутствии — текст локали по умолчанию; при отсутствии обоих —
  ничего. Правило записано в ADR-0005 и живёт в коде без единого теста. Формат — `[Theory]` по
  локалям, без дублирования тел.
- **CI**: `.github/workflows/` с шагами restore → build → test из `backend/`, .NET 10. Учесть, что
  `dotnet test` на пустом наборе возвращает код 5; после появления тестов это уходит. Проверить,
  что `TreatWarningsAsErrors` действительно валит CI, а не проходит.

Интеграционные тесты с реальным PostgreSQL (Testcontainers) — отдельный тикет позже, когда будет
что защищать: сейчас контент читается, но не пишется.

## Answer

## Answer

**Сделано.** Архитектурный проект (`MySite.ArchitectureTests`, ArchUnitNET 0.13.4) проверяет
направление зависимостей (`Domain` — только себя и BCL; `Application` не видит
`Infrastructure`/`Web`; `Infrastructure` не видит `Web`; `Contracts` — только BCL), запрет
`async void` и обязательные суффиксы (интерфейсы — `I`, контроллеры — `Controller`, адаптеры —
`Repository`/`Writer`). К каждому правилу есть негативная фикстура — отдельная сборка
`MySite.ArchitectureTests.NegativeFixture` в `MySite.slnx`, на которой то же правило обязано упасть.
Юнит `TranslationForTests` — `[Theory]` по локалям для `Project` и `OwnerProfile`: своя локаль,
fallback на `ru`, `null` при отсутствии обеих. Интеграционные тесты (Testcontainers.PostgreSql
4.16.0, `postgres:18.6-alpine`) поднимают контейнер, накатывают миграции и через production DI
покрывают `SeedContentService` + `ContentSeedWriter`: первый прогон создаёт профиль и проекты,
второй ничего не меняет, добавленная/убранная локаль сходится, стек растёт и сокращается, проект
вне файла не удаляется и попадает в отчёт; состояние изолируется `TRUNCATE` перед каждым тестом.
CI — `.github/workflows/backend.yml`: push/PR в `develop` и `master`, checkout → setup-dotnet
10.0.401 → restore → build → test из `backend/`. Документы приведены в соответствие: ADR-0007
(рабочий каталог `dotnet run` — `src/MySite.Web`, поэтому путь
`../../content/site-content.json`), `ARCHITECTURE.md` (`PaginatedProjectsResponse`, нет «пустых
скелетов» и «первого, что надо заполнить», CI больше не в «намеренно отсутствующем»), `README.md`,
`WORKFLOW.md`.

**Чем подтверждено** (из `backend/`):

```text
dotnet build MySite.slnx -m:1
    Предупреждений: 0
    Ошибок: 0
код возврата 0

dotnet test MySite.slnx
  итог: 43
  сбой: 0
  успешно: 43
  пропущено: 0
код возврата 0
```

43 = 22 unit (15 прежних + 7 на локаль) + 16 architecture (8 правил + 8 негативных) + 5 integration.
Негативная фикстура проверена поломкой: при временно ослабленном правиле
`ApplicationDoesNotDependOnInfrastructureOrWeb` тест
`ApplicationDependencyRuleFailsOnTheNegativeFixture` покраснел с `Assert.Throws() Failure: No
exception was thrown`, после возврата правила — снова зелёный. `TreatWarningsAsErrors` проверен
временным `#warning`: сборка упала с `error CS1030`, код 1; после удаления — снова 0 предупреждений.
Интеграционные тесты прогнаны дважды (отдельной сборкой и в составе `dotnet test`) — зелёные.
CI: PR https://github.com/IKalentsov/MySite_dev/pull/13, прогон
https://github.com/IKalentsov/MySite_dev/actions/runs/43770452576 — `Backend / Build and test`,
шаги Restore, Build и Test зелёные.

**Ветка и коммиты:** `feature/architecture-tests-unit-tests-ci` от `develop` (`3bed360`):
`6cc0d1c` архитектурные тесты с негативной фикстурой, `23564f2` юнит на локаль и интеграционные
тесты сидирования, `d194116` CI, `923267c` документы.

**Файлы:** новые — `backend/tests/MySite.ArchitectureTests/{ArchitectureRules,TestArchitectures,
DependencyDirectionTests,ConventionTests,NegativeFixtureTests}.cs`, проект
`backend/tests/MySite.ArchitectureTests.NegativeFixture/` (6 файлов + `.csproj`),
`backend/tests/MySite.UnitTests/Domain/Content/TranslationForTests.cs`,
`backend/tests/MySite.IntegrationTests/{PostgresContainerFixture,PostgresCollection,SeedFiles,
SeedContentTests}.cs`, `.github/workflows/backend.yml`; изменены — `backend/MySite.slnx`,
`backend/Directory.Packages.props` (единственная новая зависимость `Testcontainers.PostgreSql`
4.16.0), оба тестовых `.csproj`, `ARCHITECTURE.md`, `README.md`, `WORKFLOW.md`, ADR-0007.

**За рамками и находки.** HTTP-слой интеграционно не покрыт: `WebApplicationFactory` /
`Microsoft.AspNetCore.Mvc.Testing` вне объёма по решению владельца, покрыт путь сидирования. В
`MySite.Contracts` остался неиспользуемый `ProjectListResponse` — документация теперь называет
реально возвращаемый `PaginatedProjectsResponse`, удаление мёртвого DTO в объём не входит.
Негативная фикстура собирается с выключенными анализаторами только она сама (иначе `async void`
не скомпилировать); production-код остаётся под полным набором правил, `.globalconfig` не тронут.
Интеграционным тестам нужен работающий Docker, в том числе на CI — это отмечено в `WORKFLOW.md`.
Цифра «5» из текста тикета устарела: на пустых проектах `dotnet test` возвращал 8, теперь — 0 при
43 тестах.

**Устаревшие строки журналов (не редактировались, это планер).** `.dsh/AGENT-TASKS.md` §5 —
«Пока в проекте нет тестов, `dotnet test` сообщает „Запущено ноль тестов“ и возвращает код 8»;
§11, строка 2026-10-07 «Критерий „зелёные тесты“ нельзя проверить…» (условие возврата выполнено) и
строка 2026-10-07 «`git fetch`/`git pull` у `origin` недоступны» (с ключом `~/.ssh/my_ssh` fetch
работает). `WORKFLOW.md`, «Deviations from the general rules» — те же две строки: про «green tests»
и про недоступный fetch.

