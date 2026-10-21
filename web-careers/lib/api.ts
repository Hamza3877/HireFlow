// Talks to the HireFlow public API. The address comes from NEXT_PUBLIC_API_URL.
export const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

export interface Job {
  id: number;
  title: string;
  department: string;
  location: string;
  description: string;
  requiredSkills: string;
  positions: number;
}

export interface PagedJobs {
  items: Job[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface StatusResult {
  jobTitle: string;
  stage: string;
  appliedAt: string;
}

export interface PublicOffer {
  candidateName: string;
  jobTitle: string;
  salary: number;
  status: string;
}

// Turns an API error body (ProblemDetails) into a short message for the user.
export async function readError(response: Response): Promise<string> {
  try {
    const body = await response.json();
    if (body?.detail) return body.detail;
    if (body?.errors) {
      const first = Object.values(body.errors)[0] as string[] | undefined;
      if (first?.length) return first[0];
    }
    return body?.title ?? 'Something went wrong.';
  } catch {
    return 'Something went wrong.';
  }
}

export async function getOpenJobs(search: string): Promise<PagedJobs> {
  const query = new URLSearchParams({ pageSize: '50' });
  if (search) query.set('search', search);
  const response = await fetch(`${API_URL}/api/public/jobs?${query}`, { cache: 'no-store' });
  if (!response.ok) throw new Error('Could not load jobs.');
  return response.json();
}

export async function getOpenJob(id: string): Promise<Job | null> {
  const response = await fetch(`${API_URL}/api/public/jobs/${id}`, { cache: 'no-store' });
  if (response.status === 404) return null;
  if (!response.ok) throw new Error('Could not load the job.');
  return response.json();
}
