import type { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { getOpenJob } from '@/lib/api';
import ApplyForm from './ApplyForm';

type Props = { params: { id: string } };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const job = await getOpenJob(params.id);
  return job
    ? { title: `${job.title} · HireFlow Careers`, description: job.description.slice(0, 150) }
    : { title: 'Job not found' };
}

export default async function JobPage({ params }: Props) {
  const job = await getOpenJob(params.id);
  if (!job) notFound();

  return (
    <>
      <Link href="/">← All jobs</Link>
      <div className="two" style={{ marginTop: 12 }}>
        <section className="card">
          <h1 style={{ marginTop: 0 }}>{job.title}</h1>
          <p className="muted">{job.department} · {job.location} · {job.positions} {job.positions === 1 ? 'position' : 'positions'}</p>
          <p>{job.description}</p>
          <h3>Skills we look for</h3>
          <div className="tags">
            {job.requiredSkills.split(',').map(skill => <span key={skill}>{skill.trim()}</span>)}
          </div>
        </section>
        <ApplyForm jobId={job.id} />
      </div>
    </>
  );
}
