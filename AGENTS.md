# AGENTS.md – TimeVic (TimeTracker) Codebase Guide

---

## 1. System & Architecture Overview

**TimeVic** is an ASP.NET Core time-tracking platform (.NET 10, `global.json` pins SDK with `rollForward: latestMajor`).

### Key Projects
| Project | Role |
|---|---|
| `TimeTracker.Api` | REST API + SignalR WebSocket host |
| `TimeTracker.WorkerServices` | Background queue processors (hosted services) |
| `TimeTracker.Migrations` | FluentMigrator console app (PostgreSQL) |

Solution folders in `TimeTracker.sln`: **Infrastructure** (reusable cross-cutting libs), **Business**, **Tests**.

### Dependency Injection (Autofac)
- DI uses **Autofac modules** (not standard `IServiceCollection`). Each assembly exposes modules via `RegisterAssemblyModules(assembly)`.
- `IDomainService` → `InstancePerDependency`; `IScopedDomainService` → `InstancePerLifetimeScope`.
- DAOs implement `IDomainService` and are resolved from scope automatically.
- Key modules in `TimeTracker.Business/Di/Autofac/Modules/`: `DbModule`, `DomainModule`, `QueueModule`, `NotificationsModule`, `ExternalClientsModule`.

### Configuration
- `appsettings.json` → `appsettings.{ASPNETCORE_ENVIRONMENT}.json` → `appsettings.Local.json` (gitignored for local secrets).
- DB connection key: `ConnectionStrings:DefaultConnection`.
- Set `Hibernate:IsShowSql: true` to log SQL queries.

---

## 2. Execution & Implementation Plan Rules

1. **Strict Plan Adherence:**
   - Whenever an implementation plan or numbered list of requirements is provided, you MUST implement EVERY single item in order.
   - Do NOT skip, summarize, or leave placeholders (TODOs) for intermediate steps unless explicitly requested.

2. **Pre-Execution / Post-Execution Checklist (Mandatory):**
   - Before outputting or editing final code, perform a self-check confirming all N requirements/steps from the plan are covered.
   - If any requirement cannot be met or conflicts with existing code, explicitly state it instead of silently omitting it.

### General Implementation Standards
- **English Only**: Write all comments, code documentation, and commit/task notes in English.
- **No Git Commits**: Do not create git commits directly unless explicitly requested.
- **Issue Fix Documentation**: When adding a fix for a specific issue or bug, add a concise comment describing the resolved problem. Avoid meaningless comments like `@* Added: Invalid *@`.

---

## 3. Backend Architecture & API Standards

### Request / Handler Pattern (NOT MediatR)
All API endpoints use a custom `IAsyncRequestHandler<TRequest, TResponse>` pattern — **not** MediatR.

**Workspace Context:**
The current active workspace ID is passed from clients in the `Workspace-Id` HTTP header (`AuthConstants.WorkspaceIdHeaderName`), not as part of the request payload or route. Request handlers retrieve it via `_apiRequestService.GetCurrentWorkspaceId()` and resolve the entity via `_userDao.GetUsersWorkspace(user, workspaceId)`.

**Controller Layout & Dispatch:**
```
Controllers/Dashboard/TimeEntry/
  TimeEntryController.cs          ← thin controller, route definitions only
  Actions/
    StartRequestHandler.cs        ← all business/action logic lives here
    StopRequestHandler.cs
```

Controllers extend `MainApiControllerBase(ILifetimeScope scope)` and dispatch via:
```csharp
// With response payload:
=> this.RequestAsync().For<TimeEntryDto>().With(request);

// Without response (void):
=> this.RequestAsync(request);
```

Handlers implement `IAsyncRequestHandler<TRequest, TResponse>` and are auto-registered by Autofac in `ApiModule.cs` using `AsClosedTypesOf`.

### DTOs & AutoMapper
- One class/DTO per file.
- Entity→DTO mapping profiles live in `TimeTracker.Api/Profiles/Api/` (one `Profile` per entity, e.g. `TimeEntryProfile`).
- DTOs live in `TimeTracker.Api.Shared/Dto/Entity/`.
- Request / Response types live in `TimeTracker.Api.Shared/Dto/RequestsAndResponses/`.
- **Avoid Lazy Loading in AutoMapper via `IgnoreAllAndConstructUsing`**: Never use `.ForMember(...)` directly on entity-to-DTO mappings without ignoring all members. Default AutoMapper member traversal touches all properties, triggering unintentional loading of lazy-loaded fields/relationships across the entire Entity tree. Always map entities using `.IgnoreAllAndConstructUsing((src, mapper) => new MyDto { ... })` (from `TimeTracker.Business.Extensions.AutoMapperExtensions`).

### Exception & Error Handling
- Domain exceptions implement `IDomainException` → intercepted by `ExceptionHandlerActionFilter` → returns HTTP 400:
  ```json
  { "status": "fail", "errorCode": "ExceptionClassName", "message": "..." }
  ```
