import { storage } from '@forge/api';
import { StoredSettings, PublicSettings } from '../timevic/types';

const SETTINGS_KEY = 'timevic_settings';

export async function saveSettings(settings: StoredSettings): Promise<void> {
  await storage.set(SETTINGS_KEY, settings);
}

export async function getStoredSettings(): Promise<StoredSettings | null> {
  const value = await storage.get(SETTINGS_KEY);
  return (value as StoredSettings) ?? null;
}

export async function getPublicSettings(): Promise<PublicSettings> {
  const settings = await getStoredSettings();
  if (!settings) {
    return {
      connected: false,
      apiBaseUrl: 'https://api.timevic.com',
      workspaceId: '',
    };
  }
  return {
    connected: true,
    apiBaseUrl: settings.apiBaseUrl,
    workspaceId: settings.workspaceId,
    workspaceName: settings.workspaceName,
  };
}

export async function deleteSettings(): Promise<void> {
  await storage.delete(SETTINGS_KEY);
}
