'use client';

import { useEffect, useState } from 'react';
import { API_URL, PublicOffer, readError } from '@/lib/api';

// The candidate opens the secret link from the email and accepts or declines the offer.
export default function OfferPage({ params }: { params: { token: string } }) {
  const [offer, setOffer] = useState<PublicOffer | null>(null);
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const [done, setDone] = useState('');

  useEffect(() => {
    fetch(`${API_URL}/api/public/offers/${params.token}`)
      .then(async response => (response.ok ? setOffer(await response.json()) : setError(await readError(response))))
      .catch(() => setError('Cannot reach the server. Please try again in a minute.'));
  }, [params.token]);

  async function respond(accept: boolean) {
    setBusy(true);
    setError('');
    try {
      const response = await fetch(`${API_URL}/api/public/offers/${params.token}/respond`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ accept })
      });
      if (response.ok) setDone(accept ? 'Welcome aboard! We will contact you about onboarding.' : 'You declined the offer. Thank you for your time.');
      else setError(await readError(response));
    } catch {
      setError('Cannot reach the server. Please try again in a minute.');
    }
    setBusy(false);
  }

  return (
    <div style={{ maxWidth: 520, margin: '0 auto' }}>
      <h1>Your job offer</h1>
      {error && <div className="error" role="alert">{error}</div>}
      {done && <div className="success" role="status">{done}</div>}
      {offer && !done && (
        <div className="card">
          <p>Hi <strong>{offer.candidateName}</strong>, we are happy to offer you the position of <strong>{offer.jobTitle}</strong>.</p>
          <p>Monthly salary: <strong>{offer.salary.toLocaleString()}</strong></p>
          {offer.status === 'Approved' ? (
            <div style={{ display: 'flex', gap: 10 }}>
              <button className="btn" disabled={busy} onClick={() => respond(true)}>Accept offer</button>
              <button className="btn ghost" disabled={busy} onClick={() => respond(false)}>Decline</button>
            </div>
          ) : (
            <p className="muted">This offer cannot be answered (status: {offer.status}).</p>
          )}
        </div>
      )}
    </div>
  );
}