- Use static helper methods on pre-built exceptions:
  - `RecordNotFoundException.ThrowIfNull(entity)` (when an entity is missing)
  - `throw new HasNoAccessException()` (when permission is denied)
  - `throw new RecordIsExistsException()` (on unique constraint / duplication conflict)
  - See `TimeTracker.Business.Common/Exceptions/Api`.
- Non-domain exceptions produce HTTP 500 with `"Server error"` (details logged via Serilog).

### Queue System
Internal DB-backed queue with 3 channels: `Default`, `Notifications`, `ExternalClient`.

**To enqueue work:**
1. Create a context class implementing `IQueueItemContext`, `INotificationItemContext`, or `IExternalServiceItemContext`.
2. Create a handler implementing `IAsyncQueueHandler<TContext>` (auto-registered via `QueueModule`).
3. Push via `IQueueService.PushDefaultAsync(ctx)` / `PushNotificationAsync(ctx)` / `PushExternalClientAsync(ctx)`.

Context type is serialized as JSON; its `FullName` is stored for handler resolution at runtime. See `QueueService.cs` and `TimeTracker.WorkerServices/Services/Queue/`.

---

## 4. ORM, Database & Migrations

### NHibernate & Entities
- **PostgreSQL** (default port `5433` in testing/appsettings).
- All entities extend `AEntity` (provides `Id: Guid`, `CreatedAt`, `UpdatedAt`, `DeletedAt`, `IsDeleted`, `IsNew`).
- Primary keys are **UUID v7** (`GuidV7Generator`) — generated by the application, never the database.
- Mappings live in `TimeTracker.Business.Orm/Mapping/Entities/` and extend `BaseGuidMappings<T>`.
- **`SnakeCaseConvention`** converts `PascalCase` → `snake_case` automatically for table and column names; specify explicit overrides in the mapping class only when needed.
- Initialize `DateTime` fields with `DateTime.UtcNow` by default.

### Transactions & Persistence
- `CommitPerformerMiddleware` wraps each HTTP request in a transaction (commits on success, rolls back on exception). Do not manually commit in handlers.
- **Entity State & Persistence**: Do NOT call `SaveAsync` or `SaveOrUpdateAsync` on existing entities loaded in the NHibernate session. NHibernate dirty tracking automatically flushes changes on transaction commit (`CommitPerformerMiddleware`) or `FlushDbChanges()`.
- Call `await Session.SaveAsync(entity)` **only** for newly created entities (`IsNew` is true or unattached instances).
- **Avoid N+1 queries**: When adding or modifying DAO query methods, check for potential N+1 queries from lazy-loaded relationships during DTO mapping; use explicit eager fetching or projections.

### Database Migrations
- Create migrations using FluentMigrator mechanisms (classes, helpers, etc.).
- Use `timestamp` column type for date/time fields.
- Do not specify custom names for indexes created with FluentMigrator; use `Create.Index()` default naming. Specify explicit index names only for raw `CREATE INDEX` SQL.
- Run migrations strictly via the `TimeTracker.Migrations` project:
  ```bash
  dotnet run --project ./TimeTracker.Migrations
  ```

---

## 5. Frontend & Blazor Standards

### Component Structure & File Conventions
- **Page components**: `<SomeName>Page.razor` (e.g. `LoginPage.razor`).
- **Partial/block components**: `<SomeName>Block.razor` (e.g. `ProfileBlock.razor`).
- **Layout components**: `<SomeName>Layout.razor` (e.g. `MainLayout.razor`).
- **Form components**: `<SomeName>Select.razor`, `<SomeName>Input.razor`, etc.
- **Code-behind**: Create a `*.razor.cs` file for each component when additional business logic is needed.
- **Styles**: Create `*.razor.less` files for component-scoped styles (do **not** edit `*.razor.css` directly; CSS is generated from LESS during build and gitignored).
- **Component Decomposition**: If a page/component exceeds 200 lines, decompose it into sub-blocks/partial components with dedicated child directories.
- **Shared Placement**: If a component is used in more than 1 place, place it in the nearest `Shared/` directory.

### HTML & Razor Formatting Rules
- Do not write inline HTML.
- All HTML and Razor templates must be formatted with opening and closing tags on new lines and indented appropriately.
- When a Blazor component has more than one attribute, place each attribute on a separate line.

### UI Components & Tailwind CSS Guidelines
- Built on **Tailwind CSS v4**.
- **Custom Native Components**: All UI components are native Blazor components located in `TimeTracker.Client.Core/Ui/Shared/Components/` (`Dropdown/`, `Popover/`, `Modal/`, `Form/`, `Button/`, `Card/`, `Table/`, `Tabs/`, `Badge/`, `Spinner/`).
- **Do NOT use external UI component libraries** (e.g. LumexUI, MudBlazor). Use the built-in native components or create new ones in `TimeTracker.Client.Core/Ui/Shared/Components/`.
- **Component Enums & Tokens**: Use shared component enums from `TimeTracker.Client.Core.Ui.Shared.Components.Enums` (`ComponentColor`, `ComponentSize`, `ButtonVariant`, `ButtonRadius`, `BadgeVariant`, `BadgeRadius`, `SpinnerVariant`, `DropdownAlignment`, `PopoverPlacement`, `PopoverTriggerMode`).

