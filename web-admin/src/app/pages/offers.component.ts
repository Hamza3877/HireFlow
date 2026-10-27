import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
   import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { Offer } from '../core/models';
import { ToastService } from '../core/toast.service';

// Offers list. A hiring manager (or admin) approves; then the candidate link can be shared.
@Component({
  selector: 'app-offers',
  standalone: true,
  imports: [DatePipe, RouterLink],
  template: `
    <h1>Offers</h1>
    <div class="card">
      @if (!items) {
        <div class="skeleton"></div><div class="skeleton"></div>
      } @else if (items.length === 0) {
        <div class="empty">No offers yet. Create one from a candidate in the Offer stage.</div>
      } @else {
        <div class="table-wrap">
          <table>
            <thead><tr><th>Candidate</th><th>Job</th><th>Salary</th><th>Status</th><th>Created</th><th>Actions</th></tr></thead>
            <tbody>
              @for (o of items; track o.id) {
                <tr>
                  <td><a [routerLink]="['/candidates', o.applicationId]" [queryParams]="{ from: 'offers' }">{{ o.candidateName }}</a></td>
                  <td>{{ o.jobTitle }}</td>
                  <td>{{ o.salary }}</td>
                  <td><span class="chip" [class]="'chip ' + o.status">{{ o.status }}</span></td>
                  <td>{{ o.createdAt | date: 'mediumDate' }}</td>
                 <td>
  <div class="actions">
    @if (o.status === 'Pending' && auth.hasRole('Admin', 'HiringManager')) {
      <button class="btn small" (click)="approve(o)">Approve</button>
    }
    @if (o.status === 'Pending' && !auth.hasRole('Admin', 'HiringManager')) {
      <span class="muted">Waiting for manager approval</span>
    }
    @if (o.status === 'Approved') {
      <button class="btn ghost small" (click)="copyLink(o)">Copy candidate link</button>
    }
  </div>
</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `
})
export class OffersComponent implements OnInit {
  private api = inject(ApiService);
  private toast = inject(ToastService);
  auth = inject(AuthService);

  private router = inject(Router);

  openCandidate(applicationId: number): void {
    this.router.navigate(['/candidates', applicationId], { queryParams: { from: 'offers' } });
  }

  items: Offer[] | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.offers().subscribe(o => (this.items = o));
  }

  approve(offer: Offer): void {
    this.api.approveOffer(offer.id).subscribe(() => {
      this.toast.success('Offer approved.');
      this.load();
    });
  }

  // The link opens the Next.js careers site's offer page. Change the address when you deploy.
  copyLink(offer: Offer): void {
       const link = `https://hireflow-careers-fawn.vercel.app/offer/${offer.acceptToken}`;
    navigator.clipboard.writeText(link).then(() => this.toast.success('Link copied. Send it to the candidate.'));
  }
}
