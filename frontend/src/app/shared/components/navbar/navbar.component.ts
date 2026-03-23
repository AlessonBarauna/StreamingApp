import { Component, computed } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule, MatMenuModule],
  template: `
    <mat-toolbar class="navbar">
      <a routerLink="/" class="brand">▶ StreamApp</a>
      <nav class="nav-links">
        <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}">Início</a>
        <a routerLink="/browse" routerLinkActive="active">Explorar</a>
        <a *ngIf="auth.isAdmin()" routerLink="/admin/upload" routerLinkActive="active">Upload</a>
        <a *ngIf="auth.isAdmin()" routerLink="/admin/content" routerLinkActive="active">Conteúdos</a>
      </nav>
      <span class="spacer"></span>
      <button mat-icon-button routerLink="/browse">
        <mat-icon>search</mat-icon>
      </button>
      <button mat-icon-button [matMenuTriggerFor]="userMenu">
        <mat-icon>account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <button mat-menu-item routerLink="/profile">
          <mat-icon>person</mat-icon> Perfil
        </button>
        <button mat-menu-item (click)="auth.logout()">
          <mat-icon>logout</mat-icon> Sair
        </button>
      </mat-menu>
    </mat-toolbar>
  `,
  styles: [`
    .navbar {
      background: linear-gradient(to bottom, rgba(0,0,0,0.9) 0%, transparent 100%);
      position: fixed; top: 0; left: 0; right: 0; z-index: 1000;
      padding: 0 40px; height: 68px;
    }
    .brand {
      color: #e50914; font-size: 1.8rem; font-weight: 900;
      text-decoration: none; margin-right: 32px; letter-spacing: -1px;
    }
    .nav-links { display: flex; gap: 16px; }
    .nav-links a {
      color: #e5e5e5; text-decoration: none; font-size: 0.9rem;
      transition: color 0.2s;
    }
    .nav-links a:hover, .nav-links a.active { color: #fff; font-weight: 600; }
    .spacer { flex: 1; }
    mat-icon { color: #e5e5e5; }
  `]
})
export class NavbarComponent {
  constructor(public auth: AuthService) {}
}