### Localization Rules for UI Components
Do not hardcode user-facing text in Razor/HTML/C# components. All visible strings must be added to localization resource files for both supported locales:
- `en` — English (default)
- `uk-UA` — Ukrainian

**Key Guidelines:**
- Use descriptive keys (e.g. `Hero_Title`, `Button_Save`, `Menu_Settings`, `Payments_OutstandingBalance`). Avoid generic keys like `Text1`, `Label3`.
- Use `IStringLocalizer<T>` or project localization abstractions.
- Keep resource keys strictly identical across both locales. Do not leave English fallbacks in `uk-UA`.
- Do not localize brand/product names (`TimeVic`, `Jira`, `CSV`, `PDF`).
- For public SEO pages, localize `page title`, `meta description`, `CTA labels`, `navigation labels`, and `footer labels`.

**Terminology Conventions:**
- **English**: Keep wording concise, clear, product-focused for freelancers (`earned`, `paid`, `outstanding`, `client`, `project`, `time entry`). Use short labels like `Search` instead of verbose placeholders.
- **Ukrainian**: Adapt naturally for freelancers/small teams:
  - `проєкт` (not `проект`)
  - `облік часу` (for formal time tracking UI)
  - `трекати`, `затрекано`, `затреканий` (acceptable in product context)
  - `Earned` → `Зароблено`
  - `Paid` → `Оплачено`
  - `Outstanding Balance` → `Неоплачений залишок`
  - `Time Entries` → `Записи часу`
  - `Payments` → `Оплати`
  - `Clients` → `Клієнти`
  - `Projects` → `Проєкти`

---

## 6. Testing Standards

### Test Projects Overview
| Project | Type | Base Class / Environment |
|---|---|---|
| `TimeTracker.Tests.Integration.Api` | API integration (HTTP endpoints) | `BaseTest : IClassFixture<ApiCustomWebApplicationFactory>` |
| `TimeTracker.Tests.Integration.Business` | Business & DAO integration | `BaseTest` (builds Autofac container directly) |
| `TimeTracker.Tests.Unit.Business` | Unit tests | `BaseUnitTest` |

### Test Execution Rules
- **Run strictly sequentially**: Run test projects and test suites one at a time because they share a single test database (`port 5433`, credentials in `appsettings.Testing.json`).
- Wait until the current `dotnet test` process exits completely before starting another run. **Never run test suites in parallel**.
- Test database is automatically cleaned before each test via `IDbCleanUpService.CleanUp()` in `BaseTest`.

```bash
dotnet test ./TimeTracker.Tests.Integration.Api
dotnet test ./TimeTracker.Tests.Integration.Business
dotnet test ./TimeTracker.Tests.Unit.Business
```

### Test Data, Seeding & Utilities
- **Factories**: Use `IDataFactory<TEntity>` (e.g. `_userFactory.Generate()`) for Faker-generated entity stubs.
- **Seeders**: Use `IUserSeeder.CreateAuthorizedAsync()` to obtain `(jwtToken, user, defaultWorkspace)` or entity seeders (`ITimeEntrySeeder`, `IWorkspaceSeeder`).
- **Session Flushing**: Call `await FlushDbChanges()` before re-querying the database in assertions; use `FlushAndRefreshEntity(entity)` to reload NHibernate tracked instances.
- **Queue Processing**: Process background queues synchronously in tests via `await QueueProcess(QueueChannel.Default)` (or `QueueChannel.Notifications`).
- **Mocks**: External services are pre-mocked (`SmtpClientServiceMock`, `FirebaseClientServiceMock`, `ClickUpClientMock`, `RedmineClientMock`, `JiraClientMock`).

### Test Organization & Coverage Requirements
- **One Dedicated File per API Route**: Keep tests for a single API URL in one dedicated test file. Do not mix unrelated routes into one test file.
- **Multi-URL Scenarios**: A test may cover multiple URLs only when testing their explicit interaction (document this in the test name or with a short comment).
- **Mandatory Negative Flow Coverage**: Every new or modified API endpoint must include tests for negative and edge scenarios:
  - Unauthorized access (`PostRequestAsAnonymousAsync` → 401 Unauthorized).
  - Validation failures (`[Required]`, custom validation attributes, empty/malformed inputs → 400 BadRequest).
  - Missing entities (`RecordNotFoundException` → 400 BadRequest).
  - Duplicate / conflicting records (`RecordIsExistsException` → 400 BadRequest).
  - Permission and role restrictions (`HasNoAccessException` → 400 BadRequest).

---

## 7. Code Style & General Conventions

- **Boolean Naming**: Property, field, and variable names with boolean values must start with the prefix `Is` or `Has` (e.g. `IsGroupByClient`, `HasAccess`; incorrect: `GroupByClient`).
- **Interface Naming**: Interface names must start with `I` (e.g. `IProjectService`, `IUserDao`).
- **Class Member Ordering**: Nested classes, interfaces, records, and types must be placed at the top of the enclosing class.
- **One Class per File**: Keep each class, interface, DTO, and request/response type in its own separate `.cs` file.
