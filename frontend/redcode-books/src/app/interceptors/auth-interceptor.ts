import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, switchMap, throwError } from 'rxjs';
import { Auth } from '../services/auth';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(Auth);
  const authEndpoint = req.url.includes('/api/auth/');
  const token = auth.getToken();
  const request = token && !authEndpoint
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(request).pipe(
    catchError(error => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || authEndpoint) {
        return throwError(() => error);
      }
      return auth.refreshToken().pipe(
        switchMap(newToken => newToken
          ? next(req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } }))
          : throwError(() => error)
        )
      );
    })
  );
};
