import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ApiService } from '../core/api.service';
import { TimelineItem } from '../core/models';

// Admin-only audit trail: who did what and when.
@Component({
  selector: 'app-activity',
  standalone: true,
  imports: [DatePipe],
  template: `
    <h1>Activity log</h1>
    <div class="card">
      @if (!items) {
        <div class="skeleton"></div><div class="skeleton"></div>
      } @else {
        <div class="table-wrap">
          <table>
            <thead><tr><th>When</th><th>Action</th><th>Details</th><th>User</th></tr></thead>
            <tbody>
              @for (i of items; track i.id) {
                <tr><td>{{ i.createdAt | date: 'medium' }}</td><td>{{ i.action }}</td><td style="white-space:normal">{{ i.details }}</td><td>{{ i.userName ?? 'System / candidate' }}</td></tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `
})
export class ActivityComponent implements OnInit {
  private api = inject(ApiService);
  items: TimelineItem[] | null = null;

  ngOnInit(): void {
    this.api.activityLog().subscribe(i => (this.items = i));
  }
}
