import React from 'react';
import { JiraContext, IssueStateResponse } from '../api/forgeApi';
import TimerCard from './TimerCard';

interface Props {
  ctx: JiraContext;
  state: IssueStateResponse;
  onStateChange: (state: IssueStateResponse) => void;
}

const IssuePanel: React.FC<Props> = ({ ctx, state, onStateChange }) => {
  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <span style={styles.logo}>⏱ TimeVic</span>
      </div>

      <div style={styles.issueInfo}>
        <span style={styles.issueKey}>{ctx.issueKey}</span>
        {ctx.summary && <p style={styles.summary}>{ctx.summary}</p>}
        <span style={styles.project}>Jira project: {ctx.projectKey}</span>
      </div>

      <TimerCard ctx={ctx} state={state} onStateChange={onStateChange} />
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  container: { padding: '16px', display: 'flex', flexDirection: 'column', gap: '12px' },
  header: { display: 'flex', alignItems: 'center', gap: '8px' },
  logo: { fontSize: '15px', fontWeight: 700, color: '#172b4d' },
  issueInfo: { display: 'flex', flexDirection: 'column', gap: '2px' },
  issueKey: { fontSize: '12px', fontWeight: 700, color: '#0052cc' },
  summary: { fontSize: '14px', color: '#172b4d', lineHeight: 1.4, margin: '2px 0' },
  project: { fontSize: '12px', color: '#5e6c84' },
};

export default IssuePanel;
