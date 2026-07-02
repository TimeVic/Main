# TimeVic

**Track time. See money. Know who owes you.**

https://timevic.com

TimeVic is a simple time and income tracking tool for freelance developers and small teams working with multiple clients.

It helps you track work hours, organize tasks around real client work, manage clients and projects, record payments, and turn raw time entries into clear reports. The goal is not just to know how many hours were tracked. The goal is to understand what those hours are worth, what has already been paid, and what is still outstanding.

## Why TimeVic exists

Most time trackers are focused on activity: timers, entries, tasks, and reports.

TimeVic is focused on the outcome of that activity:

- how much you earned;
- which client owes you money;
- which projects are paid or unpaid;
- whether tracked time matches the actual business value of your work.

For freelancers, time tracking only becomes useful when it answers a practical question: **how much money did this work create, and has it been paid?**

## Who it is for

TimeVic is designed for people who work across several clients, projects, and rates:

- freelance developers;
- independent consultants;
- small development teams;
- contractors who need a lightweight way to track time, income, and payments;
- workspace owners who need a clear view of client work and team activity.

It is especially useful when you do not need a heavy ERP system, but a spreadsheet is no longer enough.

## What TimeVic helps with

### Time tracking

Track work time by client, project, task, and workspace. Use time entries to keep a clear history of what was done and when.

### Client and project management

Keep clients, projects, rates, and tracked work connected. This makes reports easier to read and reduces the manual work usually needed before billing.

### Income visibility

Turn tracked hours into financial insight. See what was earned, what was paid, and what remains unpaid.

### Payment tracking

Record client payments and compare them against tracked work. This helps identify unpaid balances without maintaining a separate spreadsheet.

### Reports

Use reports to understand tracked time, client activity, billable work, payments, and outstanding balances.

### Lightweight tasks

Tasks in TimeVic are not meant to replace Jira, ClickUp, or a full project management system. They exist to give time entries useful context and make client work easier to review later.

### Workspace collaboration

Work inside shared workspaces, organize client-related activity, and keep team members aligned around tracked work and payment status.

## The core workflow

TimeVic is built around a short path from first records to useful insight:

1. Add a client.
2. Create a project.
3. Set an hourly rate.
4. Track work time.
5. See earned, paid, and unpaid amounts.

The important moment is not the timer itself. The important moment is seeing what the tracked work means financially.

## Product philosophy

TimeVic avoids trying to become an all-in-one workspace for everyone.

The product is intentionally centered around a narrower problem: helping freelancers and small teams understand their work, income, and unpaid client balances without unnecessary complexity.

The main principles are:

- keep time tracking simple;
- connect time with money;
- make unpaid work visible;
- keep tasks lightweight;
- make reports understandable;
- reduce the need for manual spreadsheets.

## Repository status

TimeVic is under active development. The product is being shaped around practical use cases for freelancers and small teams, with a strong focus on financial clarity rather than generic productivity tracking.

## Tech stack

- Backend: ASP.NET Core on .NET 10.
- Frontend: Blazor WebAssembly with Razor components, Tailwind CSS, and LESS.
- Mobile: .NET MAUI client.
- Database: PostgreSQL with NHibernate, FluentNHibernate, and FluentMigrator.

## Repository structure

- `TimeTracker.Api` - REST API and realtime backend entry point.
- `TimeTracker.Client.Web` - Blazor WebAssembly web app.
- `TimeTracker.Client.Mobile` - .NET MAUI mobile client.
- `TimeTracker.Client.Core` - shared client code used across app surfaces.
- `TimeTracker.Business` and `TimeTracker.Business.*` - domain services, integrations, notifications, and ORM code.
- `TimeTracker.Migrations` - database migration project.
- `TimeTracker.WorkerServices` - background processing services.
- `TimeTracker.Tests.*` - integration and unit test projects.

## Development notes

Contributions should keep product behavior, financial clarity, and existing project boundaries in focus. Avoid adding secrets, generated build output, or local environment details to the repository.

## License

Licensed under the Apache License 2.0. See [LICENSE.md](LICENSE.md).
