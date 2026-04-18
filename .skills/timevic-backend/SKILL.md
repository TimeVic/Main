---
name: timevic-backend
description: Implement and maintain the TimeVic backend. Use for ASP.NET Core API endpoints, custom request handlers, business services, DAOs, NHibernate entities and mappings, FluentMigrator migrations, Autofac registration, queue handlers, domain exceptions, AutoMapper profiles, and backend build or behavior fixes.
---

# TimeVic Backend

## Overview

Use this skill for backend changes across `TimeTracker.Api`, `TimeTracker.Business`, `TimeTracker.Business.Orm`, shared API contracts, migrations, and worker services. TimeVic uses ASP.NET Core with Autofac, a custom request/handler pattern, NHibernate, PostgreSQL, FluentMigrator, and an internal DB-backed queue.

## Endpoint Workflow

1. Put request and response contracts in `TimeTracker.Api.Shared/Dto/RequestsAndResponses/...`.
2. Put entity DTOs in `TimeTracker.Api.Shared/Dto/Entity/...` when the response exposes reusable entity data.
3. Add or update `ApiUrl` constants when the frontend will call the endpoint.
4. Keep controllers thin. Controllers should route and dispatch only:

```csharp
=> this.RequestAsync()
    .For<ResponseDto>()
    .With(request);
```

For no-response endpoints:

```csharp
=> this.RequestAsync(request);
```

5. Put logic in `Actions/*RequestHandler.cs`.
6. Implement `IAsyncRequestHandler<TRequest, TResponse>` or the no-response variant used nearby.
7. Rely on `CommitPerformerMiddleware` for transactions. Do not manually commit in handlers.
8. Enforce workspace and role access with existing security services before mutating data.

## Dependency Injection

- Use Autofac modules, not ad hoc `IServiceCollection` registration.
- Handlers are auto-registered through `ApiModule` using `AsClosedTypesOf`; avoid manual handler registration unless the existing module requires it.
- Domain services implement `IDomainService` or `IScopedDomainService`.
- DAOs live under `TimeTracker.Business.Orm/Dao` and are resolved from the Autofac scope.
- Keep service lifetimes aligned with the marker interface conventions.

## ORM And Migrations

- Entities extend `AEntity` and use app-generated UUID v7 identifiers.
- Mappings live in `TimeTracker.Business.Orm/Mapping/Entities` and extend `BaseGuidMappings<T>`.
- `SnakeCaseConvention` maps PascalCase names to snake_case automatically; add explicit columns only when needed for relationships or legacy names.
- Use NHibernate async APIs and existing DAO query helpers.
- Add FluentMigrator migrations for schema changes in `TimeTracker.Migrations/Migrations`.
- Keep entity, mapping, migration, DAO, DTO, and AutoMapper profile changes consistent.

## Queues

Use the internal DB-backed queue for deferred work:

1. Create a context implementing `IQueueItemContext`, `INotificationItemContext`, or `IExternalServiceItemContext`.
2. Create an `IAsyncQueueHandler<TContext>` handler.
3. Push through `IQueueService.PushDefaultAsync`, `PushNotificationAsync`, or `PushExternalClientAsync`.
4. Keep context classes JSON-serializable; handler resolution uses the context `FullName`.

## Exceptions And Responses

- Throw domain exceptions for user-correctable failures. They are returned as HTTP 400 by `ExceptionHandlerActionFilter`.
- Use helpers such as `RecordNotFoundException.ThrowIfNull(entity)` where they fit.
- Throw `HasNoAccessException` for authorization failures that match existing behavior.
- Do not leak internal exception details in API responses.

## AutoMapper

- API-facing entity mappings live in `TimeTracker.Api/Profiles/Api`.
- Add one profile per entity or feature when matching nearby structure.
- Keep mapping logic minimal; complex calculations belong in services or handlers.

## Verification

Use the narrowest useful build or test command:

```bash
dotnet build ./TimeTracker.Api
dotnet build ./TimeTracker.Business
dotnet build ./TimeTracker.Business.Orm
dotnet run --project ./TimeTracker.Migrations
```

Run migrations only when schema changes are included and the configured PostgreSQL instance is available.
