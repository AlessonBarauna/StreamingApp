import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { refreshTokenInterceptor } from './core/interceptors/refresh-token.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([
      authInterceptor,         // 1. injeta o Bearer token nas requests
      refreshTokenInterceptor, // 2. captura 401, renova token e retenta
    ])),
    provideAnimations(),
  ]
};
