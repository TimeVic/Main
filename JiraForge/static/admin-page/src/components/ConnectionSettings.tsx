import React, { useState } from 'react';
import {
  PublicSettings,
  testConnection,
  saveSettings,
  disconnect,
} from '../api/forgeApi';

interface Props {
  initialSettings: PublicSettings;
  onSettingsChange: (settings: PublicSettings) => void;
}

type Status = { type: 'success' | 'error'; message: string } | null;

const ConnectionSettings: React.FC<Props> = ({ initialSettings, onSettingsChange }) => {
  const [apiBaseUrl, setApiBaseUrl] = useState(
    initialSettings.apiBaseUrl || 'https://api.timevic.com',
  );
  const [token, setToken] = useState('');
  const [workspaceId, setWorkspaceId] = useState(initialSettings.workspaceId || '');
  const [status, setStatus] = useState<Status>(null);
  const [testing, setTesting] = useState(false);
  const [saving, setSaving] = useState(false);
  const [disconnecting, setDisconnecting] = useState(false);

  const isConnected = initialSettings.connected;

  const handleTest = async () => {
    if (!token) {
      setStatus({ type: 'error', message: 'Integration Token is required to test the connection.' });
      return;
    }
    setTesting(true);
    setStatus(null);
    try {
      const result = await testConnection(apiBaseUrl, token, workspaceId);
      if (result.success) {
        setStatus({ type: 'success', message: `Connection successful! Workspace: ${result.workspaceName ?? workspaceId}` });
      } else {
        setStatus({ type: 'error', message: result.error ?? 'Connection failed' });
      }
    } catch (err) {
      setStatus({ type: 'error', message: err instanceof Error ? err.message : 'Unexpected error' });
    } finally {
      setTesting(false);
    }
  };

  const handleSave = async () => {
    if (!token) {
      setStatus({ type: 'error', message: 'Integration Token is required.' });
      return;
    }
    setSaving(true);
    setStatus(null);
    try {
      const result = await saveSettings(apiBaseUrl, token, workspaceId);
      if (result.success && result.settings) {
        setStatus({ type: 'success', message: 'Settings saved successfully.' });
        setToken('');
        onSettingsChange(result.settings);
      } else {
        setStatus({ type: 'error', message: result.error ?? 'Failed to save settings' });
      }
    } catch (err) {
      setStatus({ type: 'error', message: err instanceof Error ? err.message : 'Unexpected error' });
    } finally {
      setSaving(false);
    }
  };

  const handleDisconnect = async () => {
    if (!window.confirm('Disconnect TimeVic? This will remove all saved settings.')) return;
    setDisconnecting(true);
    setStatus(null);
    try {
      await disconnect();
      setToken('');
      setWorkspaceId('');
      setApiBaseUrl('https://api.timevic.com');
      setStatus({ type: 'success', message: 'Disconnected from TimeVic.' });
      onSettingsChange({ connected: false, apiBaseUrl: 'https://api.timevic.com', workspaceId: '' });
    } catch (err) {
      setStatus({ type: 'error', message: err instanceof Error ? err.message : 'Unexpected error' });
    } finally {
      setDisconnecting(false);
    }
  };

  const busy = testing || saving || disconnecting;

  return (
    <div style={styles.container}>
      <h2 style={styles.heading}>TimeVic — Jira Integration</h2>

      {isConnected && (
        <div style={styles.connectedBadge}>
          ✓ Connected{initialSettings.workspaceName ? ` · ${initialSettings.workspaceName}` : ''}
        </div>
      )}

      {status && (
        <div style={{ ...styles.statusBox, ...(status.type === 'success' ? styles.statusSuccess : styles.statusError) }}>
          {status.message}
        </div>
      )}

      <div style={styles.formGroup}>
        <label style={styles.label}>API Base URL</label>
        <input
          style={styles.input}
          type="url"
          value={apiBaseUrl}
          onChange={(e) => setApiBaseUrl(e.target.value)}
          placeholder="https://api.timevic.com"
          disabled={busy}
        />
      </div>

      <div style={styles.formGroup}>
        <label style={styles.label}>
          Integration Token{isConnected && <span style={styles.savedNote}> (leave blank to keep saved token)</span>}
        </label>
        <input
          style={styles.input}
          type="password"
          value={token}
          onChange={(e) => setToken(e.target.value)}
          placeholder={isConnected ? '••••••••' : 'Enter integration token'}
          disabled={busy}
          autoComplete="off"
        />
      </div>

      <div style={styles.formGroup}>
        <label style={styles.label}>Workspace ID</label>
        <input
          style={styles.input}
          type="text"
          value={workspaceId}
          onChange={(e) => setWorkspaceId(e.target.value)}
          placeholder="Your TimeVic Workspace ID"
          disabled={busy}
        />
      </div>

      <div style={styles.buttonRow}>
        <button style={{ ...styles.button, ...styles.secondaryButton }} onClick={handleTest} disabled={busy}>
          {testing ? 'Testing…' : 'Test connection'}
        </button>
        <button style={{ ...styles.button, ...styles.primaryButton }} onClick={handleSave} disabled={busy}>
          {saving ? 'Saving…' : 'Save connection'}
        </button>
      </div>

      {isConnected && (
        <button
          style={{ ...styles.button, ...styles.dangerButton, marginTop: '8px', width: '100%' }}
          onClick={handleDisconnect}
          disabled={busy}
        >
          {disconnecting ? 'Disconnecting…' : 'Disconnect'}
        </button>
      )}
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  container: { padding: '24px', maxWidth: '480px', display: 'flex', flexDirection: 'column', gap: '16px' },
  heading: { fontSize: '18px', fontWeight: 700, color: '#172b4d' },
  connectedBadge: {
    padding: '8px 12px',
    background: '#e3fcef',
    border: '1px solid #57d9a3',
    borderRadius: '4px',
    color: '#006644',
    fontSize: '13px',
    fontWeight: 500,
  },
  statusBox: { padding: '10px 12px', borderRadius: '4px', fontSize: '13px' },
  statusSuccess: { background: '#e3fcef', border: '1px solid #57d9a3', color: '#006644' },
  statusError: { background: '#ffebe6', border: '1px solid #ff8f73', color: '#bf2600' },
  formGroup: { display: 'flex', flexDirection: 'column', gap: '4px' },
  label: { fontSize: '12px', fontWeight: 600, color: '#5e6c84' },
  savedNote: { fontWeight: 400, fontStyle: 'italic' },
  input: {
    padding: '8px 10px',
    border: '2px solid #dfe1e6',
    borderRadius: '4px',
    fontSize: '14px',
    outline: 'none',
    width: '100%',
  },
  buttonRow: { display: 'flex', gap: '8px' },
  button: {
    flex: 1,
    padding: '8px 16px',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
    fontSize: '14px',
    fontWeight: 500,
  },
  primaryButton: { background: '#0052cc', color: '#fff' },
  secondaryButton: { background: '#f4f5f7', color: '#172b4d', border: '1px solid #dfe1e6' },
  dangerButton: { background: '#fff', color: '#bf2600', border: '1px solid #ff8f73' },
};

export default ConnectionSettings;
