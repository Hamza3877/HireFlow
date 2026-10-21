import { Component, OnInit, inject } from '@angular/core';
import { ApiService } from '../core/api.service';
import { Dashboard } from '../core/models';

// Summary numbers and a simple bar chart made with plain CSS (no chart library).
@Component({
  selector: 'app-dashboard',
  standalone: true,
  template: `
    <h1>Dashboard</h1>
    @if (data) {
      <div class="grid">
        <div class="card stat"><div class="num">{{ data.openJobs }}</div><div class="label">Open jobs</div></div>
        <div class="card stat"><div class="num">{{ data.totalCandidates }}</div><div class="label">Total applications</div></div>
        <div class="card stat"><div class="num">{{ data.averageDaysToHire }}</div><div class="label">Average days to hire</div></div>
        <div class="card stat"><div class="num">{{ data.jobsWaitingApproval }}</div><div class="label">Jobs waiting approval</div></div>
        <div class="card stat"><div class="num">{{ data.offersWaitingApproval }}</div><div class="label">Offers waiting approval</div></div>
      </div>
      <div class="card">
        <h2>Candidates per stage</h2>
        @for (item of data.candidatesPerStage; track item.stage) {
          <div class="bar-row">
            <span class="name">{{ item.stage }}</span>
            <div class="bar-track"><div class="bar-fill" [style.width.%]="percent(item.count)"></div></div>
            <span>{{ item.count }}</span>
          </div>
        }
      </div>
    } @else {
      <div class="card"><div class="skeleton"></div><div class="skeleton"></div><div class="skeleton"></div></div>
    }
  `
})
export class DashboardComponent implements OnInit {
  private api = inject(ApiService);
  data: Dashboard | null = null;

  ngOnInit(): void {
    this.api.dashboard().subscribe(result => (this.data = result));
  }

  percent(count: number): number {
    const max = Math.max(1, ...(this.data?.candidatesPerStage.map(s => s.count) ?? [1]));
    return (count / max) * 100;
  }
}
