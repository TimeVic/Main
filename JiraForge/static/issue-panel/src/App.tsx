import React, { useEffect, useState } from 'react';
import { getJiraContext, getIssueState, JiraContext, IssueStateResponse } from './api/forgeApi';
import IssuePanel from './components/IssuePanel';
import NotConnectedState from './components/NotConnectedState';

const App: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ctx, setCtx] = useState<JiraContext | null>(null);
  const [state, setState] = useState<IssueStateResponse | null>(null);

  useEffect(() => {
    async function init() {
      try {
        const jiraCtx = await getJiraContext();
        setCtx(jiraCtx);
        const issueState = await getIssueState(jiraCtx);
        setState(issueState);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load');
      } finally {
        setLoading(false);
      }
    }
    init();
  }, []);

  const handleStateChange = (newState: IssueStateResponse) => {
    setState(newState);
  };

  if (loading) {
    return <div style={styles.loading}>Loading…</div>;
  }

  if (error) {
    return <div style={styles.error}>{error}</div>;
  }

  if (!state?.connected) {
    return (
      <NotConnectedState
        errorMessage={state?.authError ? state.error : undefined}
      />
    );
  }

  return (
    <IssuePanel
      ctx={ctx!}
      state={state}
      onStateChange={handleStateChange}
    />
  );
};

const styles: Record<string, React.CSSProperties> = {
  loading: { padding: '16px', color: '#5e6c84' },
  error: { padding: '16px', color: '#de350b' },
};

export default App;
