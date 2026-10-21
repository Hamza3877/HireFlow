import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/guards';

// Every page is lazy-loaded: its code is downloaded only when the user opens it.
export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./pages/login.component').then(m => m.LoginComponent) },
  { path: 'dashboard', canActivate: [authGuard], loadComponent: () => import('./pages/dashboard.component').then(m => m.DashboardComponent) },
  { path: 'jobs', canActivate: [authGuard], loadComponent: () => import('./pages/jobs.component').then(m => m.JobsComponent) },
  { path: 'pipeline', canActivate: [authGuard], loadComponent: () => import('./pages/kanban.component').then(m => m.KanbanComponent) },
  { path: 'candidates/:id', canActivate: [authGuard], loadComponent: () => import('./pages/candidate.component').then(m => m.CandidateComponent) },
  { path: 'interviews', canActivate: [authGuard], loadComponent: () => import('./pages/interviews.component').then(m => m.InterviewsComponent) },
  { path: 'offers', canActivate: [authGuard], loadComponent: () => import('./pages/offers.component').then(m => m.OffersComponent) },
  { path: 'activity', canActivate: [authGuard, roleGuard('Admin')], loadComponent: () => import('./pages/activity.component').then(m => m.ActivityComponent) },
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: '**', redirectTo: 'dashboard' }
];
