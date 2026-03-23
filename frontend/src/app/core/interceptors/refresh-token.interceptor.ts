import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError, BehaviorSubject, filter, take } from 'rxjs';
import { AuthService } from '../auth/auth.service';

let isRefreshing = false;
const refreshDone$ = new BehaviorSubject<string | null>(null);

export const refreshTokenInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const auth = inject(AuthService);

  // Não intercepta chamadas ao próprio endpoint de refresh para evitar loop infinito
  if (req.url.includes('/api/auth/refresh') || req.url.includes('/api/auth/login')) {
    return next(req);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401) return throwError(() => error);

      if (isRefreshing) {
        // Outra requisição já está renovando o token — aguarda e retenta com o novo token
        return refreshDone$.pipe(
          filter(token => token !== null),
          take(1),
          switchMap(token => next(cloneWithToken(req, token!)))
        );
      }

      isRefreshing = true;
      refreshDone$.next(null);

      return auth.refresh().pipe(
        switchMap(user => {
          isRefreshing = false;
          refreshDone$.next(user.accessToken);
          return next(cloneWithToken(req, user.accessToken));
        }),
        catchError(refreshError => {
          isRefreshing = false;
          refreshDone$.next(null);
          auth.clearSession();
          return throwError(() => refreshError);
        })
      );
    })
  );
};

function cloneWithToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}
