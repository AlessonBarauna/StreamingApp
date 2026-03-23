import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../../shared/services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const message = resolveMessage(error);
      if (message) toast.show(message, error.status >= 500 ? 'error' : 'warning');
      return throwError(() => error);
    })
  );
};

function resolveMessage(error: HttpErrorResponse): string | null {
  switch (error.status) {
    case 0:    return 'Sem conexão com o servidor.';
    case 400:  return error.error?.error ?? 'Requisição inválida.';
    case 401:  return null; // tratado pelo refreshTokenInterceptor
    case 403:  return 'Você não tem permissão para realizar esta ação.';
    case 404:  return null; // not found é esperado em muitos casos
    case 409:  return error.error?.error ?? 'Conflito de dados.';
    case 422:  return error.error?.error ?? 'Dados inválidos.';
    default:
      if (error.status >= 500) return 'Erro no servidor. Tente novamente mais tarde.';
      return null;
  }
}
