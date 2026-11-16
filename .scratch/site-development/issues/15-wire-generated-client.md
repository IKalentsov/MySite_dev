# 15 — Подключить сгенерированный клиент

Type: task
Status: open
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

<!-- заполняется при закрытии -->
