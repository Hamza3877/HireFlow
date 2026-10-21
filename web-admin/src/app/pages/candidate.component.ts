import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { Application, Interview, OnboardingTask, Staff, TimelineItem } from '../core/models';
import { ToastService } from '../core/toast.service';

// Candidate profile: details, match score, CV, timeline, interviews, offer and onboarding checklist.
@Component({
  selector: 'app-candidate',
  standalone: true,
  imports: [FormsModule, DatePipe, RouterLink],
  template: `
    <a routerLink="/pipeline">← Back to pipeline</a>
    @if (app) {
      <div class="row" style="align-items:center;margin:12px 0">
        <h1 style="margin:0">{{ app.candidateName }}</h1>
        <span class="chip" [class]="'chip ' + app.stage" style="flex:0 0 auto">{{ app.stage }}</span>
      </div>

      <div class="grid">
        <div class="card"><div class="label muted">Applied for</div><strong>{{ app.jobTitle }}</strong></div>
        <div class="card"><div class="label muted">Match score</div><strong style="font-size:1.4rem">{{ app.matchScore }}%</strong></div>
        <div class="card"><div class="label muted">Contact</div>{{ app.candidateEmail }}<br>{{ app.candidatePhone }}</div>
        <div class="card"><div class="label muted">Applied</div>{{ app.appliedAt | date: 'mediumDate' }}<br><span class="muted">{{ app.daysInStage }} days in stage</span></div>
      </div>
      @if (app.rejectionReason) {
        <div class="card" style="margin-bottom:16px">Rejection reason: {{ app.rejectionReason }}</div>
      }
      <p><button class="btn ghost" (click)="downloadCv()">Download CV</button></p>

      <div class="card" style="margin-bottom:16px">
        <h2>Interviews</h2>
        @for (i of interviews; track i.id) {
          <div style="padding:8px 0;border-bottom:1px solid var(--border)">
            {{ i.startTime | date: 'medium' }} with {{ i.interviewerName }} <span class="chip" [class]="'chip ' + i.status">{{ i.status }}</span>
            @if (i.scorecard) { <div class="muted">Score {{ i.scorecard.rating }}/5 · {{ i.scorecard.recommendation }} · {{ i.scorecard.comments }}</div> }
          </div>
        } @empty { <div class="muted">No interviews yet.</div> }

        @if (auth.hasRole('Admin', 'Recruiter') && app.stage !== 'Rejected' && app.stage !== 'Hired') {
          <h2 style="margin-top:16px">Schedule an interview</h2>
          <div class="row">
            <div><label>Interviewer</label>
              <select [(ngModel)]="interviewerId">
                <option [ngValue]="null">Select…</option>
                @for (s of staff; track s.id) { <option [ngValue]="s.id">{{ s.fullName }} ({{ s.role }})</option> }
              </select></div>
            <div><label>Start</label><input type="datetime-local" [(ngModel)]="start"></div>
            <div><label>End</label><input type="datetime-local" [(ngModel)]="end"></div>
            <button class="btn" [disabled]="!interviewerId || !start || !end" (click)="schedule()">Schedule</button>
          </div>
        }
      </div>

      @if (app.stage === 'Offer' && auth.hasRole('Admin', 'Recruiter')) {
        <div class="card" style="margin-bottom:16px">
          <h2>Create offer</h2>
          <div class="row">
            <div><label>Salary (monthly)</label><input type="number" min="1" [(ngModel)]="salary"></div>
            <button class="btn" [disabled]="!salary" (click)="createOffer()">Create offer</button>
          </div>
          <p class="muted">A hiring manager must approve it on the Offers page.</p>
        </div>
      }

      @if (app.stage === 'Hired') {
        <div class="card" style="margin-bottom:16px">
          <h2>Onboarding checklist</h2>
          <div class="bar-track" style="margin-bottom:12px"><div class="bar-fill" [style.width.%]="progress"></div></div>
          @for (t of tasks; track t.id) {
            <div style="display:flex;justify-content:space-between;align-items:center;padding:6px 0">
              <span [style.text-decoration]="t.isDone ? 'line-through' : 'none'">{{ t.title }}</span>
              @if (!t.isDone && auth.hasRole('Admin', 'Recruiter')) { <button class="btn small" (click)="complete(t)">Mark done</button> }
              @if (t.isDone) { <span class="chip good">Done</span> }
            </div>
          } @empty { <div class="muted">No tasks.</div> }
        </div>
      }

      <div class="card">
        <h2>Timeline</h2>
        @for (item of timeline; track item.id) {
          <div style="padding:6px 0;border-bottom:1px solid var(--border)">
            <strong>{{ item.action }}</strong> — {{ item.details }}
            <div class="muted" style="font-size:.8rem">{{ item.createdAt | date: 'medium' }}{{ item.userName ? ' · ' + item.userName : '' }}</div>
          </div>
        } @empty { <div class="muted">No activity yet.</div> }
      </div>
    } @else {
      <div class="card" style="margin-top:12px"><div class="skeleton"></div><div class="skeleton"></div></div>
    }
  `
})
export class CandidateComponent implements OnInit {
  private api = inject(ApiService);
  private toast = inject(ToastService);
  private route = inject(ActivatedRoute);
  auth = inject(AuthService);

  app: Application | null = null;
  timeline: TimelineItem[] = [];
  interviews: Interview[] = [];
  tasks: OnboardingTask[] = [];
  staff: Staff[] = [];
  interviewerId: number | null = null;
  start = '';
  end = '';
  salary: number | null = null;

  private get id(): number {
    return Number(this.route.snapshot.paramMap.get('id'));
  }

  get progress(): number {
    return this.tasks.length ? (this.tasks.filter(t => t.isDone).length / this.tasks.length) * 100 : 0;
  }

  ngOnInit(): void {
    this.reload();
    this.api.staff().subscribe(s => (this.staff = s));
  }

  reload(): void {
    this.api.application(this.id).subscribe(a => {
      this.app = a;
      if (a.stage === 'Hired') {
        this.api.onboarding(a.id).subscribe(t => (this.tasks = t));
      }
    });
    this.api.timeline(this.id).subscribe(t => (this.timeline = t));
    this.api.interviews(this.id).subscribe(i => (this.interviews = i));
  }

  downloadCv(): void {
    this.api.downloadCv(this.id).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${this.app?.candidateName ?? 'candidate'}-CV`;
      link.click();
      URL.revokeObjectURL(url);
    });
  }

  schedule(): void {
    this.api
      .scheduleInterview(this.id, this.interviewerId!, new Date(this.start).toISOString(), new Date(this.end).toISOString())
      .subscribe(() => {
        this.toast.success('Interview scheduled.');
        this.start = this.end = '';
        this.reload();
      });
  }

  createOffer(): void {
    this.api.createOffer(this.id, this.salary!).subscribe(() => {
      this.toast.success('Offer created. A hiring manager must approve it.');
      this.salary = null;
      this.reload();
    });
  }

  complete(task: OnboardingTask): void {
    this.api.completeTask(task.id).subscribe(() => this.reload());
  }
}
