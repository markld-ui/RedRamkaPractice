# RedRamkaPractice — Project Lifecycle Service

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)
![Tests](https://img.shields.io/badge/tests-334_passing-brightgreen)
[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/markld-ui/RedRamkaPractice)

## 📌 О проекте

**RedRamkaPractice** — учебный backend-проект, реализующий сервис управления жизненным циклом проектов в компании.

Проект сфокусирован на **архитектуре, доменной логике и корректной реализации бизнес-сценариев**. Ключевая особенность — конечный автомат (FSM) для управления стадиями проекта с жёсткой защитой инвариантов на уровне домена.

> Проект намеренно ограничен по scope, чтобы не размывать архитектуру, показать осознанный дизайн и упростить сопровождение.

---

## 🎯 Цели проекта

- Отработать **Clean Architecture** с логическим CQRS через MediatR
- Реализовать **FSM** для управления стадиями проекта (Stateless)
- Показать правильное разделение ответственности между слоями
- Покрыть логику юнит-тестами (334 теста, Moq + FluentAssertions)
- Подготовить проект, который **запускается одной командой**

---

## 🚫 Явные ограничения

| Есть | Нет |
|---|---|
| Один backend-сервис | UI / Frontend |
| REST API + Swagger | Микросервисы |
| JWT-аутентификация | Внешние интеграции |
| Фиксированный FSM | Event Sourcing |
| Юнит-тесты | Нагрузочные тесты |

---

## 🧱 Архитектура

Проект построен на **Clean Architecture** с вертикальными слоями.

```
src/
├── API/                  ← Controllers, Middleware, DI
├── Application/          ← MediatR, Commands/Queries, Validators
├── Domain/               ← Entities, FSM, Domain Events
└── Persistence/          ← EF Core, Configurations, Seed
```

Зависимости направлены строго внутрь: `API → Application → Domain ← Persistence`.

### CQRS

Команды и запросы разделены логически через MediatR. Каждый use case — отдельный файл:

```
Application/Features/
├── Auth/
│   ├── Commands/         ← Register, Login, RefreshToken
├── Projects/
│   ├── Commands/         ← Create, StartDevelopment, SendToQA, ...
│   ├── Queries/          ← GetProjects, GetProjectById
│   └── Specifications/
│       ├── Commands/     ← CreateSpecification, ApproveSpecification
│       └── Queries/      ← GetSpecifications, GetSpecificationById
├── Credentials/
├── Roles/
└── Users/
```

---

## 🔄 Жизненный цикл проекта (FSM)

FSM реализован через библиотеку **Stateless**. Стадии и переходы:

```
                 ┌──────────────┐
         ┌──────►│    Design    │◄──────────────┐
         │       └──────┬───────┘               │
         │              │ StartDevelopment       │ ReturnToDesign
         │              ▼                        │ (reason)
         │       ┌──────────────┐                │
         │       │ Development  ├────────────────┘
         │       └──────┬───────┘
         │              │ SendToQA
         │              ▼
         │       ┌──────────────┐
         │       │      QA      │
         │       └──────┬───────┘
         │    PassQA    │    FailQA (reason)
         │              ▼         └──► Development
         │       ┌──────────────┐
         │       │   Delivery   │
         │       └──────┬───────┘
         │              │ Release
         │              ▼
         │       ┌──────────────┐
         └───────┤   Support    │
                 └──────┬───────┘
                        │ Archive (reason)
                        ▼
                 ┌──────────────┐
                 │   Archived   │
                 └──────────────┘
```

Все переходы, требующие причины (`reason`), валидируются. Неверный переход возвращает `TransitionResult { IsSuccess: false, Error: "..." }` — исключений в контроллер не летит.

---

## 👥 Роли и авторизация

| Роль | Возможности |
|---|---|
| `Admin` | Всё, в том числе управление ролями и пользователями |
| `ProjectManager` | Управление своими проектами, переходы стадий, спецификации |
| `Developer` | Участие в проектах, отправка на QA |
| `Tester` | Прохождение / провал QA |
| `ProductManager` | Участие в проектах |
| `DevOps` | Участие в проектах |

Авторизация двухуровневая: системная роль (JWT-клейм) + роль в конкретном проекте (`ProjectMember`).

---

## 📋 Спецификации проекта

Перед переходом в `Development` проект должен иметь хотя бы одну **утверждённую спецификацию**. Логика:

- Создать спецификацию может только `ProjectManager`
- Утвердить может только `ProjectManager`
- Утверждение новой спецификации автоматически отзывает предыдущую утверждённую
- Версии инкрементируются автоматически

---

## 📁 Полная структура репозитория

```
/
├── src/
│   ├── Api/
│   │   ├── Controllers/
│   │   ├── Middleware/           ← GlobalExceptionHandler
│   │   └── Services/             ← CurrentUserService, TokenService, ...
│   ├── Application/
│   │   ├── Common/
│   │   │   ├── Behaviours/       ← ValidationBehaviour (MediatR pipeline)
│   │   │   ├── Constants/        ← RoleConstants
│   │   │   └── Interfaces/       ← IApplicationDbContext, ICurrentUserService, ...
│   │   └── Features/
│   ├── Domain/
│   │   ├── Common/               ← BaseEntity, BaseEvent
│   │   ├── Events/               ← ProjectCreatedEvent
│   │   ├── Models/               ← User, Role, Credentials, UserRole
│   │   └── Projects/             ← Project, ProjectMember, ProjectSpecification,
│   │                               ProjectTransition, ProjectStateMachine, ...
│   └── Persistence/
│       ├── Configurations/       ← EF Fluent API конфигурации
│       ├── ApplicationDbContext.cs
│       └── ApplicationDbContextSeed.cs
├── tests/
│   └── Application.UnitTests/
├── docker-compose.yml
├── global.json
├── PSL.slnx
└── README.md
```

---

## 🛠️ Технологический стек

| Категория | Технология |
|---|---|
| Runtime | .NET 10.0, ASP.NET Core Web API |
| CQRS | MediatR |
| ORM | Entity Framework Core 10.0.3 |
| База данных | PostgreSQL |
| FSM | Stateless |
| Валидация | FluentValidation |
| Аутентификация | JWT Bearer |
| Хэширование паролей | BCrypt.Net |
| Документация API | Swagger / Swashbuckle 7.0.0 |
| Контейнеризация | Docker, Docker Compose |
| Тесты | xUnit, Moq, FluentAssertions, FluentValidation.TestHelper |

---

## ▶️ Запуск

### Требования

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (для запуска через Docker)

### Docker (рекомендуется)

```bash
docker compose up --build
```

При сборке образа **автоматически прогоняются все 334 юнит-теста**. Если хотя бы один тест упадёт — сборка прервётся и контейнер не поднимется. Это гарантирует, что в рабочей среде всегда запускается только проверенный код.

Порядок сборки внутри `Dockerfile`:
```
build → test (334 теста) → publish → final
```

После успешной сборки:

- API: `http://localhost:8080`
- Swagger UI: `http://localhost:8080/swagger`

Остановить:
```bash
docker compose down          # сохранить данные БД
docker compose down -v       # удалить данные БД
```

### Локально

```bash
# Восстановить зависимости
dotnet restore

# Собрать проект
dotnet build

# Применить миграции (PostgreSQL должен быть запущен)
dotnet ef database update --project src/Persistence --startup-project src/Api

# Запустить
dotnet run --project src/Api
```

> Миграции и seed-данные применяются автоматически при старте приложения.

### Seed-данные

При первом запуске автоматически создаются 6 тестовых пользователей — по одному на каждую роль:

| Email | Пароль | Роль |
|---|---|---|
| `admin@test.com` | `Admin1234!` | Admin |
| `pm@test.com` | `Pm123456!` | ProjectManager |
| `dev@test.com` | `Dev12345!` | Developer |
| `tester@test.com` | `Test1234!` | Tester |
| `product@test.com` | `Product1!` | ProductManager |
| `devops@test.com` | `Devops12!` | DevOps |

---

## 🧪 Тестирование

Тесты прогоняются **автоматически при каждой Docker-сборке**. Для ручного запуска:

```bash
# Все тесты
dotnet test

# Только доменные тесты
dotnet test --filter "FullyQualifiedName~Domain"

# С подробным выводом
dotnet test --logger "console;verbosity=detailed"

# В режиме watch
dotnet watch test --project tests/Application.UnitTests
```

Подробная документация по тестам — в [`tests/Application.UnitTests/README.md`](tests/Application.UnitTests/README.md).

---

## 📖 Swagger

Swagger UI — основной интерфейс для демонстрации и тестирования use cases.

Для работы с защищёнными эндпоинтами:
1. Выполнить `POST /api/auth/login`
2. Скопировать `accessToken` из ответа
3. Нажать **Authorize** в правом верхнем углу Swagger UI
4. Ввести `Bearer <accessToken>`

---

## 🪪 Лицензия

MIT License — подробности в файле [LICENSE](LICENSE).

## TEST LINUX
