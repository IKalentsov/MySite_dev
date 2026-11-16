# 11 — Генерация API-клиента

Type: research
Status: resolved
Blocked by: 03

## Как работать с этой задачей

**Правила работы с проектом — `.dsh/AGENT-TASKS.md`.** Прочитай его первым, до первой правки: там
команды, правила веток, среда и критерий готовности. Ниже — только специфика этой задачи.

- Ветка: `feature/generated-api-client` от свежего `develop`; push ветки и PR в `develop` — работа
  исполнителя, слияние — владельца.
- Критерий готовности этого тикета: исследование записано в `docs/research/frontend-api-client.md`,
  выбор инструмента зафиксирован в `## Answer` с обоснованием и с тем, что именно он даёт против
  альтернатив. Это тикет-решение: «зелёные тесты» к нему не применяются, но OpenAPI-схема, на
  которую он опирается, обязана быть проверена на реальном запущенном API.
- Если для проверки схемы нужно поднять API — это можно: подними, сними `openapi.json`, останови.

**Условие остановки:** Инструмент генерации выбран и обоснован в `docs/research/frontend-api-client.md`; схема OpenAPI снята с реально запущенного локального API; в файле тикета записано, как встраивается в pnpm-монорепу и как ловится расхождение в CI.

## Question

Правила проекта: клиент **генерируется из OpenAPI-схемы бэкенда**, generated-файлы никогда не
правятся руками, при изменении схемы клиент перегенерируется тем же изменением, расхождение
ловится CI. UI не импортирует сгенерированное напрямую — только через публичный API фичи.

Выяснить:

- Инструмент под TypeScript и текущий стек: `openapi-typescript` + `openapi-fetch`, `orval`,
  `kubb`, `NSwag` или иное. Критерии: типы без `any`, tree-shaking, поддержка OpenAPI 3.1 (что
  отдаёт `Microsoft.AspNetCore.OpenApi`), отсутствие тяжёлого рантайма.
- Откуда берётся схема: коммитим `openapi.json` в репозиторий или генерируем при сборке из
  запущенного API. Как это стыкуется с `packages/api-client` в pnpm-монорепе.
- Как выглядит проверка расхождения в CI: перегенерировать и упасть, если `git diff` не пуст.
- Как запретить прямые импорты generated из UI: правило ESLint или граница пакета.

Результат — `docs/research/frontend-api-client.md`. Решение по инструменту принимается здесь же, в
ответе тикета, и применяется при выполнении тикета 15.

## Answer

**Решение: `openapi-typescript@7.13.0` + `openapi-fetch@0.17.0`.** Схема генерируется **на сборке
бэкенда** пакетом `Microsoft.Extensions.ApiDescription.Server@10.0.12` в закоммиченный
`backend/docs/openapi.json`; сгенерированные типы лежат в
`frontend/packages/api-client/src/generated/openapi.d.ts` и тоже закоммичены. Клиент — тонкая
обёртка над `fetch`, без query-библиотеки и без сгенерированного рантайма. Полное исследование —
[`docs/research/frontend-api-client.md`](../../../docs/research/frontend-api-client.md).

### Что сделано и чем подтверждено

1. Прочитаны контракт, тикеты 03/09/10/14/18, заметки `frontend-stack-versions.md` и
   `frontend-quality-contour.md`, `ARCHITECTURE.md` (Frontend).
2. Снята схема с **реально запущенного** локального API; проверены пути, параметры, схемы,
   версия OpenAPI, размер и SHA-256.
3. `openapi-typescript` + `openapi-fetch` установлены и запущены во временном каталоге
   `%TEMP%\api-client-eval` против этой схемы: генерация, `tsc --noEmit` под TS 6.0.3, вызов
   живого API из Node, проверка детерминированности.
4. Проверено по документации Microsoft, что схема может генерироваться на сборке бэкенда
   (`Microsoft.Extensions.ApiDescription.Server`) и что это требует непустой строки подключения.
5. Написана заметка; обновлены тикеты 15/16/20 (владелец разрешил правку их файлов).

