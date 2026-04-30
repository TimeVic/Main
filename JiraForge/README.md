# TimeVic Jira Forge Plugin

Minimal Atlassian Forge plugin that lets you start and stop a TimeVic timer directly from a Jira issue.

## Prerequisites

- Node.js 18+
- [Forge CLI](https://developer.atlassian.com/platform/forge/getting-started/) installed globally: `npm install -g @forge/cli`
- A Jira Cloud instance
- A TimeVic account with API access

## Setup

### 1. Install dependencies

```bash
cd JiraForge

# Install backend dependencies
npm install

# Install frontend dependencies
cd static/issue-panel && npm install
cd ../admin-page && npm install
cd ../..
```

Or in one command:

```bash
npm run install:all
```

### 2. Build frontend apps

```bash
npm run build:all
```

### 3. Authenticate with Forge

```bash
forge login
```

### 4. Register the app

```bash
forge register
```

This creates an app ID. The CLI will update `manifest.yml` with the real app ID.

### 5. Deploy

```bash
forge deploy
```

### 6. Install in your Jira site

```bash
forge install
```

Select your Jira Cloud site when prompted.

---

## Usage

### Admin page (Connection settings)

1. Go to **Jira Settings → Apps → Manage your apps**.
2. Find **TimeVic** and click **Configure**.
3. Enter your TimeVic **API Base URL**, **Integration Token**, and **Workspace ID**.
4. Click **Test connection** to verify, then **Save connection**.

### Issue panel

1. Open any Jira issue.
2. Find the **TimeVic** panel in the issue detail view.
3. If not connected, click **Open settings** to configure.
4. If connected, click **Start timer** to begin tracking time.
5. Click **Stop timer** to stop.

---

## Local development (tunnel mode)

Run the Forge tunnel alongside two local React dev servers:

**Terminal 1 — Issue panel dev server (port 3000):**
```bash
cd static/issue-panel && npm start
```

**Terminal 2 — Admin page dev server (port 3001):**
```bash
cd static/admin-page && npm start
```

**Terminal 3 — Forge tunnel:**
```bash
forge tunnel
```

---

## Required TimeVic Backend Endpoints

The following endpoints must exist on the TimeVic API (`https://api.timevic.com`).  
**These are NOT implemented in this plugin** — they must be implemented in the TimeVic backend.

### POST `/api/integrations/jira/test-connection`

Verifies that the provided token and workspace ID are valid.

**Request:**
```json
{
  "workspaceId": "...",
  "token": "..."
}
```

**Response:**
```json
{
  "success": true,
  "workspaceId": "...",
  "workspaceName": "My Workspace"
}
```

---

### POST `/api/integrations/jira/issue/state`

Returns the current timer state and statistics for a Jira issue.

**Request:**
```json
{
  "workspaceId": "...",
  "cloudId": "...",
  "issueId": "...",
  "issueKey": "ABC-123",
  "projectId": "...",
  "projectKey": "ABC",
  "summary": "Fix payment calculation bug"
}
```

**Response:**
```json
{
  "connected": true,
  "issueKey": "ABC-123",
  "taskId": "...",
  "timerActive": false,
  "activeTimeEntryId": null,
  "startedAt": null,
  "todayTrackedSeconds": 3600,
  "totalTrackedSeconds": 14400,
  "billableAmount": 200,
  "currency": "USD",
  "timevicUrl": "https://timevic.com/..."
}
```

---

### POST `/api/integrations/jira/timer/start`

Starts a timer for the given Jira issue.

**Request:**
```json
{
  "workspaceId": "...",
  "cloudId": "...",
  "issueId": "...",
  "issueKey": "ABC-123",
  "projectId": "...",
  "projectKey": "ABC",
  "summary": "Fix payment calculation bug",
  "accountId": "..."
}
```

**Response:**
```json
{
  "timerActive": true,
  "timeEntryId": "...",
  "startedAt": "2026-04-30T10:00:00Z",
  "todayTrackedSeconds": 3600,
  "totalTrackedSeconds": 14400,
  "billableAmount": 200,
  "currency": "USD",
  "timevicUrl": "https://timevic.com/..."
}
```

---

### POST `/api/integrations/jira/timer/stop`

Stops the active timer for the given Jira issue.

**Request:**
```json
{
  "workspaceId": "...",
  "cloudId": "...",
  "issueId": "...",
  "issueKey": "ABC-123"
}
```

**Response:**
```json
{
  "timerActive": false,
  "stoppedTimeEntryId": "...",
  "durationSeconds": 3600,
  "todayTrackedSeconds": 7200,
  "totalTrackedSeconds": 18000,
  "billableAmount": 250,
  "currency": "USD",
  "timevicUrl": "https://timevic.com/..."
}
```

---

## Security

- The Integration Token is stored only in **Forge secure storage** — never in the browser.
- `getSettings` never returns the token — only `connected`, `apiBaseUrl`, `workspaceId`, `workspaceName`.
- All TimeVic API requests go through the Forge resolver (backend), never directly from the UI.
- The token is never logged.

---

## Intentionally excluded from this MVP

The following features are **out of scope** for this version:

- Manual time entry
- Jira Worklog sync
- 2-way sync / scheduled sync
- OAuth (token-based auth only)
- Marketplace listing preparation
- `jira:timeTrackingProvider` module
- Jira project ↔ TimeVic project mapping
- User mapping
- Invoices / payments UI
- Advanced analytics
- Any TimeVic backend endpoint implementation (backend team's responsibility)

---

## Troubleshooting

| Problem | Solution |
|---|---|
| "TimeVic is not connected" in issue panel | Configure the connection in the admin page |
| "TimeVic connection is invalid" | Re-enter your token in settings and save again |
| Admin page not showing | Re-deploy with `forge deploy` and reinstall |
| Timer not updating | Refresh the Jira page |
