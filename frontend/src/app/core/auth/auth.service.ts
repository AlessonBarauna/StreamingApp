import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

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

  login(email: string, password: string) {
    return this.http.post<AuthUser>('/api/auth/login', { email, password }).pipe(
      tap(user => this.setUser(user))
    );
  }

  register(email: string, password: string, displayName: string) {
    return this.http.post<AuthUser>('/api/auth/register', { email, password, displayName }).pipe(
      tap(user => this.setUser(user))
    );
  }

  logout() {
    this._user.set(null);
    localStorage.removeItem('auth_user');
    this.router.navigate(['/auth/login']);
  }

  private setUser(user: AuthUser) {
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
