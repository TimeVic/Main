import React from 'react';

interface Props {
  errorMessage?: string;
}

const NotConnectedState: React.FC<Props> = ({ errorMessage }) => {
  const openSettings = () => {
    // Forge does not expose a programmatic way to navigate to admin page from an issue panel.
    // Users must navigate to the admin page manually via the app's admin settings.
    window.open('https://admin.atlassian.com', '_blank', 'noopener,noreferrer');
  };

  return (
    <div style={styles.container}>
      <div style={styles.icon}>⏱</div>
      {errorMessage ? (
        <>
          <p style={styles.title}>TimeVic connection error</p>
          <p style={styles.message}>{errorMessage}</p>
        </>
      ) : (
        <>
          <p style={styles.title}>TimeVic is not connected</p>
          <p style={styles.message}>
            Connect your TimeVic workspace to track time from Jira.
          </p>
        </>
      )}
      <button style={styles.button} onClick={openSettings}>
        Open settings
      </button>
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  container: {
    padding: '20px 16px',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    textAlign: 'center',
    gap: '10px',
  },
  icon: { fontSize: '32px' },
  title: { fontWeight: 600, fontSize: '14px', color: '#172b4d' },
  message: { fontSize: '13px', color: '#5e6c84', lineHeight: 1.5 },
  button: {
    marginTop: '8px',
    padding: '6px 16px',
    background: '#0052cc',
    color: '#fff',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
    fontSize: '14px',
    fontWeight: 500,
  },
};

export default NotConnectedState;
