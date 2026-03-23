import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { ContentService, Content } from '../../shared/services/content.service';
import { HttpClient } from '@angular/common/http';

interface Episode {
  id: string;
  title: string;
  description: string;
  seasonNumber: number;
  episodeNumber: number;
  durationMinutes: number;
  thumbnailUrl?: string;
  hlsManifestUrl?: string;
  status: string;
}

@Component({
  selector: 'app-details',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatIconModule, MatTabsModule],
  template: `
    <div class="details-container" *ngIf="content()">
      <div class="backdrop" [style.backgroundImage]="'url(' + (content()!.backdropUrl || content()!.thumbnailUrl) + ')'">
        <div class="backdrop-gradient"></div>
      </div>
      <div class="content-info">
        <div class="badges">
          <span class="type-badge">{{ content()!.type === 'Movie' ? 'FILME' : 'SÉRIE' }}</span>
          <span class="age-badge">{{ content()!.ageRating }}+</span>
        </div>
        <h1>{{ content()!.title }}</h1>
        <div class="meta">
          <span>{{ content()!.releaseYear }}</span>
          <span *ngIf="content()!.durationMinutes">{{ content()!.durationMinutes }}min</span>
          <span class="category">{{ content()!.categoryName }}</span>
        </div>
        <p class="description">{{ content()!.description }}</p>
        <div class="actions">
          <a [routerLink]="['/watch', content()!.id]" mat-raised-button class="play-btn" [class.disabled]="content()!.status !== 'Ready'">
            <mat-icon>play_arrow</mat-icon>
            {{ content()!.status === 'Ready' ? 'Assistir' : 'Indisponível' }}
          </a>
          <button mat-icon-button (click)="toggleWatchlist()" [title]="inWatchlist() ? 'Remover da Lista' : 'Adicionar à Lista'">
            <mat-icon>{{ inWatchlist() ? 'check' : 'add' }}</mat-icon>
          </button>
          <button mat-icon-button (click)="rate(true)" title="Gostei">
            <mat-icon>thumb_up</mat-icon>
          </button>
          <button mat-icon-button (click)="rate(false)" title="Não Gostei">
            <mat-icon>thumb_down</mat-icon>
          </button>
        </div>

        <ng-container *ngIf="content()!.type === 'Series' && episodes().length > 0">
          <h2 class="episodes-title">Episódios</h2>
          <div class="episodes-list">
            <div class="episode-item" *ngFor="let ep of episodes()">
              <img [src]="ep.thumbnailUrl || 'assets/placeholder.jpg'" [alt]="ep.title" class="ep-thumb">
              <div class="ep-info">
                <span class="ep-num">T{{ ep.seasonNumber }}E{{ ep.episodeNumber }}</span>
                <span class="ep-title">{{ ep.title }}</span>
                <span class="ep-desc">{{ ep.description }}</span>
                <span class="ep-duration">{{ ep.durationMinutes }}min</span>
              </div>
            </div>
          </div>
        </ng-container>
      </div>
    </div>
    <div *ngIf="loading()" class="loading-screen">
      <div class="spinner"></div>
    </div>
  `,
  styles: [`
    .details-container { background: #141414; min-height: 100vh; }
    .backdrop {
      height: 70vh; background-size: cover; background-position: center;
      position: relative;
    }
    .backdrop-gradient {
      position: absolute; inset: 0;
      background: linear-gradient(to right, rgba(20,20,20,0.95) 30%, transparent),
                  linear-gradient(to top, #141414 0%, transparent 40%);
    }
    .content-info { padding: 0 40px 60px; margin-top: -200px; position: relative; z-index: 1; max-width: 700px; }
    .badges { display: flex; gap: 8px; margin-bottom: 12px; }
    .type-badge { background: #e50914; color: #fff; padding: 3px 10px; font-size: 0.75rem; font-weight: 700; letter-spacing: 1px; border-radius: 2px; }
    .age-badge { border: 1px solid #aaa; color: #aaa; padding: 3px 8px; font-size: 0.75rem; border-radius: 2px; }
    h1 { font-size: 2.5rem; font-weight: 900; margin-bottom: 8px; }
    .meta { display: flex; gap: 12px; color: #aaa; font-size: 0.9rem; margin-bottom: 16px; }
    .category { color: #e50914; }
    .description { color: #e5e5e5; line-height: 1.6; margin-bottom: 24px; font-size: 1rem; }
    .actions { display: flex; gap: 12px; align-items: center; margin-bottom: 40px; }
    .play-btn { background: #fff !important; color: #000 !important; font-weight: 700; padding: 0 28px; height: 44px; }
    .play-btn.disabled { opacity: 0.5; pointer-events: none; }
    mat-icon { color: #e5e5e5; }
    .episodes-title { font-size: 1.2rem; font-weight: 700; margin-bottom: 16px; }
    .episodes-list { display: flex; flex-direction: column; gap: 12px; }
    .episode-item { display: flex; gap: 16px; background: rgba(255,255,255,0.05); border-radius: 4px; overflow: hidden; }
    .ep-thumb { width: 140px; height: 80px; object-fit: cover; flex-shrink: 0; }
    .ep-info { padding: 12px; display: flex; flex-direction: column; gap: 4px; }
    .ep-num { color: #e50914; font-size: 0.8rem; font-weight: 700; }
    .ep-title { font-weight: 600; }
    .ep-desc { color: #aaa; font-size: 0.85rem; }
    .ep-duration { color: #777; font-size: 0.8rem; }
    .loading-screen { display: flex; align-items: center; justify-content: center; height: 100vh; background: #141414; }
    .spinner { width: 48px; height: 48px; border: 4px solid rgba(255,255,255,0.2); border-top-color: #e50914; border-radius: 50%; animation: spin 1s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class DetailsComponent implements OnInit {
  content = signal<Content | null>(null);
  episodes = signal<Episode[]>([]);
  loading = signal(true);
  inWatchlist = signal(false);

  private contentId!: string;

  constructor(private route: ActivatedRoute, private contentService: ContentService) {}

  ngOnInit() {
    this.contentId = this.route.snapshot.params['contentId'];
    this.contentService.getById(this.contentId).subscribe(content => {
      this.content.set(content);
      this.loading.set(false);
      if (content.type === 'Series') {
        this.contentService['http'].get<Episode[]>(`/api/content/${this.contentId}/episodes`)
          .subscribe(eps => this.episodes.set(eps));
      }
    });
  }

  toggleWatchlist() {
    const current = this.inWatchlist();
    if (current) {
      this.contentService.removeFromWatchlist(this.contentId).subscribe(() => this.inWatchlist.set(false));
    } else {
      this.contentService.addToWatchlist(this.contentId).subscribe(() => this.inWatchlist.set(true));
    }
  }

  rate(isLiked: boolean) {
    this.contentService.rate(this.contentId, isLiked).subscribe();
  }
}
