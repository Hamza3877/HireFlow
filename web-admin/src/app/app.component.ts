import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';
import { ToastService } from './core/toast.service';

// The page frame: top bar, side menu (a drawer on phones), and the area where pages appear.
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    @if (auth.user(); as user) {
      <header class="topbar">
        <button class="menu" (click)="open = !open" aria-label="Open menu">☰</button>
        <span class="brand">HireFlow</span>
        <span class="spacer"></span>
        <span class="who">{{ user.fullName }} · {{ user.role }}</span>
        <button class="btn ghost small" (click)="auth.logout()">Log out</button>
      </header>
      <div class="layout">
        <nav class="side" [class.open]="open" (click)="open = false">
          <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
          <a routerLink="/jobs" routerLinkActive="active">Jobs</a>
          <a routerLink="/pipeline" routerLinkActive="active">Pipeline</a>
          <a routerLink="/interviews" routerLinkActive="active">Interviews</a>
          <a routerLink="/offers" routerLinkActive="active">Offers</a>
          @if (auth.hasRole('Admin')) {
            <a routerLink="/activity" routerLinkActive="active">Activity log</a>
          }
        </nav>
        <main class="content"><router-outlet /></main>
      </div>
    } @else {
      <router-outlet />
    }

    @if (toast.slow()) {
      <div class="banner">Server is waking up, please wait a moment…</div>
    }
    @if (toast.message(); as text) {
      <div class="toast" [class.error]="toast.kind() === 'error'" role="status">{{ text }}</div>
    }
  `
})
export class AppComponent {
  auth = inject(AuthService);
  toast = inject(ToastService);
  open = false;
}
