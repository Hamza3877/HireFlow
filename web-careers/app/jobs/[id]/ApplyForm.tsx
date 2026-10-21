'use client';

import { FormEvent, useState } from 'react';
import { API_URL } from '@/lib/api';

const MAX_BYTES = 5 * 1024 * 1024;

// Apply form with CV upload, progress bar and clear error messages.
export default function ApplyForm({ jobId }: { jobId: number }) {
  const [file, setFile] = useState<File | null>(null);
  const [progress, setProgress] = useState(0);
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const [result, setResult] = useState<{ applicationCode: string; jobTitle: string } | null>(null);

  function checkFile(selected: File | null) {
    setError('');
    setFile(null);
    if (!selected) return;
    const name = selected.name.toLowerCase();
    if (!name.endsWith('.pdf') && !name.endsWith('.docx')) {
      setError('Please choose a PDF or DOCX file.');
      return;
    }
    if (selected.size > MAX_BYTES) {
      setError('The file is larger than 5 MB.');
      return;
    }
    setFile(selected);
  }

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!file) {
      setError('Please attach your CV.');
      return;
    }
    const form = new FormData(event.currentTarget);
    form.set('Cv', file);

    // XMLHttpRequest is used (not fetch) because it can report upload progress.
    const request = new XMLHttpRequest();
    request.open('POST', `${API_URL}/api/public/jobs/${jobId}/apply`);
    request.upload.onprogress = e => e.lengthComputable && setProgress(Math.round((e.loaded / e.total) * 100));
    request.onload = () => {
      setBusy(false);
      const body = safeJson(request.responseText);
      if (request.status === 200) {
        setResult(body);
      } else {
        setError(body?.detail ?? firstValidationError(body) ?? 'Could not send your application.');
      }
    };
    request.onerror = () => {
      setBusy(false);
      setError('Cannot reach the server. It may be waking up, please try again in a minute.');
    };
    setBusy(true);
    setError('');
    setProgress(0);
    request.send(form);
  }

  if (result) {
    return (
      <div className="card success" role="status">
        <h2 style={{ marginTop: 0 }}>Application sent</h2>
        <p>Thank you! Keep this code to check your status:</p>
        <div className="code">{result.applicationCode}</div>
        <p><a href="/status">Check my application status</a></p>
      </div>
    );
  }

  return (
    <form className="card" onSubmit={submit}>
      <h2 style={{ marginTop: 0 }}>Apply now</h2>
      {error && <div className="error" role="alert">{error}</div>}
      <div className="field"><label htmlFor="FullName">Full name</label><input id="FullName" name="FullName" required maxLength={100} /></div>
      <div className="field"><label htmlFor="Email">Email</label><input id="Email" name="Email" type="email" required maxLength={150} /></div>
      <div className="field"><label htmlFor="Phone">Phone</label><input id="Phone" name="Phone" required maxLength={30} /></div>
      <div className="field">
        <label htmlFor="cv">CV (PDF or DOCX, max 5 MB)</label>
        <input id="cv" type="file" accept=".pdf,.docx" onChange={e => checkFile(e.target.files?.[0] ?? null)} />
      </div>
      {busy && <div className="progress" aria-label="Upload progress"><div style={{ width: `${progress}%` }} /></div>}
      <button className="btn" type="submit" disabled={busy} style={{ width: '100%' }}>{busy ? 'Sending…' : 'Submit application'}</button>
    </form>
  );
}

function safeJson(text: string) {
  try { return JSON.parse(text); } catch { return null; }
}

function firstValidationError(body: { errors?: Record<string, string[]> } | null): string | undefined {
  const first = body?.errors ? Object.values(body.errors)[0] : undefined;
  return first?.[0];
}
