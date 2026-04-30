import React, { useEffect, useState } from 'react';
import { getSettings, PublicSettings } from './api/forgeApi';
import ConnectionSettings from './components/ConnectionSettings';

const App: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [settings, setSettings] = useState<PublicSettings | null>(null);

  useEffect(() => {
    getSettings()
      .then(setSettings)
      .catch(() => {
        setSettings({ connected: false, apiBaseUrl: 'https://api.timevic.com', workspaceId: '' });
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return <div style={{ padding: '24px', color: '#5e6c84' }}>Loading…</div>;
  }

  return (
    <ConnectionSettings
      initialSettings={settings!}
      onSettingsChange={setSettings}
    />
  );
};

export default App;