### Схема, снятая с API

Команда:

```powershell
Invoke-WebRequest http://localhost:5054/openapi/v1.json -OutFile "$env:TEMP\mysite-openapi.json"
```

| Факт | Значение |
|---|---|
| `openapi` | **3.1.1** |
| Пути | 3: `/api/v1/home`, `/api/v1/profile`, `/api/v1/projects` (все `GET`) |
| Схемы | `HomeResponse`, `OwnerProfileResponse`, `PaginatedProjectsResponse`, `ProblemDetails`, `ProjectResponse` |
| Параметры | `locale` на всех трёх; `cursor` и `pageSize` на `/home` и `/projects` |
| Размер | **11 334 байта** |
| SHA-256 | `8b5575e318365656890bf67b82bef13c7cfa5f2c5fee9ec6cff3ccb8741e6ac8` |

Схема использует 3.1-конструкции: `pageSize` и `year` — `["integer", "string"]`, `nextCursor` —
`["null", "string"]`, `HomeResponse.profile` — `oneOf` с `null`. Это то, что реально отдаёт
`Microsoft.AspNetCore.OpenApi` 10.0.12; документация Microsoft подтверждает, что .NET 10 по
умолчанию генерирует OpenAPI 3.1.

`docker compose ps` показал `mysite_postgres` в состоянии `Up` (контейнер уже работал до задачи);
`pg_isready` — `accepting connections`; `dotnet ef migrations list` — `20261006231005_ContentSchema`,
без `(Pending)`. API на 5054 уже слушал (PID 8616, `dotnet run --no-build`, запущен в 03:18:54);
повторный `dotnet run` упал на `address already in use`, поэтому схема снята с уже запущенного
экземпляра этого же приложения, а не с пересобранного.

Дополнительно проверено, что схема не требует живой базы: второй экземпляр на 5055 с
`ConnectionStrings__DefaultConnection`, указывающей на недоступный порт 5999, отдал
`/openapi/v1.json` с `200`, тем же размером 11 334 байта; отличие от захвата 5054 — только
`servers.url` (`http://127.0.0.1:5055/` вместо `http://localhost:5054/`). Экземпляр 5055 остановлен.

### Инструмент: что и почему

| Пакет | Версия | Лицензия | Роль |
|---|---|---|---|
| `openapi-typescript` | **7.13.0** | MIT | генерация типов, devDependency |
| `openapi-fetch` | **0.17.0** | MIT | fetch-клиент, dependency |
| `openapi-typescript-helpers` | **0.1.0** | MIT | транзитивная зависимость `openapi-fetch`, отдельно не пинуется |
| `Microsoft.Extensions.ApiDescription.Server` | **10.0.12** | MIT | build-time генерация схемы (NuGet) |

Проверка во временном каталоге:

```powershell
pnpm install
pnpm exec openapi-typescript openapi.json -o src/generated/openapi.d.ts
pnpm exec tsc --noEmit
node src/live.ts
```

Результаты: файл 9 370 байт / 241 строка; `any` в нём нет; два прогона дали одинаковый SHA-256
(`BD1667FFA53E5E0245C887B62399A5B98136D58889C2A83EBA52617078229047`); `tsc --noEmit` — `EXIT=0`;
Node-вызов живого API вернул `profile.locale=ru`, три проекта, `hasMore=false`; `cache: "no-store"`
принят как обычная fetch-опция.

