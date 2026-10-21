// Shapes of the data the HireFlow API sends. They match the backend DTOs.

export type Role = 'Admin' | 'Recruiter' | 'HiringManager';
export type JobStatus = 'Draft' | 'PendingApproval' | 'Open' | 'Closed';
export type Stage = 'Applied' | 'Screening' | 'Interview' | 'Offer' | 'Hired' | 'Rejected';
export type OfferStatus = 'Pending' | 'Approved' | 'Accepted' | 'Declined';

export const STAGES: Stage[] = ['Applied', 'Screening', 'Interview', 'Offer', 'Hired', 'Rejected'];

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  userId: number;
  fullName: string;
  role: Role;
}

export interface Staff {
  id: number;
  fullName: string;
  role: Role;
}

export interface Job {
  id: number;
  title: string;
  department: string;
  location: string;
  description: string;
  requiredSkills: string;
  positions: number;
  status: JobStatus;
  createdAt: string;
}

export interface JobRequest {
  title: string;
  department: string;
  location: string;
  description: string;
  requiredSkills: string;
  positions: number;
}

export interface Application {
  id: number;
  jobId: number;
  jobTitle: string;
  candidateId: number;
  candidateName: string;
  candidateEmail: string;
  candidatePhone: string;
  stage: Stage;
  matchScore: number;
  applicationCode: string;
  rejectionReason: string | null;
  appliedAt: string;
  stageChangedAt: string;
  daysInStage: number;
}

export interface TimelineItem {
  id: number;
  action: string;
  details: string;
  userName: string | null;
  createdAt: string;
}

export interface Scorecard {
  rating: number;
  comments: string;
  recommendation: string;
}

export interface Interview {
  id: number;
  applicationId: number;
  candidateName: string;
  interviewerId: number;
  interviewerName: string;
  startTime: string;
  endTime: string;
  status: 'Scheduled' | 'Completed' | 'Cancelled';
  scorecard: Scorecard | null;
}

export interface Offer {
  id: number;
  applicationId: number;
  candidateName: string;
  jobTitle: string;
  salary: number;
  status: OfferStatus;
  acceptToken: string;
  createdAt: string;
}

export interface OnboardingTask {
  id: number;
  applicationId: number;
  title: string;
  isDone: boolean;
}

export interface Dashboard {
  openJobs: number;
  jobsWaitingApproval: number;
  totalCandidates: number;
  offersWaitingApproval: number;
  averageDaysToHire: number;
  candidatesPerStage: { stage: Stage; count: number }[];
}
