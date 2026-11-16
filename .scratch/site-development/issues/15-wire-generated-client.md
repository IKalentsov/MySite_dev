# 15 — Подключить сгенерированный клиент

Type: task
Status: resolved
Blocked by: 11, 14, 18

## Как работать с этой задачей

**Правила работы с проектом — `.dsh/AGENT-TASKS.md`.** Прочитай его первым, до первой правки: там
команды, правила веток, среда и критерий готовности. Ниже — только специфика этой задачи.

- Ветка: `feature/wire-generated-client` от свежего `develop`; push ветки и PR в `develop` — работа
  исполнителя, слияние — владельца.
- Критерий готовности: клиент сгенерирован в `packages/api-client` и экспортирован через публичный
  API пакета; `getHomeContent` (решение тикета 18) вызывает его публичные функции и отдаёт
  `profile` и `projects` с локального API; проверка расхождения в CI ловит правку схемы
  (перегенерировать → `git diff` не пуст → падение); гейты фронтенда зелёные. Страницу собирает
  тикет 16.
- Схема производится на сборке бэкенда (`Microsoft.Extensions.ApiDescription.Server`), см. раздел
  «Уточнения» ниже; запущенный локальный API нужен только как резервный путь, если build-time
  генерация не запустится. Запускать приложение «навсегда» не требуется.

**Условие остановки:** Клиент сгенерирован и экспортирован; `getHomeContent` отдаёт `profile` и
`projects` с локального API; проверка расхождения схемы в CI падает на правке OpenAPI; гейты
фронтенда зелёные. Страница целиком — тикет 16.

## Question

Взять решение тикета 11 и встроить его в монорепу:

- Генерация клиента в `packages/api-client` из закоммиченной OpenAPI-схемы бэкенда
  (`backend/docs/openapi.json`, производится на сборке).
- Скрипт регенерации и проверка расхождения в CI (перегенерировать → `git diff` пуст).
- Экспорт типизированных функций через публичный API пакета; сгенерированные файлы помечены как
  generated и не правятся руками.
- ESLint-правило, запрещающее UI импортировать сгенерированное напрямую (правило проекта: UI
  работает только через публичный API слайса).
- Минимальная проверка, что клиент действительно достаёт `profile` и `projects` с локального API
  на `http://localhost:5054`.

Критерий: `getHomeContent` получает данные через сгенерированный клиент, а не через `fetch` по
строке; страница целиком — тикет 16.

## Уточнения после исследования 11 (2026-11-16)

Решение 11: **`openapi-typescript` + `openapi-fetch`**, схема генерируется на сборке бэкенда и
коммитится. Ниже — что это меняет в этой задаче.

### Инструмент и версии

| Пакет | Версия | Где |
|---|---|---|
| `openapi-typescript` | **7.13.0** | devDependency `packages/api-client` |
| `openapi-fetch` | **0.17.0** | dependency `packages/api-client` |
| `openapi-typescript-helpers` | 0.1.0 | транзитивная зависимость `openapi-fetch`, отдельно не пинуется |
| `Microsoft.Extensions.ApiDescription.Server` | **10.0.12** | `backend/Directory.Packages.props` + `MySite.Web.csproj` |

