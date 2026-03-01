# Application.UnitTests

Юнит-тесты для `RedRamkaPractice`. Покрывают доменную логику, application-слой (команды, запросы, валидаторы) и сервисы в изоляции, без обращения к базе данных или HTTP.

**334 теста** · xUnit · Moq · FluentAssertions

---

## Быстрый старт

```bash
# Запустить все тесты
dotnet test

# Запустить конкретный класс
dotnet test --filter "FullyQualifiedName~ProjectEdgeCaseTests"

# Запустить по категории (только домен)
dotnet test --filter "FullyQualifiedName~Domain"

# С подробным выводом
dotnet test --logger "console;verbosity=detailed"

# В режиме watch
dotnet watch test
```

---

## Структура проекта

```
Application.UnitTests/
│
├── Common/
│   ├── HandlerTestBase.cs          ← базовый класс с фабриками тестовых данных
│   └── TestAsyncQueryProvider.cs   ← хелперы для async EF Core DbSet
│
├── Domain/                         ← 164 теста, наиболее важные
│   ├── BaseEntityTests.cs          ← механизм доменных событий
│   ├── BaseEventTests.cs           ← OccurredOn, UTC-гарантии
│   ├── DomainModelTests.cs         ← User, Role, Credentials, UserRole, RoleConstants
│   ├── ProjectTests.cs             ← основные сценарии Project
│   ├── ProjectEdgeCaseTests.cs     ← все недопустимые переходы, Reason в Transition
│   ├── ProjectExtendedTests.cs     ← UpdatedAt, CreatedAt, domain events, versioning
│   ├── ProjectMemberAndTransitionTests.cs
│   ├── ProjectSpecificationTests.cs ← Approve/Revoke/идемпотентность
│   ├── ProjectStateMachineTests.cs
│   ├── ProjectStateMachineExtendedTests.cs ← все невалидные триггеры из каждой стадии
│   └── ProjectTransitionResultTests.cs
│
├── Validators/                     ← 37 тестов
│   ├── CommandValidatorTests.cs    ← Archive, FailQA, ReturnToDesign,
│   │                                  CreateProject, CreateSpecification
│   └── AddProjectMemberValidatorTests.cs
│
├── Application/
│   ├── Auth/
│   │   ├── LoginCommandHandlerTests.cs
│   │   ├── RegisterCommandHandlerTests.cs
│   │   └── RefreshTokenCommandHandlerTests.cs
│   ├── Credentials/
│   │   └── ChangePasswordCommandHandlerTests.cs
│   ├── Projects/
│   │   ├── CreateProjectCommandHandlerTests.cs
│   │   ├── StartDevelopmentCommandHandlerTests.cs
│   │   ├── TransitionCommandHandlerTests.cs  ← FailQA, PassQA, Release,
│   │   │                                        SendToQA, ReturnToDesign, Archive
│   │   ├── ProjectMemberCommandHandlerTests.cs
│   │   ├── ProjectQueryHandlerTests.cs
│   │   └── SpecificationHandlerTests.cs
│   ├── Users/
│   │   └── UserHandlerTests.cs
│   ├── MissingHandlerTests.cs      ← Revoke, AssignRole, CreateRole,
│   │                                  DeleteRole, GetAllRoles, GetUserById,
│   │                                  GetAllUsers, GetSpecificationById
│   └── InfrastructureTests.cs      ← ProjectCreatedEventHandler,
│                                      DateTimeService, TransitionResult
│
└── Services/
    ├── CurrentUserServiceTests.cs
    ├── PasswordHasherTests.cs
    ├── ProjectAuthorizationServiceTests.cs
    └── TokenServiceTests.cs
```

---

## Покрытие по категориям

### Домен (приоритет №1)

Тесты не зависят ни от каких моков — только чистая логика сущностей.

