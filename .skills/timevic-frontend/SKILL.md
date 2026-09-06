---
name: timevic-frontend
description: Build and maintain the TimeVic frontend. Use for Blazor WebAssembly UI work in TimeTracker.Web, Razor components, code-behind files, Fluxor store changes, ApiService client methods, public landing pages that require UI implementation, styling with Less/CSS, and frontend build or behavior fixes.
---

# TimeVic Frontend

## Overview

Use this skill to implement frontend changes in `TimeTracker.Web`. The app is a Blazor WebAssembly project using Razor components, code-behind classes, native custom UI components, Fluxor state, shared API DTOs, and Less/CSS assets.

## Project Map

- `TimeTracker.Web/Ui/Pages`: active page and feature components.
- `TimeTracker.Web/Ui/Shared`: shared layouts and reusable UI.
- `TimeTracker.Web/Services/Http/ApiService.*.cs`: typed API client partials.
- `TimeTracker.Web/Store/*`: Fluxor actions, reducers, effects, and state.
- `TimeTracker.Web/wwwroot`: static assets and appsettings.
- `TimeTracker.Api.Shared`: request, response, constants, and DTO contracts shared with the API.
- `TimeTracker.Web/Ignore`: parked or excluded code; do not add new active work here unless explicitly requested.

## Workflow

1. Inspect nearby components, store files, and API client methods before changing patterns.
2. Identify whether the change is presentational, stateful, API-backed, or cross-cutting.
3. Keep markup in `.razor` and interaction logic in `.razor.cs` when the local component already uses code-behind.
4. Reuse shared DTOs and request/response types from `TimeTracker.Api.Shared`; do not create frontend-only copies of API contracts.
5. For API calls, add methods to the relevant `ApiService.*.cs` partial and use `ApiUrl` constants.
6. For stateful flows, update Fluxor actions, reducers, effects, and state in the same feature folder.
7. Keep styling local to the component when possible: `.razor.less` or existing scoped style files. Match nearby naming and spacing.
8. Build after implementation and run the narrowest useful verification command.

## Component Rules

- Prefer native custom UI components in `TimeTracker.Client.Core/Ui/Shared/Components/` and local component abstractions.
- Preserve existing split files: `Component.razor`, `Component.razor.cs`, and optional `Component.razor.less`.
- Use dependency injection and inherited base classes the same way nearby components do.
- Avoid adding user-facing instructional copy inside product UI unless the requested feature requires it.
- Keep UI dense and scannable for dashboard workflows: tables, lists, reports, timers, and forms should prioritize fast task completion.
- Do not introduce unrelated layout redesigns while implementing feature work.
- Keep comments in English and only add them for non-obvious logic.

## State And API

- Use Fluxor for state already represented in `TimeTracker.Web/Store`.
- Use record structs for actions when matching existing store style.
- Reducers should be deterministic and should avoid side effects.
- Put async work, API calls, toast handling, navigation side effects, and dispatch chains in effects or services, following the local feature pattern.
- Use `ApiService` typed methods instead of calling `HttpClient` directly from components.
- Keep endpoint constants in `TimeTracker.Api.Shared.Constants.ApiUrl` when adding new backend endpoints.

## Styling

- Use existing visual language before introducing new colors, spacing, or component shapes.
- Prefer stable dimensions for fixed-format controls such as timers, icon buttons, counters, table action cells, and kanban columns.
- Check that text fits in buttons and cards on both mobile and desktop sizes.
- Avoid one-off decorative gradients, large rounded cards, and nested card layouts unless the existing screen already establishes that pattern.
- For public pages, coordinate copy decisions with `timevic-content`; for UI implementation, follow this skill.

## Verification

Use the narrowest command that proves the change:

```bash
dotnet build ./TimeTracker.Web
```

For API-backed UI changes, also build the shared/API projects touched by the contract change.
