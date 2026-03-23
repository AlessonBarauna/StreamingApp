import { Component, signal } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatFormFieldModule, MatInputModule, MatButtonModule, MatProgressSpinnerModule],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <div class="brand">▶ StreamApp</div>
        <h2>Criar Conta</h2>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <mat-form-field appearance="fill" class="full-width">
            <mat-label>Nome</mat-label>
            <input matInput formControlName="displayName" autocomplete="name">
          </mat-form-field>
          <mat-form-field appearance="fill" class="full-width">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" autocomplete="email">
          </mat-form-field>
          <mat-form-field appearance="fill" class="full-width">
            <mat-label>Senha</mat-label>
            <input matInput type="password" formControlName="password" autocomplete="new-password">
            <mat-hint>Mínimo 8 caracteres, 1 maiúscula, 1 número, 1 especial</mat-hint>
          </mat-form-field>
          <div class="error-msg" *ngIf="error()">{{ error() }}</div>
          <button mat-raised-button type="submit" [disabled]="loading() || form.invalid" class="submit-btn">
            <mat-spinner *ngIf="loading()" diameter="20"></mat-spinner>
            <span *ngIf="!loading()">Criar Conta</span>
          </button>
        </form>
        <p class="login-link">Já tem conta? <a routerLink="/auth/login">Entrar</a></p>
      </div>
    </div>
  `,
  styles: [`
    .auth-container { min-height: 100vh; display: flex; align-items: center; justify-content: center; background-color: #141414; }
    .auth-card { background: rgba(0,0,0,0.85); padding: 48px 60px; border-radius: 4px; width: 100%; max-width: 450px; }
    .brand { color: #e50914; font-size: 1.8rem; font-weight: 900; margin-bottom: 28px; }
    h2 { font-size: 1.8rem; font-weight: 700; margin-bottom: 24px; }
    .full-width { width: 100%; margin-bottom: 16px; }
    .error-msg { color: #e50914; font-size: 0.85rem; margin-bottom: 12px; }
    .submit-btn { width: 100%; height: 48px; background: #e50914 !important; color: #fff !important; font-size: 1rem; font-weight: 700; margin-top: 8px; }
    .login-link { color: #aaa; margin-top: 20px; font-size: 0.9rem; text-align: center; }
    .login-link a { color: #fff; font-weight: 600; text-decoration: none; }
    ::ng-deep .mdc-text-field--filled { background: rgba(255,255,255,0.07) !important; }
  `]
})
export class RegisterComponent {
  form = this.fb.group({
    displayName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });
  loading = signal(false);
  error = signal('');

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {}

  submit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');
    const { email, password, displayName } = this.form.value;
    this.auth.register(email!, password!, displayName!).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => {
        this.error.set(err.error?.error || 'Erro ao criar conta.');
        this.loading.set(false);
      }
    });
  }
}
