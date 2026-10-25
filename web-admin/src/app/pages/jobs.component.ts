import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { Job, JobRequest, PagedResult } from '../core/models';
import { ToastService } from '../core/toast.service';

// Jobs list with search, filter and paging, plus create/edit and the approval actions.
@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [FormsModule, DatePipe],
  template: `
        @if (showForm) {
      <div style="margin-bottom:12px">
        <button class="btn ghost" type="button" style="background:#fff;color:var(--primary);border-color:#c7d2fe;font-weight:600" (click)="showForm = false">← All jobs</button>
      </div>
      <h1 style="margin:0 0 12px">{{ editingId ? 'Edit job' : 'New job' }}</h1>
    } @else {
      <div class="row" style="align-items:center;margin-bottom:12px">
        <h1 style="margin:0">Jobs</h1>
        @if (auth.hasRole('Admin', 'Recruiter')) {
          <button class="btn" style="flex:0 0 auto" (click)="newJob()">New job</button>
        }
      </div>
    }

    @if (showForm) {
      <form class="card" #f="ngForm" (ngSubmit)="save()" style="margin-bottom:16px">
        <div class="row">
          <div class="field"><label>Title</label><input name="title" [(ngModel)]="form.title" required maxlength="150"></div>
          <div class="field"><label>Department</label><input name="department" [(ngModel)]="form.department" required></div>
          <div class="field"><label>Location</label><input name="location" [(ngModel)]="form.location" required></div>
          <div class="field"><label>Positions</label><input name="positions" type="number" min="1" [(ngModel)]="form.positions" required></div>
        </div>
        <div class="field"><label>Required skills (comma separated)</label><input name="skills" [(ngModel)]="form.requiredSkills" required placeholder="C#, SQL Server, Docker"></div>
        <div class="field"><label>Description</label><textarea name="description" rows="3" [(ngModel)]="form.description" required></textarea></div>
        <div class="actions">
          <button class="btn" type="submit" [disabled]="f.invalid">Save as draft</button>
          <button class="btn ghost" type="button" (click)="showForm = false">Cancel</button>
        </div>
      </form>
    }

    @if (!showForm) {
    <div class="card">
      <div class="row" style="margin-bottom:12px">
        <div><label>Search</label><input [(ngModel)]="search" (keyup.enter)="load(1)" placeholder="Title, department, location"></div>
        <div>
          <label>Status</label>
          <select [(ngModel)]="status" (change)="load(1)">
            <option value="">All</option>
            <option>Draft</option><option>PendingApproval</option><option>Open</option><option>Closed</option>
          </select>
        </div>
        <button class="btn ghost" (click)="load(1)">Search</button>
      </div>

      @if (!result) {
        <div class="skeleton"></div><div class="skeleton"></div><div class="skeleton"></div>
      } @else if (result.items.length === 0) {
        <div class="empty">No jobs found.</div>
      } @else {
        <div class="table-wrap">
          <table>
            <thead><tr><th>Title</th><th>Department</th><th>Location</th><th>Positions</th><th>Status</th><th>Created</th><th>Actions</th></tr></thead>
            <tbody>
              @for (job of result.items; track job.id) {
                <tr>
                  <td><strong>{{ job.title }}</strong></td>
                  <td>{{ job.department }}</td>
                  <td>{{ job.location }}</td>
                  <td>{{ job.positions }}</td>
                  <td><span class="chip" [class]="'chip ' + job.status">{{ job.status }}</span></td>
                  <td>{{ job.createdAt | date: 'mediumDate' }}</td>
                  <td class="actions">
                    @if (job.status === 'Draft' && auth.hasRole('Admin', 'Recruiter')) {
                      <button class="btn ghost small" (click)="edit(job)">Edit</button>
                      <button class="btn small" (click)="act(job, 'submit')">Submit</button>
                    }
                    @if (job.status === 'PendingApproval' && auth.hasRole('Admin', 'HiringManager')) {
                      <button class="btn small" (click)="act(job, 'approve')">Approve</button>
                    }
                    @if (job.status !== 'Closed') {
                      <button class="btn danger small" (click)="act(job, 'close')">Close</button>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        <div class="pager">
          <span class="muted">Page {{ result.page }} of {{ pages }} · {{ result.totalCount }} jobs</span>
          <button class="btn ghost small" [disabled]="result.page <= 1" (click)="load(result.page - 1)">Previous</button>
          <button class="btn ghost small" [disabled]="result.page >= pages" (click)="load(result.page + 1)">Next</button>
        </div>
      }
    </div>
}
  `
})
export class JobsComponent implements OnInit {
  private api = inject(ApiService);
  private toast = inject(ToastService);
  auth = inject(AuthService);

  result: PagedResult<Job> | null = null;
  search = '';
  status = '';
  showForm = false;
  editingId: number | null = null;
  form: JobRequest = this.emptyForm();

  get pages(): number {
    return this.result ? Math.max(1, Math.ceil(this.result.totalCount / this.result.pageSize)) : 1;
  }

  ngOnInit(): void {
    this.load(1);
  }

  load(page: number): void {
    this.api.jobs(this.search, this.status, page).subscribe(r => (this.result = r));
  }

  newJob(): void {
    this.editingId = null;
    this.form = this.emptyForm();
    this.showForm = true;
  }

  edit(job: Job): void {
    this.editingId = job.id;
    this.form = { ...job };
    this.showForm = true;
  }

  save(): void {
    const request = this.editingId ? this.api.updateJob(this.editingId, this.form) : this.api.createJob(this.form);
    request.subscribe(() => {
      this.toast.success('Job saved.');
      this.showForm = false;
      this.load(1);
    });
  }

  act(job: Job, action: 'submit' | 'approve' | 'close'): void {
    if (action === 'close' && !confirm(`Close "${job.title}"?`)) {
      return;
    }
    this.api.jobAction(job.id, action).subscribe(() => {
      this.toast.success(`Job ${action === 'submit' ? 'submitted for approval' : action === 'approve' ? 'approved' : 'closed'}.`);
      this.load(this.result?.page ?? 1);
    });
  }

  private emptyForm(): JobRequest {
    return { title: '', department: '', location: '', description: '', requiredSkills: '', positions: 1 };
  }
}
