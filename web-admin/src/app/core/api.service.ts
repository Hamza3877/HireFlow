import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import {
  Application, Dashboard, Interview, Job, JobRequest, Offer, OnboardingTask,
  PagedResult, Staff, Stage, TimelineItem
} from './models';

// Every call to the HireFlow API in one place, so pages stay small.
@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  // ----- dashboard and log -----
  dashboard() {
    return this.http.get<Dashboard>(`${this.base}/api/dashboard`);
  }

  activityLog() {
    return this.http.get<TimelineItem[]>(`${this.base}/api/activity-log?count=100`);
  }

  staff() {
    return this.http.get<Staff[]>(`${this.base}/api/auth/staff`);
  }

  // ----- jobs -----
  jobs(search: string, status: string, page: number, pageSize = 10) {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) { params = params.set('search', search); }
    if (status) { params = params.set('status', status); }
    return this.http.get<PagedResult<Job>>(`${this.base}/api/jobs`, { params });
  }

  createJob(request: JobRequest) {
    return this.http.post<Job>(`${this.base}/api/jobs`, request);
  }

  updateJob(id: number, request: JobRequest) {
    return this.http.put<Job>(`${this.base}/api/jobs/${id}`, request);
  }

  jobAction(id: number, action: 'submit' | 'approve' | 'close') {
    return this.http.post<Job>(`${this.base}/api/jobs/${id}/${action}`, {});
  }

  // ----- applications -----
  applications(jobId: number | null, search: string, pageSize = 100) {
    let params = new HttpParams().set('pageSize', pageSize);
    if (jobId) { params = params.set('jobId', jobId); }
    if (search) { params = params.set('search', search); }
    return this.http.get<PagedResult<Application>>(`${this.base}/api/applications`, { params });
  }

  application(id: number) {
    return this.http.get<Application>(`${this.base}/api/applications/${id}`);
  }

  timeline(id: number) {
    return this.http.get<TimelineItem[]>(`${this.base}/api/applications/${id}/timeline`);
  }

  moveStage(id: number, stage: Stage, reason?: string) {
    return this.http.post<Application>(`${this.base}/api/applications/${id}/move`, { stage, reason });
  }

  downloadCv(id: number) {
    return this.http.get(`${this.base}/api/applications/${id}/cv`, { responseType: 'blob' });
  }

  // ----- interviews -----
  interviews(applicationId?: number) {
    let params = new HttpParams();
    if (applicationId) { params = params.set('applicationId', applicationId); }
    return this.http.get<Interview[]>(`${this.base}/api/interviews`, { params });
  }

  scheduleInterview(applicationId: number, interviewerId: number, startTime: string, endTime: string) {
    return this.http.post<Interview>(`${this.base}/api/interviews`, { applicationId, interviewerId, startTime, endTime });
  }

  cancelInterview(id: number) {
    return this.http.post<Interview>(`${this.base}/api/interviews/${id}/cancel`, {});
  }

  submitScorecard(id: number, rating: number, comments: string, recommendation: string) {
    return this.http.post<Interview>(`${this.base}/api/interviews/${id}/scorecard`, { rating, comments, recommendation });
  }

  // ----- offers and onboarding -----
  offers() {
    return this.http.get<Offer[]>(`${this.base}/api/offers`);
  }

  createOffer(applicationId: number, salary: number) {
    return this.http.post<Offer>(`${this.base}/api/offers`, { applicationId, salary });
  }

  approveOffer(id: number) {
    return this.http.post<Offer>(`${this.base}/api/offers/${id}/approve`, {});
  }

  onboarding(applicationId: number) {
    return this.http.get<OnboardingTask[]>(`${this.base}/api/onboarding/${applicationId}`);
  }

  completeTask(taskId: number) {
    return this.http.post<OnboardingTask>(`${this.base}/api/onboarding/tasks/${taskId}/complete`, {});
  }
}