| Файл | Что проверяется |
|---|---|
| `ProjectTests` | Базовые сценарии: создание, спецификации, участники, переходы |
| `ProjectEdgeCaseTests` | Каждый метод вызывается из каждой недопустимой стадии; `Reason` сохраняется в `Transition`; многократные QA-циклы |
| `ProjectExtendedTests` | `UpdatedAt` обновляется только при успешном переходе; `CreatedAt`/`ArchivedAt`; `HasApprovedSpecification`; авто-версионирование |
| `ProjectStateMachineExtendedTests` | `CanFire` для всех триггеров из всех стадий; полный цикл; мутатор и аксессор состояния |
| `ProjectSpecificationTests` | `Approve` → `Revoke`; идемпотентность; `ApprovedAt` |
| `ProjectMemberAndTransitionTests` | `JoinedAt`, `ChangedAt`, уникальность Id |
| `ProjectTransitionResultTests` | `Success` / `Fail` factory-методы; все стадии через `[Theory]` |
| `BaseEntityTests` | `AddDomainEvent`, порядок, `ClearDomainEvents`, повторное добавление |
| `BaseEventTests` | `OccurredOn` в UTC; монотонность |
| `DomainModelTests` | `User`, `Role`, `Credentials`, `UserRole`, `RoleConstants` (уникальность) |

### Валидаторы

Используют `FluentValidation.TestHelper` — `TestValidate` + `ShouldHaveValidationErrorFor`.

| Валидатор | Что проверяется |
|---|---|
| `ArchiveCommandValidator` | `Reason`: empty, whitespace, 1000/1001 символов |
| `FailQACommandValidator` | `Reason`: empty, 1000/1001 |
| `ReturnToDesignCommandValidator` | `Reason`: empty, 1000/1001 |
| `CreateProjectCommandValidator` | `Name`: empty, 200/201, спецсимволы, Unicode; `Description`: null/empty/1000/1001; `MemberIds`: `Guid.Empty`, смешанный список, null |
| `CreateSpecificationCommandValidator` | `Content`: empty, whitespace, 10000/10001 |
| `AddProjectMemberCommandValidator` | Разрешённые роли (Developer/Tester/ProductManager/DevOps); запрещённые (ProjectManager, Admin, неизвестная) |

### Application-слой

Все обработчики мокируются через Moq. `IApplicationDbContext` поднимается через `HandlerTestBase.CreateContextMock()` с `TestAsyncQueryProvider` для поддержки EF Core async-операций.

Стандартный набор проверок для каждого обработчика:
- не аутентифицирован → `UnauthorizedAccessException`
- сущность не найдена → `InvalidOperationException`
- успешный путь → корректный DTO / `TransitionResult`
- вызов `SaveChangesAsync` при мутации (через `Verify`)

### Сервисы

| Сервис | Что проверяется |
|---|---|
| `PasswordHasher` | BCrypt: хэш ≠ plain text; разные хэши одного пароля (соль); верификация; набор паролей через `[Theory]` |
| `TokenService` | JWT читаем; содержит все клеймы (`NameIdentifier`, `Email`, `Name`, `Role`); срок ≈ 2 часа; корректные issuer/audience |
| `CurrentUserService` | Парсинг `UserId` из клейма; null при невалидном Guid; `IsAuthenticated`; `IsInRoleAsync`; null `HttpContext` |
| `ProjectAuthorizationService` | `IsAdminAsync`; `RequireProjectMemberAsync` (Admin/Member/не-член); `RequireProjectRoleAsync` (правильная/неправильная роль; несколько допустимых) |

---

## Инфраструктура тестов

### HandlerTestBase

Базовый класс, от которого наследуются все тесты обработчиков.

```csharp
// Создать мок контекста с нужными данными
var contextMock = CreateContextMock(
    users: new[] { user },
    roles: new[] { pmRole, devRole },
    projects: Enumerable.Empty<Project>());

// Фабрики тестовых данных
var user = CreateUser(userId, "Alice", "Smith");
var role = CreateRole(name: RoleConstants.ProjectManager);
var creds = CreateCredentials(userId, "alice@test.com", refreshToken: "token");
```

