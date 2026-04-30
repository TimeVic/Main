import { invoke, view, requestJira } from '@forge/bridge';

export interface JiraContext {
  cloudId: string;
  issueId: string;
  issueKey: string;
  projectId: string;
  projectKey: string;
  summary: string;
  accountId: string;
}

export async function getJiraContext(): Promise<JiraContext> {
  const ctx = await view.getContext();
  const { issue, project } = ctx.extension as {
    issue: { id: string; key: string };
    project: { id: string; key: string };
  };

  let summary = '';
  try {
    const resp = await requestJira(`/rest/api/3/issue/${issue.key}?fields=summary`);
    const data = await resp.json();
    summary = (data as { fields?: { summary?: string } }).fields?.summary ?? '';
  } catch {
    // summary is optional
  }

  return {
    cloudId: (ctx as { cloudId?: string }).cloudId ?? '',
    issueId: issue.id,
    issueKey: issue.key,
    projectId: project.id,
    projectKey: project.key,
    summary,
    accountId: (ctx as { accountId?: string }).accountId ?? '',
  };
}

export interface PublicSettings {
  connected: boolean;
  apiBaseUrl: string;
  workspaceId: string;
  workspaceName?: string;
}

export interface IssueStateResponse {
  connected: boolean;
  issueKey?: string;
  taskId?: string;
  timerActive?: boolean;
  activeTimeEntryId?: string | null;
  startedAt?: string | null;
  todayTrackedSeconds?: number;
  totalTrackedSeconds?: number;
  billableAmount?: number;
  currency?: string;
  timevicUrl?: string;
  error?: string;
  authError?: boolean;
}

export interface TimerActionResult {
  success: boolean;
  data?: {
    timerActive: boolean;
    timeEntryId?: string;
    startedAt?: string;
    stoppedTimeEntryId?: string;
    durationSeconds?: number;
    todayTrackedSeconds: number;
    totalTrackedSeconds: number;
    billableAmount?: number;
    currency?: string;
    timevicUrl?: string;
  };
  error?: string;
}

export async function getSettings(): Promise<PublicSettings> {
  return invoke<PublicSettings>('getSettings');
}

export async function getIssueState(ctx: JiraContext): Promise<IssueStateResponse> {
  return invoke<IssueStateResponse>('getIssueState', {
    cloudId: ctx.cloudId,
    issueId: ctx.issueId,
    issueKey: ctx.issueKey,
    projectId: ctx.projectId,
    projectKey: ctx.projectKey,
    summary: ctx.summary,
  });
}

export async function startTimer(ctx: JiraContext): Promise<TimerActionResult> {
  return invoke<TimerActionResult>('startTimer', {
    cloudId: ctx.cloudId,
    issueId: ctx.issueId,
    issueKey: ctx.issueKey,
    projectId: ctx.projectId,
    projectKey: ctx.projectKey,
    summary: ctx.summary,
    accountId: ctx.accountId,
  });
}

export async function stopTimer(ctx: JiraContext): Promise<TimerActionResult> {
  return invoke<TimerActionResult>('stopTimer', {
    cloudId: ctx.cloudId,
    issueId: ctx.issueId,
    issueKey: ctx.issueKey,
  });
}
