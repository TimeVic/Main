import React, { useEffect, useRef, useState } from 'react';
import { JiraContext, IssueStateResponse, TimerActionResult, startTimer, stopTimer } from '../api/forgeApi';

function formatSeconds(totalSeconds: number): string {
  if (totalSeconds <= 0) return '0m';
  const h = Math.floor(totalSeconds / 3600);
  const m = Math.floor((totalSeconds % 3600) / 60);
  if (h > 0 && m > 0) return `${h}h ${m}m`;
  if (h > 0) return `${h}h`;
  return `${m}m`;
}

function formatElapsed(startedAt: string): string {
  const diffSeconds = Math.max(
    0,
    Math.floor((Date.now() - new Date(startedAt).getTime()) / 1000),
  );
  const h = Math.floor(diffSeconds / 3600);
  const m = Math.floor((diffSeconds % 3600) / 60);
  const s = diffSeconds % 60;
  return [h, m, s].map((n) => String(n).padStart(2, '0')).join(':');
}

interface Props {
  ctx: JiraContext;
  state: IssueStateResponse;
  onStateChange: (state: IssueStateResponse) => void;
}

const TimerCard: React.FC<Props> = ({ ctx, state, onStateChange }) => {
  const [busy, setBusy] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [elapsed, setElapsed] = useState<string>('00:00:00');
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    if (state.timerActive && state.startedAt) {
      setElapsed(formatElapsed(state.startedAt));
      intervalRef.current = setInterval(() => {
        setElapsed(formatElapsed(state.startedAt!));
      }, 1000);
    } else {
      if (intervalRef.current) clearInterval(intervalRef.current);
    }
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, [state.timerActive, state.startedAt]);

  const handleStart = async () => {
    setActionError(null);
    setBusy(true);
    try {
      const result: TimerActionResult = await startTimer(ctx);
      if (result.success && result.data) {
        onStateChange({
          ...state,
          timerActive: result.data.timerActive,
          startedAt: result.data.startedAt ?? null,
          todayTrackedSeconds: result.data.todayTrackedSeconds,
          totalTrackedSeconds: result.data.totalTrackedSeconds,
          billableAmount: result.data.billableAmount,
          currency: result.data.currency,
          timevicUrl: result.data.timevicUrl ?? state.timevicUrl,
        });
      } else {
        setActionError(result.error ?? 'Failed to start timer');
      }
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Unexpected error');
    } finally {
      setBusy(false);
    }
  };

  const handleStop = async () => {
    setActionError(null);
    setBusy(true);
    try {
      const result: TimerActionResult = await stopTimer(ctx);
      if (result.success && result.data) {
        onStateChange({
          ...state,
          timerActive: result.data.timerActive,
          startedAt: null,
          todayTrackedSeconds: result.data.todayTrackedSeconds,
          totalTrackedSeconds: result.data.totalTrackedSeconds,
          billableAmount: result.data.billableAmount,
          currency: result.data.currency,
          timevicUrl: result.data.timevicUrl ?? state.timevicUrl,
        });
      } else {
        setActionError(result.error ?? 'Failed to stop timer');
      }
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Unexpected error');
    } finally {
      setBusy(false);
    }
  };

  const hasSummary =
    state.todayTrackedSeconds !== undefined ||
    state.totalTrackedSeconds !== undefined ||
    state.billableAmount !== undefined;

  return (
    <div style={styles.card}>
      {hasSummary && (
        <div style={styles.stats}>
          {state.todayTrackedSeconds !== undefined && (
            <span style={styles.stat}>
              Today: <strong>{formatSeconds(state.todayTrackedSeconds)}</strong>
            </span>
          )}
          {state.totalTrackedSeconds !== undefined && (
            <span style={styles.stat}>
              Total: <strong>{formatSeconds(state.totalTrackedSeconds)}</strong>
            </span>
          )}
          {state.billableAmount !== undefined && (
            <span style={styles.stat}>
              Billable:{' '}
              <strong>
                {state.currency ?? '$'}{state.billableAmount}
              </strong>
            </span>
          )}
        </div>
      )}

      {state.timerActive ? (
        <div style={styles.activeTimer}>
          <span style={styles.trackingLabel}>Tracking now</span>
          <span style={styles.elapsed}>{elapsed}</span>
        </div>
      ) : null}

      {actionError && <p style={styles.error}>{actionError}</p>}

      <div style={styles.actions}>
        {state.timerActive ? (
          <button
            style={{ ...styles.button, ...styles.stopButton }}
            onClick={handleStop}
            disabled={busy}
          >
            {busy ? 'Stopping…' : 'Stop timer'}
          </button>
        ) : (
          <button
            style={{ ...styles.button, ...styles.startButton }}
            onClick={handleStart}
            disabled={busy}
          >
            {busy ? 'Starting…' : 'Start timer'}
          </button>
        )}

        {state.timevicUrl && (
          <a
            href={state.timevicUrl}
            target="_blank"
            rel="noopener noreferrer"
            style={styles.link}
          >
            Open in TimeVic ↗
          </a>
        )}
      </div>
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  card: { display: 'flex', flexDirection: 'column', gap: '12px' },
  stats: { display: 'flex', flexDirection: 'column', gap: '4px' },
  stat: { fontSize: '13px', color: '#5e6c84' },
  activeTimer: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'flex-start',
    gap: '2px',
  },
  trackingLabel: {
    fontSize: '12px',
    color: '#00875a',
    fontWeight: 600,
    textTransform: 'uppercase',
    letterSpacing: '0.5px',
  },
  elapsed: { fontSize: '24px', fontWeight: 700, color: '#172b4d', letterSpacing: '1px' },
  actions: { display: 'flex', flexDirection: 'column', gap: '8px' },
  button: {
    padding: '8px 16px',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
    fontSize: '14px',
    fontWeight: 500,
    width: '100%',
  },
  startButton: { background: '#0052cc', color: '#fff' },
  stopButton: { background: '#de350b', color: '#fff' },
  link: {
    fontSize: '13px',
    color: '#0052cc',
    textDecoration: 'none',
    textAlign: 'center' as const,
  },
  error: { fontSize: '12px', color: '#de350b' },
};

export default TimerCard;
