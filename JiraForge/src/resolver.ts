import Resolver from '@forge/resolver';
import {
  deleteSettings,
  getPublicSettings,
  getStoredSettings,
  saveSettings,
} from './storage/settingsStorage';
import { createTimevicClient } from './timevic/timevicClient';
import { StoredSettings } from './timevic/types';
import { formatError, isAuthError } from './utils/errors';

const resolver = new Resolver();

resolver.define('getSettings', async () => {
  return getPublicSettings();
});

resolver.define('testConnection', async ({ payload }) => {
  const { apiBaseUrl, integrationToken, workspaceId } = payload as {
    apiBaseUrl: string;
    integrationToken: string;
    workspaceId: string;
  };

  try {
    const client = createTimevicClient(apiBaseUrl, integrationToken);
    const result = await client.testConnection(workspaceId);
    return { success: true, workspaceName: result.workspaceName };
  } catch (error) {
    return { success: false, error: formatError(error) };
  }
});

resolver.define('saveSettings', async ({ payload }) => {
  const { apiBaseUrl, integrationToken, workspaceId } = payload as {
    apiBaseUrl: string;
    integrationToken: string;
    workspaceId: string;
  };

  try {
    const client = createTimevicClient(apiBaseUrl, integrationToken);
    const testResult = await client.testConnection(workspaceId);

    const settings: StoredSettings = {
      apiBaseUrl,
      integrationToken,
      workspaceId,
      workspaceName: testResult.workspaceName,
    };

    await saveSettings(settings);

    return {
      success: true,
      settings: {
        connected: true,
        apiBaseUrl,
        workspaceId,
        workspaceName: testResult.workspaceName,
      },
    };
  } catch (error) {
    return { success: false, error: formatError(error) };
  }
});

resolver.define('disconnect', async () => {
  await deleteSettings();
  return { success: true };
});

resolver.define('getIssueState', async ({ payload }) => {
  const { cloudId, issueId, issueKey, projectId, projectKey, summary } = payload as {
    cloudId: string;
    issueId: string;
    issueKey: string;
    projectId: string;
    projectKey: string;
    summary: string;
  };

  const stored = await getStoredSettings();
  if (!stored) {
    return { connected: false };
  }

  try {
    const client = createTimevicClient(stored.apiBaseUrl, stored.integrationToken);
    const state = await client.getIssueState({
      workspaceId: stored.workspaceId,
      cloudId,
      issueId,
      issueKey,
      projectId,
      projectKey,
      summary,
    });
    return state;
  } catch (error) {
    if (isAuthError(error)) {
      return {
        connected: false,
        error: 'TimeVic connection is invalid. Please reconnect in settings.',
        authError: true,
      };
    }
    return { connected: false, error: formatError(error) };
  }
});

resolver.define('startTimer', async ({ payload }) => {
  const { cloudId, issueId, issueKey, projectId, projectKey, summary, accountId } =
    payload as {
      cloudId: string;
      issueId: string;
      issueKey: string;
      projectId: string;
      projectKey: string;
      summary: string;
      accountId: string;
    };

  const stored = await getStoredSettings();
  if (!stored) {
    return { success: false, error: 'TimeVic is not connected' };
  }

  try {
    const client = createTimevicClient(stored.apiBaseUrl, stored.integrationToken);
    const result = await client.startTimer({
      workspaceId: stored.workspaceId,
      cloudId,
      issueId,
      issueKey,
      projectId,
      projectKey,
      summary,
      accountId,
    });
    return { success: true, data: result };
  } catch (error) {
    return { success: false, error: formatError(error) };
  }
});

resolver.define('stopTimer', async ({ payload }) => {
  const { cloudId, issueId, issueKey } = payload as {
    cloudId: string;
    issueId: string;
    issueKey: string;
  };

  const stored = await getStoredSettings();
  if (!stored) {
    return { success: false, error: 'TimeVic is not connected' };
  }

  try {
    const client = createTimevicClient(stored.apiBaseUrl, stored.integrationToken);
    const result = await client.stopTimer({
      workspaceId: stored.workspaceId,
      cloudId,
      issueId,
      issueKey,
    });
    return { success: true, data: result };
  } catch (error) {
    return { success: false, error: formatError(error) };
  }
});

export const handler = resolver.getDefinitions();
