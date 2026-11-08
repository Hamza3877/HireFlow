'use client';

import { FormEvent, useState } from 'react';
import { API_URL, StatusResult, readError } from '@/lib/api';

const STAGES = ['Applied', 'Screening', 'Interview', 'Offer', 'Hired'];

export default function StatusPage() {
  const [result, setResult] = useState<StatusResult | null>(null);
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const query = new URLSearchParams({ email: String(data.get('email')), code: String(data.get('code')) });
    setBusy(true);
    setError('');
    setResult(null);
    try {
      const response = await fetch(`${API_URL}/api/public/applications/status?${query}`);
      if (response.ok) setResult(await response.json());
      else setError(await readError(response));
    } catch {
      setError('Cannot reach the server. Please try again in a minute.');
    }
    setBusy(false);
  }

  const index = result ? STAGES.indexOf(result.stage) : -1;

  return (
    <div style={{ maxWidth: 480, margin: '0 auto' }}>
      <h1>Check my application</h1>
      <form className="card" onSubmit={submit}>
        {error && <div className="error" role="alert">{error}</div>}
        <div className="field"><label htmlFor="email">Email you applied with</label><input id="email" name="email" type="email" required /></div>
        <div className="field"><label htmlFor="code">Application code</label><input id="code" name="code" required /></div>
        <button className="btn" type="submit" disabled={busy} style={{ width: '100%' }}>{busy ? 'Checking…' : 'Check status'}</button>
      </form>

      {result && (
        <div className="card" style={{ marginTop: 16 }} role="status">
          <h2 style={{ marginTop: 0 }}>{result.jobTitle}</h2>
          <p>Current status: <strong>{result.stage}</strong></p>
          {result.stage !== 'Rejected' && index >= 0 && (
            <div className="progress"><div style={{ width: `${((index + 1) / STAGES.length) * 100}%` }} /></div>
          )}
          {result.stage === 'Rejected' && <p className="muted">Thank you for applying. We will not move forward this time.</p>}
          <p className="muted">Applied on {new Date(result.appliedAt).toLocaleDateString()}</p>
        </div>
      )}
    </div>
  );
}
