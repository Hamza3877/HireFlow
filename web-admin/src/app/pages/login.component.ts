import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';

// Login page with one-click demo accounts so HR can try each role quickly.
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="login-wrap">
      <div class="card login-card">
        <h1>HireFlow</h1>
        <p class="muted">Sign in to the staff dashboard.</p>
        <form #form="ngForm" (ngSubmit)="submit()">
          <div class="field">
            <label for="email">Email</label>
            <input id="email" name="email" type="email" [(ngModel)]="email" required autocomplete="username">
          </div>
          <div class="field">
            <label for="password">Password</label>
            <input id="password" name="password" type="password" [(ngModel)]="password" required autocomplete="current-password">
          </div>
          <button class="btn" type="submit" [disabled]="form.invalid || loading" style="width:100%">
            {{ loading ? 'Signing in…' : 'Sign in' }}
          </button>
        </form>
        <hr style="border:0;border-top:1px solid var(--border);margin:16px 0">
        <p class="muted" style="margin-top:0">Demo accounts (password Demo&#64;12345):</p>
        <div class="actions">
          <button class="btn ghost small" type="button" (click)="fill('admin@hireflow.test')">Admin</button>
          <button class="btn ghost small" type="button" (click)="fill('recruiter@hireflow.test')">Recruiter</button>
          <button class="btn ghost small" type="button" (click)="fill('manager@hireflow.test')">Hiring manager</button>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  loading = false;

  fill(email: string): void {
    this.email = email;
    this.password = 'Demo@12345';
  }

  submit(): void {
    this.loading = true;
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigateByUrl('/dashboard'),
      error: () => (this.loading = false)
    });
  }
}
