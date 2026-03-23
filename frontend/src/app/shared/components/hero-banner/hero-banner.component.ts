import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { Content } from '../../services/content.service';

@Component({
  selector: 'app-hero-banner',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterLink, MatButtonModule, MatIconModule],
  template: `
    <div class="hero" [style.backgroundImage]="'url(' + (content?.backdropUrl || content?.thumbnailUrl) + ')'">
      <div class="gradient"></div>
      <div class="hero-content" *ngIf="content">
        <span class="badge">{{ content.type === 'Movie' ? 'FILME' : 'SÉRIE' }}</span>
        <h1 class="title">{{ content.title }}</h1>
        <p class="description">{{ content.description }}</p>
        <div class="meta">
          <span class="year">{{ content.releaseYear }}</span>
          <span class="rating">{{ content.ageRating }}+</span>
          <span *ngIf="content.durationMinutes" class="duration">{{ content.durationMinutes }}min</span>
        </div>
        <div class="actions">
          <a [routerLink]="['/watch', content.id]" mat-raised-button class="play-btn">
            <mat-icon>play_arrow</mat-icon> Assistir
          </a>
          <a [routerLink]="['/details', content.id]" mat-stroked-button class="info-btn">
            <mat-icon>info</mat-icon> Mais informações
          </a>
        </div>
      </div>
      <div class="bottom-fade"></div>
    </div>
  `,
  styles: [`
    .hero {
      height: 85vh; position: relative;
      background-size: cover; background-position: center top;
      background-color: #141414;
    }
    .gradient {
      position: absolute; inset: 0;
      background: linear-gradient(to right, rgba(20,20,20,0.9) 30%, transparent 70%),
                  linear-gradient(to top, #141414 0%, transparent 30%);
    }
    .hero-content {
      position: absolute; bottom: 20%; left: 40px; max-width: 500px; z-index: 1;
    }
    .badge {
      background: #e50914; color: #fff; padding: 2px 8px;
      font-size: 0.75rem; font-weight: 700; letter-spacing: 2px;
      border-radius: 2px; display: inline-block; margin-bottom: 12px;
    }
    .title { font-size: 3rem; font-weight: 900; line-height: 1.1; margin-bottom: 12px; text-shadow: 2px 2px 4px rgba(0,0,0,0.7); }
    .description { font-size: 1rem; color: #e5e5e5; line-height: 1.5; margin-bottom: 12px; display: -webkit-box; -webkit-line-clamp: 3; -webkit-box-orient: vertical; overflow: hidden; }
    .meta { display: flex; gap: 12px; font-size: 0.85rem; color: #aaa; margin-bottom: 20px; align-items: center; }
    .rating { border: 1px solid #aaa; padding: 0 6px; border-radius: 2px; }
    .actions { display: flex; gap: 12px; }
    .play-btn { background: #fff !important; color: #000 !important; font-weight: 700; padding: 0 24px; height: 44px; }
    .info-btn { border-color: rgba(255,255,255,0.5) !important; color: #fff !important; padding: 0 24px; height: 44px; }
    mat-icon { margin-right: 6px; }
    .bottom-fade {
      position: absolute; bottom: 0; left: 0; right: 0; height: 120px;
      background: linear-gradient(to top, #141414, transparent);
    }
  `]
})
export class HeroBannerComponent {
  @Input() content: Content | null = null;
}
