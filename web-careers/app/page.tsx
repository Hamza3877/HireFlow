import Link from 'next/link';
import { getOpenJobs } from '@/lib/api';

// Careers home: hero, search box and the list of open jobs (loaded on the server).
export default async function Home({ searchParams }: { searchParams: { q?: string } }) {
  const search = searchParams.q?.trim() ?? '';
  const jobs = await getOpenJobs(search);

  return (
    <>
      <section className="hero">
        <h1>Find your next role</h1>
        <p>Apply in two minutes. We tell you your skills match right away.</p>
        <form className="search" action="/" method="get">
          <input name="q" defaultValue={search} placeholder="Search by title, department or city" aria-label="Search jobs" />
          <button className="btn light" type="submit">Search</button>
        </form>
      </section>

      <p className="muted">{jobs.totalCount} open {jobs.totalCount === 1 ? 'job' : 'jobs'}</p>
      {jobs.items.length === 0 ? (
        <div className="card">No open jobs match your search.</div>
      ) : (
        <div className="jobs">
          {jobs.items.map(job => (
            <Link key={job.id} href={`/jobs/${job.id}`} className="card job">
              <h3>{job.title}</h3>
              <div className="muted">{job.department} · {job.location}</div>
              <div className="tags">
                {job.requiredSkills.split(',').slice(0, 4).map(skill => <span key={skill}>{skill.trim()}</span>)}
              </div>
            </Link>
          ))}
        </div>
      )}
    </>
  );
}
