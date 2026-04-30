export interface TestConnectionRequest {
  workspaceId: string;
  token: string;
}

export interface TestConnectionResponse {
  success: boolean;
  workspaceId: string;
  workspaceName: string;
}

export interface IssueStateRequest {
  workspaceId: string;
  cloudId: string;
  issueId: string;
  issueKey: string;
  projectId: string;
  projectKey: string;
  summary: string;
}

export interface IssueStateResponse {
  connected: boolean;
  issueKey: string;
  taskId?: string;
  timerActive: boolean;
  activeTimeEntryId?: string | null;
  startedAt?: string | null;
  todayTrackedSeconds: number;
  totalTrackedSeconds: number;
  billableAmount?: number;
  currency?: string;
  timevicUrl?: string;
}

export interface StartTimerRequest {
  workspaceId: string;
  cloudId: string;
  issueId: string;
  issueKey: string;
  projectId: string;
  projectKey: string;
  summary: string;
  accountId: string;
}

export interface StartTimerResponse {
  timerActive: boolean;
  timeEntryId: string;
  startedAt: string;
  todayTrackedSeconds: number;
  totalTrackedSeconds: number;
  billableAmount?: number;
  currency?: string;
  timevicUrl?: string;
}

export interface StopTimerRequest {
  workspaceId: string;
  cloudId: string;
  issueId: string;
  issueKey: string;
}

export interface StopTimerResponse {
  timerActive: boolean;
  stoppedTimeEntryId: string;
  durationSeconds: number;
  todayTrackedSeconds: number;
  totalTrackedSeconds: number;
  billableAmount?: number;
  currency?: string;
  timevicUrl?: string;
}

export interface StoredSettings {
  apiBaseUrl: string;
  integrationToken: string;
  workspaceId: string;
  workspaceName?: string;
}

export interface PublicSettings {
  connected: boolean;
  apiBaseUrl: string;
  workspaceId: string;
  workspaceName?: string;
}
