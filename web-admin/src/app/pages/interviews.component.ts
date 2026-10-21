import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { Interview } from '../core/models';
import { ToastService } from '../core/toast.service';

// All interviews. The assigned interviewer fills the scorecard here (star rating 1-5).
@Component({
  selector: 'app-interviews',
  standalone: true,
  imports: [FormsModule, DatePipe, RouterLink],
  template: `
    <h1>Interviews</h1>
    <div class="card">
      @if (!items) {
        <div class="skeleton"></div><div class="skeleton"></div>
      } @else if (items.length === 0) {
        <div class="empty">No interviews scheduled. Schedule one from a candidate profile.</div>
      } @else {
        <div class="table-wrap">
          <table>
            <thead><tr><th>Candidate</th><th>Interviewer</th><th>When</th><th>Status</th><th>Scorecard</th><th></th></tr></thead>
            <tbody>
              @for (i of items; track i.id) {
                <tr>
                  <td><a [routerLink]="['/candidates', i.applicationId]">{{ i.candidateName }}</a></td>
                  <td>{{ i.interviewerName }}</td>
                  <td>{{ i.startTime | date: 'medium' }}</td>
                  <td><span class="chip" [class]="'chip ' + i.status">{{ i.status }}</span></td>
                  <td>
                    @if (i.scorecard) { {{ '★'.repeat(i.scorecard.rating) }} ({{ i.scorecard.recommendation }}) }
                    @else if (i.status === 'Scheduled' && i.interviewerId === auth.user()?.userId) {
                      <button class="btn small" (click)="openId = i.id">Add scorecard</button>
                    } @else { <span class="muted">—</span> }
                  </td>
                  <td>
                    @if (i.status === 'Scheduled' && auth.hasRole('Admin', 'Recruiter')) {
                      <button class="btn ghost small" (click)="cancel(i)">Cancel</button>
                    }
                  </td>
                </tr>
                @if (openId === i.id) {
                  <tr><td colspan="6">
                    <div class="row">
                      <div><label>Rating</label>
                        <select [(ngModel)]="rating">
                          @for (n of [1,2,3,4,5]; track n) { <option [ngValue]="n">{{ '★'.repeat(n) }} ({{ n }})</option> }
                        </select></div>
                      <div><label>Recommendation</label>
                        <select [(ngModel)]="recommendation"><option>Hire</option><option>Maybe</option><option>No hire</option></select></div>
                      <div style="flex:2"><label>Comments</label><input [(ngModel)]="comments" placeholder="Short feedback"></div>
                      <button class="btn" [disabled]="!comments" (click)="submit(i)">Submit</button>
                    </div>
                  </td></tr>
                }
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `
})
export class InterviewsComponent implements OnInit {
  private api = inject(ApiService);
  private toast = inject(ToastService);
  auth = inject(AuthService);

  items: Interview[] | null = null;
  openId: number | null = null;
  rating = 4;
  recommendation = 'Hire';
  comments = '';

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.interviews().subscribe(i => (this.items = i));
  }

  submit(interview: Interview): void {
    this.api.submitScorecard(interview.id, this.rating, this.comments, this.recommendation).subscribe(() => {
      this.toast.success('Scorecard submitted.');
      this.openId = null;
      this.comments = '';
      this.load();
    });
  }

  cancel(interview: Interview): void {
    if (confirm('Cancel this interview?')) {
      this.api.cancelInterview(interview.id).subscribe(() => this.load());
    }
  }
}