### TestAsyncQueryProvider

Позволяет мокировать `DbSet<T>` с поддержкой `FirstOrDefaultAsync`, `AnyAsync`, `ToListAsync` и других async EF Core методов. Подключается автоматически внутри `CreateContextMock`.

---

## Соглашения об именовании

**Классы:** `{ТестируемыйКласс}Tests`  
**Методы:** `{Метод}_{Сценарий}_{ОжидаемоеПоведение}`

```csharp
// Примеры
StartDevelopment_WithoutApprovedSpec_ShouldFail()
Handle_WhenNotAuthenticated_ShouldThrowUnauthorized()
Reason_WhenExceeds1000Chars_ShouldHaveValidationError()
CanFire_ShouldReturnExpectedResult()  // [Theory]
```

---

## Паттерны

### Arrange-Act-Assert

```csharp
[Fact]
public void ApproveSpecification_ShouldRevokePreviousApproved()
{
    // Arrange
    var project = new Project("Test", "Desc");
    project.AddSpecification("v1");
    project.AddSpecification("v2");
    var firstId = project.Specifications.First().Id;
    var secondId = project.Specifications.Last().Id;
    project.ApproveSpecification(firstId);

    // Act
    project.ApproveSpecification(secondId);

    // Assert
    project.Specifications.First(s => s.Id == firstId).IsApproved.Should().BeFalse();
    project.Specifications.First(s => s.Id == secondId).IsApproved.Should().BeTrue();
}
```

### Theory для граничных значений

```csharp
[Theory]
[InlineData(RoleConstants.Developer)]
[InlineData(RoleConstants.Tester)]
[InlineData(RoleConstants.ProductManager)]
[InlineData(RoleConstants.DevOps)]
public void RoleName_WhenAllowedRole_ShouldPassValidation(string role)
{
    var result = _sut.TestValidate(ValidCommand() with { RoleName = role });

    result.ShouldNotHaveValidationErrorFor(x => x.RoleName);
}
```

### Проверка вызовов через Verify

```csharp
[Fact]
public async Task Handle_WhenUserExists_ShouldRemoveAndSave()
{
    var role = CreateRole();
    var contextMock = CreateContextMock(roles: new[] { role });
    contextMock.Setup(x => x.Roles.Remove(It.IsAny<Role>()));
    var handler = new DeleteRoleCommandHandler(contextMock.Object);

    await handler.Handle(new DeleteRoleCommand(role.Id), CancellationToken.None);

    contextMock.Verify(x => x.Roles.Remove(role), Times.Once);
    contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

---

## Зависимости

```xml
<PackageReference Include="coverlet.collector" Version="8.0.0">
		<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
		<PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="FluentAssertions" Version="8.8.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="NSubstitute" Version="5.3.0" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.5">
		<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
		<PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.3" />
```

---

## Типичные ошибки

**Тест падает на async-методах EF Core (`FirstOrDefaultAsync` и т.д.)**  
Убедитесь, что `DbSet<T>` создан через `CreateContextMock` — он автоматически подключает `TestAsyncQueryProvider`.

**`Verify` не срабатывает для `.Add()` / `.Remove()`**  
Нужно явно настроить setup перед созданием обработчика:
```csharp
contextMock.Setup(x => x.Projects.Add(It.IsAny<Project>()));
```

**Validator-тест не находит ошибку**  
Используйте `TestValidate()`, а не `Validate()`. Выражение `x => x.PropertyName` должно совпадать точно.

**`Should().Throw<>()` на async-методе**  
Используйте `await act.Should().ThrowAsync<>()`:
```csharp
var act = async () => await handler.Handle(command, CancellationToken.None);
await act.Should().ThrowAsync<UnauthorizedAccessException>();
```