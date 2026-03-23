import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

export interface AuthUser {
  accessToken: string;
  userId: string;
  email: string;
  displayName: string;
  isAdmin: boolean;
  avatarUrl?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _user = signal<AuthUser | null>(this.loadFromStorage());

  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => this._user() !== null);
  readonly isAdmin = computed(() => this._user()?.isAdmin ?? false);
  readonly token = computed(() => this._user()?.accessToken ?? null);

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string): Observable<AuthUser> {
    return this.http.post<AuthUser>('/api/auth/login', { email, password }, { withCredentials: true }).pipe(
      tap(user => this.setUser(user))
    );
  }

  register(email: string, password: string, displayName: string): Observable<AuthUser> {
    return this.http.post<AuthUser>('/api/auth/register', { email, password, displayName }, { withCredentials: true }).pipe(
      tap(user => this.setUser(user))
    );
  }

  /** Usa o cookie HttpOnly de refresh token para obter um novo access token. */
  refresh(): Observable<AuthUser> {
    return this.http.post<AuthUser>('/api/auth/refresh', {}, { withCredentials: true }).pipe(
      tap(user => this.setUser(user))
    );
  }

  logout(): void {
    this.http.post('/api/auth/logout', {}, { withCredentials: true }).subscribe({
      complete: () => this.clearSession()
    });
  }

  /** Chamado pelo interceptor de erro quando o refresh falha — limpa sessão sem chamar API. */
  clearSession(): void {
    this._user.set(null);
    localStorage.removeItem('auth_user');
    this.router.navigate(['/auth/login']);
  }

  updateToken(accessToken: string): void {
    const current = this._user();
    if (current) {
      const updated = { ...current, accessToken };
      this._user.set(updated);
      localStorage.setItem('auth_user', JSON.stringify(updated));
    }
  }

  private setUser(user: AuthUser): void {
    this._user.set(user);
    localStorage.setItem('auth_user', JSON.stringify(user));
  }

  private loadFromStorage(): AuthUser | null {
    try {
      const stored = localStorage.getItem('auth_user');
      return stored ? JSON.parse(stored) : null;
    } catch { return null; }
  }
}
