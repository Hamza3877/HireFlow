'use client';

// Shown when the page cannot load (for example the API is asleep or offline).
export default function ErrorPage({ reset }: { error: Error; reset: () => void }) {
  return (
    <div className="card">
      <h2>We could not load this page</h2>
      <p className="muted">The server may be waking up. Please try again in a moment.</p>
      <button className="btn" onClick={reset}>Try again</button>
    </div>
  );
}