**Что даёт против альтернатив:** только типы (нулевой рантайм от генератора) плюс 6 КБ
`openapi-fetch`; отсутствие query-библиотеки; точные типы путей/параметров/ответов без `any`;
детерминированный вывод; один файл на три эндпоинта; работа в Node/Server Component без DOM.
**Чего не даёт:** сгенерированных функций на операцию (вызов — `client.GET("/api/v1/home", …)`),
runtime-валидации, встроенной команды «вывод устарел» (её заменяет `git diff`), официальной
поддержки TypeScript 6 (peer `^5.x`; issue #2723 открыт).

**Альтернативы** (подробно в заметке; orval/kubb/NSwag не запускались):

- `orval@8.48.0` — свежий, 3.1, native fetch без рантайма, `--outdated`; но по умолчанию axios и
  генерирует SDK-функции. Это документированный fallback, если TS 6-peer окажется неприемлем.
- `kubb@5.20.2` — 3.1/3.2, плагины, TS 6 в peer `plugin-ts`; но bundled-рантайм с `.ts`-импортами,
  нет заявления о детерминированности, `clean` по умолчанию выключен.
- `nswag@14.8.0` — только 3.0, `any`, классы, требует .NET; отклонён.
- `@hey-api/openapi-ts@0.99.0` — единственный с явным TS 6 peer, но генерирует рантайм с `any`
  и ~60 КБ / 15 файлов; отклонён по правилу «без any» и по объёму.

**TypeScript 6.** `openapi-typescript@7.13.0` объявляет peer `typescript ^5.x`; проект на 6.0.3.
Сгенерированные типы под 6.0.3 компилируются (проверено), но upstream ещё не расширил диапазон:
issue #2723 открыт с 2026-03-27, есть открытые PR. В 15 добавляется разрешение в
`frontend/pnpm-workspace.yaml` рядом с существующим `eslint-plugin-import>eslint`:

```yaml
peerDependencyRules:
  allowedVersions:
    "eslint-plugin-import>eslint": "10"
    "openapi-typescript>typescript": "6"
```

Это разрешение устаревшего peer-объявления, а не отключение правила. Условие пересмотра — релиз с
`^6` в peer или переход на TypeScript 7. Если владелец отклонит разрешение — брать orval.

### Схема: где живёт и как обновляется

**Коммитим, а не генерируем при каждом билде из запущенного API.** Схема производится на сборке
бэкенда:

- `Microsoft.Extensions.ApiDescription.Server` **10.0.12** в `Directory.Packages.props` и
  `MySite.Web.csproj`;
- `<OpenApiGenerateDocumentsOptions>--file-name openapi</OpenApiGenerateDocumentsOptions>`;
- `<OpenApiDocumentsDirectory>../../docs</OpenApiDocumentsDirectory>` → `backend/docs/openapi.json`;
- `<OpenApiGenerateDocumentsOnBuild>false</OpenApiGenerateDocumentsOnBuild>` по умолчанию, чтобы
  обычный `dotnet build` не запускал приложение;
- отдельный шаг `dotnet build src/MySite.Web/MySite.Web.csproj -m:1
  -p:OpenApiGenerateDocumentsOnBuild=true` с непустой dummy-строкой
  `ConnectionStrings__DefaultConnection` (пакет запускает entrypoint приложения, а
  `AddInfrastructure` падает на пустой строке; база при этом не нужна).

`backend/docs/openapi.json` коммитится. `packages/api-client` генерирует из него
`src/generated/openapi.d.ts` командой
`openapi-typescript ../../../backend/docs/openapi.json -o src/generated/openapi.d.ts`.

Почему не генерировать из запущенного API на каждом фронтенд-билде: это делает сборку
негерметичной, требует живой бэкенд и базу, ломает офлайн-работу и делает фронтенд зависимым от
доступности сервиса. Почему не тянуть схему артефактом между джобами: файл лежит в репозитории,
и обе джобы видят его в одном checkout; артефакт не нужен.

**Ограничение:** build-time генерация в этом репозитории ещё **не запускалась** — 11 не меняет
backend-код. Механизм подтверждён документацией Microsoft: пакет, свойства и запуск entrypoint
приложения. 15 обязана проверить это на месте; если `dotnet-getdocument` не запускается —
остановиться, записать точную ошибку и не подменять молча. Резервный путь — снять схему с
запущенного API (команда выше), как это делал 11.

### CI: как ловится расхождение

Две независимые проверки:

**1. Клиент против схемы** (frontend job, после `pnpm install --frozen-lockfile`):

```text
pnpm --filter @mysite/api-client generate
git diff --exit-code -- packages/api-client
```

Если схема изменилась, а типы не перегенерированы, diff не пуст — падение. Это строже, чем
перегенерация без диффа: забытый закоммитить файл не будет молча перезаписан в CI.

**2. Схема против бэкенда** (backend job):

```text
dotnet build MySite.slnx -m:1 -p:OpenApiGenerateDocumentsOnBuild=true
git diff --exit-code -- docs/openapi.json
```

с dummy-строкой подключения. Если бэкенд-контракт изменился, а схема не перегенерирована — diff
не пуст. База и контейнер не нужны; цена — уже существующая сборка плюс шаг генерации.

Frontend-гейты после этого: `pnpm lint` → `pnpm typecheck` → `pnpm build`. Тесты клиента через
MSW — тикет 20.

### Форма `packages/api-client`

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
  `@mysite/api-client/*`. Для корневого импорта этого мало: текущая политика
  `eslint-plugin-boundaries` в `packages/eslint-config/src/index.mjs` разрешает `api-client` из
  `route`, `pages`, `widgets`, `features`, `entities` и `shared`. 15 убирает `api-client` из всех
  разрешённых импортёров, кроме `pages`, — тогда компонент, импортирующий пакет, падает на
  границе. `import/no-restricted-paths` (`target: ./packages`, `from: ./apps`) закрывает обратное
  направление и этот случай не ловит.
- Generated-файл помечен заголовком `openapi-typescript`; ESLint его **игнорирует**
  (`globalIgnores` в корневом `eslint.config.mjs`), `tsc --noEmit` **включает**. Основание: код не
  наш, править его нельзя, а корректность гарантируют компилятор и `git diff`; линтовать его —
  значит превратить обновление генератора в неисправимую ошибку сборки. Это требует строки в
  `.dsh/AGENTS.md` (Quality → Lint and types) — правит планер.
- `cache: "no-store"` передаётся в `client.GET` как fetch-опция; base URL — `process.env.API_BASE_URL`,
  не `import.meta.env` (Vite-механизм, в Next.js Server Component его нет).
- Query-библиотеки нет: клиент — тонкая функция, вызываемая из `getHomeContent`.

### План для 15 (15 реализует, 11 не реализует)

1. **Backend.** Добавить `Microsoft.Extensions.ApiDescription.Server` 10.0.12, свойства из раздела
   «Схема», сгенерировать `backend/docs/openapi.json`, закоммитить. Проверить, что обычный
   `dotnet build` не запускает генератор, а шаг с `-p:OpenApiGenerateDocumentsOnBuild=true` —
   запускает.
2. **Frontend-каталог.** Добавить в `pnpm-workspace.yaml` `openapi-typescript: 7.13.0`,
   `openapi-fetch: 0.17.0` и peer-разрешение `openapi-typescript>typescript: 6`.
3. **Пакет.** Добавить `packages/api-client` скрипты `generate` и `typecheck`, зависимости
   `catalog:`, файлы `src/index.ts`, `src/client.ts`, `src/api-error.ts`; сгенерировать
   `src/generated/openapi.d.ts`; `exports` оставить только `"."`.
4. **Публичный API.** `createApiClient(baseUrl)`, `getHome`, `getProfile`, `getProjects`; ошибки —
   `ApiError` из `ProblemDetails`; типы — из `components["schemas"]`.
5. **`getHomeContent`.** Рекомендация — один `GET /api/v1/home` (тикет 03 выбрал составной ответ
   ради одного round-trip; `profile: null` + `projects` выражает частичный контент). Если владелец
   хочет частичный отказ по двум запросам — `getProfile` + `getProjects` и уточнение 18. Это
   расхождение 03/18 зафиксировано в заметке; 15 выбирает и записывает.
6. **ESLint.** `globalIgnores` для `packages/api-client/src/generated/**`; паттерн
   `@mysite/api-client/*` в `no-restricted-imports`; убрать `api-client` из разрешённых
   импортёров `route`, `widgets`, `features`, `entities` и `shared` в
   `packages/eslint-config/src/index.mjs`, оставив только `pages`.
7. **CI.** Backend: `git diff --exit-code -- docs/openapi.json`; frontend: `generate` + `git diff
   --exit-code -- packages/api-client` до lint/typecheck/build.
8. **Проверка.** Минимальный вызов `getHomeContent` с локального API; тесты через MSW — 20.

### Что остаётся 16 и 20

- **16** собирает страницу на `getHomeContent` и синхронный view; состояния — по 18, с учётом формы
  ответа (`profile: null` — empty/partial, не error; полный отказ — `error.tsx`).
- **20** ставит Vitest/RTL/MSW/Playwright; тестирует публичный API клиента, а не generated-файл;
  MSW 3 (`onUnhandledFrame`), фикстуры типизированы типами из пакета; без `any`.

### Находки, риски и что требует решения

1. **TS 6 peer.** `openapi-typescript` официально не поддерживает 6.0.3; сгенерированный код
   компилируется, но это не гарантия upstream. Решение — peer-разрешение + пересмотр при релизе
   с `^6` или при переходе на TS 7.
2. **Расхождение 03 и 18.** 03 создал `/home` ради одного round-trip; 18 описывает два запроса и
   partial по одному упавшему. Генерированный клиент умеет и то, и другое; 15 должна выбрать.
   Рекомендация — `/home`.
3. **Build-time генерация не проверена в этом репозитории.** 15 проверяет; при падении — остановка
   и точная ошибка, не тихая подмена.
4. **`servers` в build-time документе может отличаться** от runtime (в runtime он выводится из
   Host). На типы это не влияет: `openapi-typescript` `servers` не использует, base URL — из env.
5. **`appsettings.Development.json` не в репозитории.** Поэтому CI-шагу нужен явный
   `ConnectionStrings__DefaultConnection`; пустая строка роняет `AddInfrastructure` ещё до
   генерации.
6. **`import.meta.env` в Next.js не используется** — это Vite-механизм; base URL берётся из
   `process.env.API_BASE_URL` в server-компоненте.

### Требует правки `.dsh/AGENTS.md` и `map.md` (правит планер)

- **`.dsh/AGENTS.md` → Frontend → Quality → Lint and types:** generated-код исключён из ESLint,
  включён в `tsc --noEmit`; основание — код не авторский, его гарантирует компилятор + `git diff`.
- **`.dsh/AGENTS.md` → Frontend → API client:** уточнить, что схема производится на сборке
  бэкенда и коммитится в `backend/docs/openapi.json`; «UI не вызывает клиент» означает
  «компоненты не вызывают, вызывает `_pages/home/api/getHomeContent`».
- **`WORKFLOW.md` → Frontend:** добавить команды генерации схемы и клиента, шаг `git diff` в CI.
- **`map.md`:** строка решения 11 (`openapi-typescript 7.13.0 + openapi-fetch 0.17.0`, build-time
  схема, два `git diff`-гейта) и отметка, что 15/16/20 уточнены.

### Git

Ветка `feature/generated-api-client` от `develop` `f01e0024930d611ac2a789859a9f10719192b9be`
(после `git fetch origin`, `git switch develop`, `git merge --ff-only origin/develop`; хеши
`develop` и `origin/develop` совпали). Коммит: `H1_PLACEHOLDER` — исследование, заметка и
уточнения 15/16/20; PR — ниже.

PR в `develop`: https://github.com/IKalentsov/MySite_dev/pull/19.

### Состояние после работы

- Docker: контейнер `mysite_postgres` уже был `Up` до задачи; `docker compose up -d` был no-op,
  `down -v` не применялся, том не пересоздавался.
- API: экземпляр 5054 уже работал; использован для схемы и остановлен по завершении. Временный
  экземпляр 5055 (без базы) остановлен.
- Временные каталоги `%TEMP%\api-client-eval`, `%TEMP%\api-client-eval-hey`, `%TEMP%\apidesc-10.0.12`
  — вне репозитория, в коммит не попали. `Migrations` не менялись, `(Pending)` нет.
