import { invoke } from '@forge/bridge';

export interface PublicSettings {
  connected: boolean;
  apiBaseUrl: string;
  workspaceId: string;
  workspaceName?: string;
}

export interface TestConnectionResult {
  success: boolean;
  workspaceName?: string;
  error?: string;
}

export interface SaveSettingsResult {
  success: boolean;
  settings?: PublicSettings;
  error?: string;
}

export interface DisconnectResult {
  success: boolean;
}

export async function getSettings(): Promise<PublicSettings> {
  return invoke<PublicSettings>('getSettings');
}

export async function testConnection(
  apiBaseUrl: string,
  integrationToken: string,
  workspaceId: string,
): Promise<TestConnectionResult> {
  return invoke<TestConnectionResult>('testConnection', {
    apiBaseUrl,
    integrationToken,
    workspaceId,
  });
}

export async function saveSettings(
  apiBaseUrl: string,
  integrationToken: string,
  workspaceId: string,
): Promise<SaveSettingsResult> {
  return invoke<SaveSettingsResult>('saveSettings', {
    apiBaseUrl,
    integrationToken,
    workspaceId,
  });
}

export async function disconnect(): Promise<DisconnectResult> {
  return invoke<DisconnectResult>('disconnect');
}
