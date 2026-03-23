import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/auth/auth.service';
import { ContentCardComponent } from '../../shared/components/content-card/content-card.component';
import { Content } from '../../shared/services/content.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, RouterLink, ContentCardComponent],
  template: `
    <div class="profile-container">
      <div class="profile-header">
        <div class="avatar">
          <mat-icon>account_circle</mat-icon>
        </div>
        <div class="user-info">
          <h1>{{ auth.user()?.displayName }}</h1>
          <p class="email">{{ auth.user()?.email }}</p>
          <span class="badge" [class.admin]="auth.isAdmin()">
            {{ auth.isAdmin() ? 'Administrador' : 'Membro' }}
          </span>
        </div>
      </div>

      <section class="section">
        <h2>Minha Lista</h2>
        <div class="cards-row" *ngIf="watchlist().length > 0; else emptyWatchlist">
          <app-content-card *ngFor="let item of watchlist()" [content]="item"></app-content-card>
        </div>
        <ng-template #emptyWatchlist>
          <p class="empty-msg">Sua lista está vazia. Adicione conteúdos para assistir depois!</p>
        </ng-template>
      </section>
    </div>
  `,
  styles: [`
    .profile-container { padding: 100px 40px 60px; background: #141414; min-height: 100vh; }
    .profile-header { display: flex; gap: 24px; align-items: center; margin-bottom: 48px; }
    .avatar mat-icon { font-size: 80px; width: 80px; height: 80px; color: #e50914; }
    h1 { font-size: 2rem; font-weight: 900; }
    .email { color: #aaa; margin-top: 4px; }
    .badge { padding: 4px 12px; border-radius: 12px; font-size: 0.8rem; font-weight: 600; background: rgba(255,255,255,0.1); color: #e5e5e5; margin-top: 8px; display: inline-block; }
    .badge.admin { background: #e50914; }
    .section { margin-bottom: 48px; }
    h2 { font-size: 1.2rem; font-weight: 600; margin-bottom: 16px; color: #e5e5e5; }
    .cards-row { display: flex; gap: 12px; flex-wrap: wrap; }
    .empty-msg { color: #aaa; font-style: italic; }
  `]
})
export class ProfileComponent implements OnInit {
  watchlist = signal<Content[]>([]);

  constructor(public auth: AuthService, private http: HttpClient) {}

  ngOnInit() {
    this.http.get<any[]>('/api/user/watchlist').subscribe(list => {
      this.watchlist.set(list as any);
    });
  }
}
