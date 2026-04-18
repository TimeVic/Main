---
name: timevic-testing
description: Write, update, and run TimeVic tests. Use for API integration tests, business and DAO integration tests, unit tests, test data setup with seeders or factories, queue processing assertions, mocked external clients, regression coverage, and choosing the right dotnet test command.
---

# TimeVic Testing

## Overview

Use this skill to add focused coverage for TimeVic changes. The repository has API integration tests, business integration tests, unit tests, shared testing helpers, seeders, factories, mocked external clients, and synchronous queue processing helpers.

## Choose Test Level

- Use `TimeTracker.Tests.Integration.Api` for HTTP endpoints, auth behavior, model binding, response status codes, and request/handler integration through the real API host.
- Use `TimeTracker.Tests.Integration.Business` for DAOs, domain services, queue handlers, NHibernate mappings, and business workflows that need the database but not HTTP.
- Use `TimeTracker.Tests.Unit.Business` for pure logic, helper methods, validators, small services, and behavior that can run without PostgreSQL or Autofac-heavy setup.
- Add tests near existing tests for the same feature before creating new folders.

## Core Patterns

- Test classes inherit from the local `BaseTest` or `BaseUnitTest`.
- API integration tests use `BaseTest : IClassFixture<ApiCustomWebApplicationFactory>`.
- Business integration tests build the Autofac container directly through their base class.
- The database is cleaned before each test through `IDbCleanUpService.CleanUp()`.
- Use `IUserSeeder.CreateAuthorizedAsync()` to get `(jwtToken, user, defaultWorkspace)` for authorized API scenarios.
- Use `IDataFactory<TEntity>.Generate()` for Faker-generated entity stubs.
- Call `FlushDbChanges()` before assertions that re-query the database.
- Use `FlushAndRefreshEntity(entity)` when assertions need a refreshed NHibernate-tracked object.
- For queued work, enqueue normally and call `await QueueProcess(QueueChannel.Default)` or the relevant channel helper before asserting side effects.

## API Test Checklist

- Seed an authorized user and workspace unless the endpoint is public.
- Send requests through the existing HTTP helpers used in nearby tests.
- Assert both HTTP result shape and persisted side effects.
- Cover access control when the endpoint reads or mutates workspace data.
- Cover domain failures with expected HTTP 400 `errorCode` when the behavior is important.

## Business Test Checklist

- Resolve DAOs and services from the test container.
- Prefer real NHibernate persistence for DAO behavior, mappings, and query correctness.
- Use external client mocks already provided by the testing infrastructure: SMTP, Firebase, ClickUp, Redmine, and Jira.
- Verify queue items through the queue processing helper instead of calling queue handlers directly unless the handler is the unit under test.

## Unit Test Checklist

- Keep unit tests independent from PostgreSQL, real file storage, network calls, and hosted services.
- Mock only boundaries; avoid over-mocking simple value objects or pure helper methods.
- Prefer clear arrange/act/assert structure over excessive helpers.
- Add regression tests for bug fixes using the smallest setup that reproduces the bug.

## Commands

```bash
dotnet test ./TimeTracker.Tests.Integration.Api
dotnet test ./TimeTracker.Tests.Integration.Business
dotnet test ./TimeTracker.Tests.Unit.Business
```

Integration tests require PostgreSQL on port `5433` with credentials from `appsettings.Testing.json`.

## Quality Bar

- Tests should describe observable behavior, not implementation details.
- Assertions should prove the behavior that could regress.
- Keep test data minimal but realistic enough to exercise security, ownership, dates, totals, and pagination.
- When a test cannot be run locally because a dependency is unavailable, state the exact missing prerequisite.
