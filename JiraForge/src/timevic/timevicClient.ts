import { fetch } from '@forge/api';
import {
  TestConnectionResponse,
  IssueStateRequest,
  IssueStateResponse,
  StartTimerRequest,
  StartTimerResponse,
  StopTimerRequest,
  StopTimerResponse,
} from './types';
import { TimevicApiError } from '../utils/errors';

export class TimevicClient {
  private readonly baseUrl: string;
  private readonly token: string;

  constructor(baseUrl: string, token: string) {
    this.baseUrl = baseUrl.replace(/\/$/, '');
    this.token = token;
  }

  private getHeaders(): Record<string, string> {
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${this.token}`,
    };
  }

  private async post<T>(path: string, body: unknown): Promise<T> {
    const url = `${this.baseUrl}${path}`;
    const response = await fetch(url, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(body),
    });

    if (!response.ok) {
      const errorText = await response.text().catch(() => 'Unknown error');
      throw new TimevicApiError(
        `TimeVic API error: ${response.status} ${response.statusText}`,
        response.status,
        errorText,
      );
    }

    return response.json() as Promise<T>;
  }

  async testConnection(workspaceId: string): Promise<TestConnectionResponse> {
    return this.post<TestConnectionResponse>('/api/integrations/jira/test-connection', {
      workspaceId,
      token: this.token,
    });
  }

  async getIssueState(request: IssueStateRequest): Promise<IssueStateResponse> {
    return this.post<IssueStateResponse>('/api/integrations/jira/issue/state', request);
  }

  async startTimer(request: StartTimerRequest): Promise<StartTimerResponse> {
    return this.post<StartTimerResponse>('/api/integrations/jira/timer/start', request);
  }

  async stopTimer(request: StopTimerRequest): Promise<StopTimerResponse> {
    return this.post<StopTimerResponse>('/api/integrations/jira/timer/stop', request);
  }
}

export function createTimevicClient(baseUrl: string, token: string): TimevicClient {
  return new TimevicClient(baseUrl, token);
}
