import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginResponse, Role } from './models';

// Keeps the logged-in user. The token lives in sessionStorage (cleared when the tab closes).
@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  user = signal<LoginResponse | null>(this.loadUser());

  login(email: string, password: string) {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, { email, password })
      .pipe(
        tap(response => {
          sessionStorage.setItem('hireflow-user', JSON.stringify(response));
          this.user.set(response);
        })
      );
  }

  logout(): void {
    sessionStorage.removeItem('hireflow-user');
    this.user.set(null);
    this.router.navigateByUrl('/login');
  }

  get token(): string | null {
    return this.user()?.token ?? null;
  }

  hasRole(...roles: Role[]): boolean {
    const current = this.user();
    return !!current && roles.includes(current.role);
  }

  private loadUser(): LoginResponse | null {
    try {
      const saved = sessionStorage.getItem('hireflow-user');
      if (!saved) {
        return null;
      }
      const user = JSON.parse(saved) as LoginResponse;
      return new Date(user.expiresAt) > new Date() ? user : null;
    } catch {
      return null;
    }
  }
}
