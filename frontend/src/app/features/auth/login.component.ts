import { Component, signal } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatFormFieldModule, MatInputModule, MatButtonModule, MatProgressSpinnerModule],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <div class="brand">▶ StreamApp</div>
        <h2>Entrar</h2>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <mat-form-field appearance="fill" class="full-width">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" autocomplete="email">
            <mat-error *ngIf="form.get('email')?.hasError('required')">Email é obrigatório</mat-error>
            <mat-error *ngIf="form.get('email')?.hasError('email')">Email inválido</mat-error>
          </mat-form-field>
          <mat-form-field appearance="fill" class="full-width">
            <mat-label>Senha</mat-label>
            <input matInput type="password" formControlName="password" autocomplete="current-password">
            <mat-error *ngIf="form.get('password')?.hasError('required')">Senha é obrigatória</mat-error>
          </mat-form-field>
          <div class="error-msg" *ngIf="error()">{{ error() }}</div>
          <button mat-raised-button type="submit" [disabled]="loading() || form.invalid" class="submit-btn">
            <mat-spinner *ngIf="loading()" diameter="20"></mat-spinner>
            <span *ngIf="!loading()">Entrar</span>
          </button>
        </form>
        <p class="register-link">Não tem conta? <a routerLink="/auth/register">Cadastre-se</a></p>
        <div class="demo-credentials">
          <p><strong>Demo:</strong> admin@streaming.local / Admin@123456</p>
          <p>user@streaming.local / User@123456</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-container {
      min-height: 100vh; display: flex; align-items: center; justify-content: center;
      background: linear-gradient(rgba(0,0,0,0.6), rgba(0,0,0,0.6)), url('assets/auth-bg.jpg') center/cover;
      background-color: #141414;
    }
    .auth-card {
      background: rgba(0,0,0,0.85); padding: 48px 60px; border-radius: 4px;
      width: 100%; max-width: 450px;
    }
    .brand { color: #e50914; font-size: 1.8rem; font-weight: 900; margin-bottom: 28px; }
    h2 { font-size: 1.8rem; font-weight: 700; margin-bottom: 24px; }
    .full-width { width: 100%; margin-bottom: 16px; }
    .error-msg { color: #e50914; font-size: 0.85rem; margin-bottom: 12px; }
    .submit-btn { width: 100%; height: 48px; background: #e50914 !important; color: #fff !important; font-size: 1rem; font-weight: 700; margin-top: 8px; }
    .register-link { color: #aaa; margin-top: 20px; font-size: 0.9rem; text-align: center; }
    .register-link a { color: #fff; font-weight: 600; text-decoration: none; }
    .demo-credentials { margin-top: 24px; padding: 12px; background: rgba(255,255,255,0.05); border-radius: 4px; font-size: 0.8rem; color: #aaa; }
    ::ng-deep .mat-mdc-form-field-focus-overlay { background: transparent; }
    ::ng-deep .mdc-text-field--filled { background: rgba(255,255,255,0.07) !important; }
  `]
})
export class LoginComponent {
  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });
  loading = signal(false);
  error = signal('');

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router, private route: ActivatedRoute) {}

  submit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');
    const { email, password } = this.form.value;
    this.auth.login(email!, password!).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.error.set(err.error?.error || 'Email ou senha inválidos.');
        this.loading.set(false);
      }
    });
  }
}
