import Link from 'next/link';
import { getOpenJobs } from '@/lib/api';



const paths = {
  bolt: <path d="M13 2 4 14h7l-1 8 9-12h-7z" />,
  bell: <path d="M18 8a6 6 0 0 0-12 0c0 7-3 9-3 9h18s-3-2-3-9M13.7 21a2 2 0 0 1-3.4 0" />,
  file: <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8zM14 2v6h6M8 13h8M8 17h5" />,
  pin: (<><path d="M20 10c0 6-8 12-8 12S4 16 4 10a8 8 0 0 1 16 0" /><circle cx="12" cy="10" r="3" /></>),
  brief: (<><rect x="2" y="7" width="20" height="14" rx="2" /><path d="M16 7V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v2" /></>),
  clock: (<><circle cx="12" cy="12" r="10" /><path d="M12 6v6l4 2" /></>),
  arrow: <path d="M5 12h14M13 6l6 6-6 6" />,
  check: <path d="M20 6 9 17l-5-5" />
};
type IconName = keyof typeof paths;

function Icon({ n }: { n: IconName }) {
  return <svg className="hf-i" viewBox="0 0 24 24" aria-hidden="true">{paths[n]}</svg>;
}

const STEPS = [
  { t: 'Pick a role', d: 'Browse open positions and search by title, department or city.' },
  { t: 'Apply in minutes', d: 'Fill a short form and upload your CV as a PDF.' },
  { t: 'See your match', d: 'Get an instant view of how your skills fit the role.' },
  { t: 'Track to offer', d: 'Follow every stage, then accept your offer online.' }
];

const PERKS: { icon: IconName; t: string; d: string }[] = [
  { icon: 'bolt', t: 'Instant skills match', d: 'See right away how your skills compare with what the role needs.' },
  { icon: 'bell', t: 'Live status tracking', d: 'Use your application code to check where you are at any time.' },
  { icon: 'check', t: 'Online offer and onboarding', d: 'Accept your offer with a secure link and follow your onboarding checklist.' }
];

const STAGES = ['Applied', 'Screening', 'Interview', 'Offer', 'Hired'];

// Careers home: hero, search box and the list of open jobs (loaded on the server).
export default async function Home({ searchParams }: { searchParams: { q?: string } }) {
  const search = searchParams.q?.trim() ?? '';
    const [jobs, all] = await Promise.all([getOpenJobs(search), search ? getOpenJobs('') : null]);
  const base = all ?? jobs;
  const departments = Array.from(new Set(base.items.map(j => j.department).filter(Boolean)));
  const locations = new Set(base.items.map(j => j.location).filter(Boolean));
  const openings = base.items.reduce((sum, j) => sum + j.positions, 0);
  const countLabel = `${jobs.totalCount} ${search ? 'matching' : 'open'} ${jobs.totalCount === 1 ? 'job' : 'jobs'}`;

  return (
    <>
      <section className="hf-hero">
        <i className="hf-blob b1" />
        <i className="hf-blob b2" />
        <div className="hf-hin">
          <span className="hf-eyebrow">We are hiring</span>
          <h1>Find your <span className="hf-hl">next role</span></h1>
          <p className="hf-lead">Apply in two minutes. We tell you your skills match right away, and you can follow every step of your application.</p>
          <form className="hf-search" action="/" method="get">
            <input name="q" defaultValue={search} placeholder="Search by title, department or city" aria-label="Search jobs" />
            <button type="submit">Search</button>
          </form>
                    {departments.length > 0 && (
            <div className="hf-pop">
              <em>Departments:</em>
              {departments.slice(0, 4).map(d => <Link key={d} href={`/?q=${encodeURIComponent(d)}`}>{d}</Link>)}
            </div>
          )}
                </div>
      </section>

           <div className="hf-bar">
        <div className="hf-bi"><span className="hf-tile"><Icon n="brief" /></span><div><b>{base.totalCount}</b><span>Open jobs</span></div></div>
        <div className="hf-bi"><span className="hf-tile"><Icon n="check" /></span><div><b>{openings}</b><span>Open positions</span></div></div>
        <div className="hf-bi"><span className="hf-tile"><Icon n="pin" /></span><div><b>{locations.size}</b><span>Locations</span></div></div>
      </div>

      <div className="hf-head" id="jobs">
        <h2>Open positions</h2>
        <span className="hf-count">{countLabel}</span>
      </div>
      {jobs.items.length === 0 ? (
        <div className="hf-card hf-empty">
          No open jobs match your search. <Link href="/">Show all jobs</Link>
        </div>
      ) : (
        <div className="hf-jobs">
          {jobs.items.map((job, i) => (
            <Link key={job.id} href={`/jobs/${job.id}`} className="hf-card hf-job hf-reveal" style={{ '--i': i } as React.CSSProperties}>
              <div className="hf-top">
                <span className="hf-tile"><Icon n="brief" /></span>
                <div>
                  <h3>{job.title}</h3>
                  <div className="hf-meta">
                    <span><Icon n="brief" />{job.department}</span>
                    <span><Icon n="pin" />{job.location}</span>
                  </div>
                </div>
              </div>
              <div className="hf-tags">
                {job.requiredSkills.split(',').slice(0, 4).map(skill => <span key={skill}>{skill.trim()}</span>)}
              </div>
              <div className="hf-go"><span>View &amp; apply</span><Icon n="arrow" /></div>
            </Link>
          ))}
        </div>
      )}

      <section className="hf-sec hf-scroll">
        <h2>How it works</h2>
        <p className="hf-sub">From first click to your first day, all in one place.</p>
        <div className="hf-grid">
          {STEPS.map((s, i) => (
            <div key={s.t} className="hf-card hf-lift">
              <div className="hf-num">{i + 1}</div>
              <h3>{s.t}</h3>
              <p>{s.d}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="hf-sec hf-scroll">
        <h2>Why apply through HireFlow</h2>
        <p className="hf-sub">A clear, fast process for candidates.</p>
        <div className="hf-grid">
          {PERKS.map(p => (
            <div key={p.t} className="hf-card hf-lift">
              <span className="hf-tile hf-tile-gap"><Icon n={p.icon} /></span>
              <h3>{p.t}</h3>
              <p>{p.d}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="hf-cta hf-scroll">
        <div>
          <h2>Already applied?</h2>
          <p>Enter your application code to see where you are.</p>
        </div>
        <Link href="/status">Check my application</Link>
        <ol className="hf-hs">
          {STAGES.map((s, i) => <li key={s} className={i < 2 ? 'done' : i === 2 ? 'now' : undefined}>{s}</li>)}
        </ol>
      </section>
    </>
  );
}