`openapi-typescript@7.13.0` объявляет peer `typescript ^5.x`, у проекта TS 6.0.3. Сгенерированные
типы под 6.0.3 компилируются (проверено в 11), но upstream ещё не расширил диапазон (issue
#2723). Поэтому в `frontend/pnpm-workspace.yaml` добавляется разрешение рядом с существующим
`eslint-plugin-import>eslint`:

```yaml
peerDependencyRules:
  allowedVersions:
    "eslint-plugin-import>eslint": "10"
    "openapi-typescript>typescript": "6"
```

Это разрешение устаревшего peer-объявления, а не отключение правила. Условие пересмотра — релиз
`openapi-typescript` с `^6` в peer или переход на TypeScript 7.

### Схема: build-time, а не запущенный API

1. В `backend/Directory.Packages.props` добавить `Microsoft.Extensions.ApiDescription.Server`
   **10.0.12** (в одну строку с `Microsoft.AspNetCore.OpenApi` 10.0.12).
2. В `MySite.Web.csproj`:

   ```xml
   <PackageReference Include="Microsoft.Extensions.ApiDescription.Server">
     <PrivateAssets>all</PrivateAssets>
     <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
   </PackageReference>
   ```

   и свойства:

   ```xml
   <OpenApiGenerateDocumentsOptions>--file-name openapi</OpenApiGenerateDocumentsOptions>
   <OpenApiDocumentsDirectory>../../docs</OpenApiDocumentsDirectory>
   <OpenApiGenerateDocumentsOnBuild>false</OpenApiGenerateDocumentsOnBuild>
   ```

3. Схема снимается отдельным шагом, а не каждым `dotnet build`:

   ```powershell
   # Строка подключения нужна только непустая; это dummy для сборки, не рабочие учётные данные.
   $env:ConnectionStrings__DefaultConnection = "Host=127.0.0.1;Port=5433;Database=MySite;Username=build;Password=build"
   dotnet build src/MySite.Web/MySite.Web.csproj -m:1 -p:OpenApiGenerateDocumentsOnBuild=true
   ```

   Получается `backend/docs/openapi.json`; он коммитится. Пакет `dotnet-getdocument` запускает
   entrypoint приложения, поэтому `AddInfrastructure` видит непустую строку; к базе он не
   подключается.

4. `packages/api-client` генерирует типы из этого файла:
   `openapi-typescript ../../../backend/docs/openapi.json -o src/generated/openapi.d.ts`.
5. **Если build-time generation в этом приложении не запускается** — остановиться, записать точную
   команду и ошибку, не подменять молча. Резервный путь — снять схему с запущенного API
   (`Invoke-WebRequest http://localhost:5054/openapi/v1.json`), как это делал 11; база при этом
   нужна только для контента, не для схемы.

### Пакет

```text
frontend/packages/api-client/
├── package.json                 # exports: { ".": "./src/index.ts" } — только "."
├── tsconfig.json
└── src/
    ├── index.ts                 # публичный API: createApiClient, getHome/getProfile/getProjects, ApiError, типы
    ├── client.ts                # createClient<paths>, baseUrl из process.env.API_BASE_URL
    ├── api-error.ts             # ApiError + разбор ProblemDetails
    └── generated/
        └── openapi.d.ts         # generated; руками не правится
```

- `openapi-typescript` — devDependency, `openapi-fetch` — dependency; обе записи `catalog:`.
- `src/index.ts` — только named exports, без `export *`, без default export.
- Глубокие импорты запрещены `exports` только `"."` и паттерном `no-restricted-imports`
  `@mysite/api-client/*` рядом с `@mysite/ui/*`. Для корневого импорта одной этой пары мало:
  текущая политика `eslint-plugin-boundaries` в `packages/eslint-config/src/index.mjs` разрешает
  `api-client` из `route`, `pages`, `widgets`, `features`, `entities` и `shared`. Нужно убрать
  `api-client` из всех разрешённых импортёров, кроме `pages`, — тогда компонент, импортирующий
  пакет, падает на границе, а `_pages/<slice>/api` остаётся единственным местом вызова.
  `import/no-restricted-paths` (`target: ./packages`, `from: ./apps`) закрывает обратное
  направление (пакеты не импортируют приложения), поэтому этот случай он не ловит.
- Generated-файл не линтуется: `globalIgnores` для `packages/api-client/src/generated/**` в
  корневом `eslint.config.mjs`; `tsc --noEmit` его включает. Причина — код не наш, чинить его
  нельзя, а сверку гарантируют `tsc` и `git diff`. Это требует строки в `.dsh/AGENTS.md`
  (Quality → Lint and types) — правит планер.
- `getHomeContent` использует публичные функции пакета, а не `fetch` по строке. Рекомендация 11 —
  один `GET /api/v1/home`: тикет 03 выбрал составной ответ именно ради одного round-trip, а
  `profile: null` + `projects` уже выражает частичный/пустой контент. Если владелец хочет
  частичный отказ по двум запросам, вызываются `getProfile` + `getProjects`, а формулировка 18 про
  `Promise.allSettled` уточняется отдельно; тикет 11 это расхождение зафиксировал.
- `cache: "no-store"` передаётся в `client.GET` как обычная fetch-опция; `process.env.API_BASE_URL`,
  не `import.meta.env` (это Vite-механизм, в Next.js Server Component его нет).

### CI

Две независимые проверки:

- **Клиент против схемы** (frontend job): `pnpm --filter @mysite/api-client generate`, затем
  `git diff --exit-code -- packages/api-client`.
- **Схема против бэкенда** (backend job): шаг из раздела «Схема» выше, затем
  `git diff --exit-code -- docs/openapi.json` из `backend/`. База и контейнер не нужны.

Порядок frontend-гейтов: install → generate → `git diff` → lint → typecheck → build. Тесты клиента
через MSW ставит тикет 20; здесь — минимальная проверка, что публичные функции достают
`profile`/`projects` с `http://localhost:5054`.

### Что остаётся 16 и 20

- 16 собирает страницу на `getHomeContent` и синхронный view; состояния — по 18, с учётом формы
  ответа из этого раздела.
- 20 ставит Vitest/RTL/MSW/Playwright и тестирует публичный API клиента, а не сгенерированный файл;
  фикстуры типизируются типами из пакета.

## Answer

**Сделано.** Клиент генерируется из закоммиченной схемы бэкенда и подключён к странице одной тонкой
границей данных.

- **Схема.** `Microsoft.Extensions.ApiDescription.Server` 10.0.12 — версия в
  `backend/Directory.Packages.props`, пакет в `MySite.Web.csproj`, `OpenApiGenerateDocumentsOnBuild`
  выключен, поэтому обычная сборка приложение не запускает. Документ снимается отдельным шагом и
  лежит в `backend/docs/openapi.json` (закоммичен). Build-time генерация в этом приложении
  **работает**: резервный путь с запущенного API не понадобился.
- **Пакет.** `@mysite/api-client`: `openapi-typescript` 7.13.0 (devDependency) + `openapi-fetch`
  0.17.0 (dependency), обе версии — в pnpm-каталоге; в `peerDependencyRules` добавлено
  `"openapi-typescript>typescript": "6"` с причиной и условием пересмотра. Типы —
  `src/generated/openapi.d.ts` (generated, в git), публичный API — только `src/index.ts`:
  `createApiClient`, `getHome`, `ApiError` и типы `OwnerProfile` / `Project` / `HomeResponse`.
  `baseUrl` берётся из `API_BASE_URL`, по умолчанию `http://localhost:5054`.
- **Граница данных.** `apps/web/src/_pages/home/api/get-home-content.ts` вызывает `getHome` и
  отдаёт `{ profile, projects }`; `HomeContent` и `getHomeContent` экспортируются из публичного API
  слайса `_pages/home/index.ts`. Компоненты пакет не импортируют.
- **ESLint.** `api-client` разрешён только слою `pages` (`eslint-plugin-boundaries`), глубокие
  импорты `@mysite/api-client/*` запрещены, `packages/api-client/src/generated/**` — в
  `globalIgnores`. Хук `pre-commit` получил `--no-warn-ignored`: без него ESLint предупреждал о
  явно переданном ignore-файле, `--max-warnings=0` превращал это в падение коммита. Правила не
  ослаблены, файл по-прежнему не линтуется — это решение тикета, а не подавление.
- **CI.** Две независимые проверки: `frontend.yml` — `pnpm --filter @mysite/api-client generate` →
  `git -C .. diff --exit-code -- frontend/packages/api-client` перед гейтами (порядок из тикета:
  install → generate → diff → lint → typecheck → build); `backend.yml` — шаг схемы →
  `git diff --exit-code -- docs/openapi.json`. База и запущенный API не нужны, строка подключения
  только непустая.

**Чем подтверждено.**

Схема, два прогона подряд (детерминированность):

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=127.0.0.1;Port=5433;Database=MySite;Username=build;Password=build"
dotnet build src/MySite.Web/MySite.Web.csproj -m:1 -p:OpenApiGenerateDocumentsOnBuild=true
# Сборка успешно завершена. Предупреждений: 0, Ошибок: 0
# (Get-FileHash docs\openapi.json).Hash → 2E8A5AF191F8836655B376F3B90265E4DC8A2E00E472FF8A4C48E12A8A00D912
# повторный прогон — тот же хеш
```

Файл 11 256 байт; `servers` в build-time документе нет (в runtime-снимке был — исследование 11 это
предсказывало), `openapi-typescript` его не использует.

Фронтенд:

```text
pnpm install --frozen-lockfile   → EXIT=0
pnpm lint                        → EXIT=0
pnpm typecheck                   → EXIT=0 (packages: ui, api-client, shared, apps/web)
pnpm build                       → EXIT=0, ƒ /[lang]
pnpm --filter @mysite/api-client generate → EXIT=0
git -C .. diff --exit-code -- frontend/packages/api-client → EXIT=0
```

Бэкенд:

```text
dotnet build MySite.slnx -m:1  → Предупреждений: 0, Ошибок: 0, EXIT=0
dotnet test MySite.slnx        → итог: 43, сбой: 0, успешно: 43, пропущено: 0, EXIT=0
dotnet ef migrations list      → 20261006231005_ContentSchema (Pending нет)
dotnet ef database update      → No migrations were applied. The database is already up to date. Done.
docker compose ps              → mysite_postgres … Up 7 hours (0.0.0.0:5433->5432/tcp)
pg_isready                     → /var/run/postgresql:5432 - accepting connections
```

Клиент против живого локального API. Строка подключения и профиль — из локальной базы
(`seed` → `Profile: unchanged. Projects: … unchanged 4`). Проверка шла через настоящий серверный путь
Next.js: `getHomeContent("ru")` временно вызывался из `home-page.tsx`, dev-сервер отдал страницу:

```text
GET http://localhost:3000/ru → 200
… Заглушка: .NET-разработчик, восемь лет в промышленной разработке 3 projects,
  first: Заглушка: MySite — этот сайт …
```

Три проекта из четырёх — верно: `Keycloak_Learn` не опубликован. Временная правка страницы
откачена, в коммиты не попала. `Invoke-WebRequest http://localhost:5054/api/v1/home?locale=ru` →
`200` с `profile` и `projects.items`.

Негативные проверки CI — обе падают на расхождении (это и был критерий):

```text
# правка описания в backend/docs/openapi.json, закоммичена, не перегенерирована:
pnpm --filter @mysite/api-client generate; git -C .. diff --exit-code -- frontend/packages/api-client
  → FRONTEND_STALE_EXIT=1 (diff по src/generated/openapi.d.ts)
dotnet build src/MySite.Web/… -p:OpenApiGenerateDocumentsOnBuild=true; git diff --exit-code -- docs/openapi.json
  → BACKEND_STALE_EXIT=1 (diff по docs/openapi.json)
# после отката правки обе проверки дают 0.
```

Эта же негативная проверка нашла ошибку в первой версии шага: из `frontend/` пути git-диффа
`packages/api-client` не существует (pathspec от корня), проверка молча проходила. Путь исправлен на
`frontend/packages/api-client`, после чего обе проверки ведут себя правильно в обе стороны.

**Что нашёл первый прогон CI (и чем это кончилось).** `Lint, typecheck, build` был зелёным сразу, а
`Build and test` упал на шаге сверки схемы: закоммиченная на Windows схема несла в описаниях
экранированные `\r\n`, Linux перегенерировал те же описания с `\n`. Причина не в переводах строк
исходников, а в компиляторе: Roslyn пишет XML-doc файл с CRLF на любой платформе, и эти символы
попадают в описания OpenAPI. Документ был platform-dependent, то есть критерий «CI ловит расхождение»
работал бы как «CI красный всегда». Исправлено целью `NormalizeGeneratedOpenApiDocument` в
`MySite.Web.csproj`: после генерации экранированные `\r\n` в документе заменяются на `\n`, других
отличий между платформами нет. Схема перегенерирована (хеш выше), `openapi.d.ts` от нормализации не
изменился (JSDoc и так нёс `\n`), повторный прогон даёт тот же хеш. Проверка на Linux — повторный
прогон CI в этом PR.

Негативные проверки ESLint (временные файлы, удалены):

```text
apps/web/src/widgets/probe/index.ts  import { getHome } from "@mysite/api-client"
  → FSD boundary violated: widgets may not import api-client (boundaries/dependencies)
widgets, feature, entity, shared       import type … "@mysite/api-client/src/generated/openapi"
  → '@mysite/api-client/*' import is restricted … (no-restricted-imports)
packages/ui (ui-package)               import api-client
  → FSD boundary violated: ui-package may not import api-client
```

**Решение, которое пришлось принять (вопрос задан владельцу при выдаче).** Владелец выбрал **один**
`GET /api/v1/home` (рекомендация исследования 11), поэтому `HomeContent = { profile, projects }`, без
поля `failed`, и `getProfile`/`getProjects` в публичный API пакета не выводятся: единственная
функция-эндпоинт — `getHome`. Формулировка тикета 18 про `Promise.allSettled` и частичный отказ по
двум запросам этим решением заменяется — её надо поправить в 18 (и учесть в 16), чтобы документ и код
не расходились.

**Файлы, ветка, коммиты.** Ветка `feature/wire-generated-client` от `develop` = `origin/develop` =
`e74f8dc`.

- `4c97c37` feat: снимать OpenAPI-схему на сборке бэкенда — `backend/Directory.Packages.props`,
  `backend/src/MySite.Web/MySite.Web.csproj`, `backend/docs/openapi.json` (новый).
- `6818c00` feat: подключить сгенерированный клиент `@mysite/api-client` — новый пакет
  (`package.json`, `tsconfig.json`, `src/client.ts`, `src/api-error.ts`, `src/index.ts`,
  `src/generated/openapi.d.ts`), `apps/web/package.json`, `apps/web/src/_pages/home/api/get-home-content.ts`
  (новый) и `_pages/home/index.ts`, `frontend/eslint.config.mjs`,
  `packages/eslint-config/src/index.mjs`, `frontend/lefthook.yml`, `pnpm-workspace.yaml`,
  `pnpm-lock.yaml`.
- `93e7fc6` ci: две проверки расхождения — `.github/workflows/frontend.yml`, `backend.yml`.
- `b4ffc86` docs: `WORKFLOW.md` (раздел «Generated API client», порядок гейтов, DoD п. 7),
  `ARCHITECTURE.md` (стек, раскладка, граница данных, тестовый контур), `frontend/README.md`.
- `a6a30ae` fix: одна и та же схема на Windows и Linux — цель нормализации в `MySite.Web.csproj`,
  перегенерированный `backend/docs/openapi.json`.

**Что осталось за рамками и что требует решения владельца.**

1. `.dsh/AGENTS.md` (Quality → Lint and types) — строку про то, что сгенерированный файл не линтуется,
   а сверяется `tsc` и diff'ом, правит планер; в этой ветке файл не тронут (граница §10).
2. Тикет 18: поправить формулировку про два запроса и `failed` — решением владельца выбран `/home`.
3. Пакет `server-only` в монорепе не установлен; добавлять зависимость без согласования не стал,
   `get-home-content.ts` помечен комментарием, что он серверный. Если нужен компиляторный запрет
   клиентского импорта — отдельный тикет и согласованная зависимость.
4. Тесты клиента через MSW, четыре состояния экрана и переключатель языка — тикеты 16 и 20 (по
   решению тикета 18 тестовый контур ставится в 20).
5. Строка подключения с `Username=build;Password=build` в CI и `WORKFLOW.md` — dummy, только чтобы
   `AddInfrastructure` пропустил старт; к базе никто не подключается.
6. `getHome`/`ApiError` протестированы вручную живым запросом, а не юнит-тестом: раннера фронтенда до
   тикета 20 нет. Это осознанное отступление от «новая функциональность покрыта тестом», записанное
   здесь как вопрос владельцу — при желании закрывается тикетом 20 до мержа.
