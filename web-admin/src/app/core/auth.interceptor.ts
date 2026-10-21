import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, finalize, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { ToastService } from './toast.service';

// Runs on every HTTP call: adds the token, shows friendly errors, and shows the
// "server is waking up" banner when a request is slow (free hosts go to sleep).
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const toast = inject(ToastService);

  const token = auth.token;
  const authorized = token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;

  const slowTimer = setTimeout(() => toast.slow.set(true), 4000);

  return next(authorized).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && auth.user()) {
        auth.logout();
      }
      toast.error(readMessage(error));
      return throwError(() => error);
    }),
    finalize(() => {
      clearTimeout(slowTimer);
      toast.slow.set(false);
    })
  );
};

function readMessage(error: HttpErrorResponse): string {
  if (error.status === 0) {
    return 'Cannot reach the server. Is the API running?';
  }
  const body = error.error;
  if (body?.detail) {
    return body.detail;
  }
  if (body?.errors) {
    const first = Object.values(body.errors)[0] as string[] | undefined;
    if (first?.length) {
      return first[0];
    }
  }
  return body?.title ?? 'Something went wrong.';
}
