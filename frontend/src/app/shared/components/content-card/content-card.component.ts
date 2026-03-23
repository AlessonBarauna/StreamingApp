import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { Content } from '../../services/content.service';

@Component({
  selector: 'app-content-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterLink, MatButtonModule, MatIconModule],
  template: `
    <div class="card" [routerLink]="['/details', content.id]">
      <div class="thumbnail">
        <img [src]="content.thumbnailUrl || 'assets/placeholder.jpg'"
             [alt]="content.title"
             loading="lazy"
             (error)="onImgError($event)">
        <div class="overlay">
          <div class="overlay-info">
            <span class="title">{{ content.title }}</span>
            <div class="actions">
              <button mat-icon-button (click)="play($event)" title="Assistir">
                <mat-icon>play_circle</mat-icon>
              </button>
              <button mat-icon-button (click)="addList($event)" title="Minha Lista">
                <mat-icon>add</mat-icon>
              </button>
            </div>
            <div class="meta">
              <span class="age">{{ content.ageRating }}</span>
              <span class="type">{{ content.type === 'Movie' ? 'Filme' : 'Série' }}</span>
              <span class="year">{{ content.releaseYear }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      cursor: pointer; position: relative; border-radius: 4px; overflow: hidden;
      transition: transform 0.3s ease, z-index 0s;
      flex-shrink: 0; width: 200px;
    }
    .card:hover { transform: scale(1.1); z-index: 10; }
    .thumbnail { position: relative; aspect-ratio: 16/9; background: #333; }
    img { width: 100%; height: 100%; object-fit: cover; display: block; }
    .overlay {
      position: absolute; inset: 0;
      background: linear-gradient(to top, rgba(0,0,0,0.95) 0%, transparent 50%);
      opacity: 0; transition: opacity 0.3s; display: flex; align-items: flex-end;
    }
    .card:hover .overlay { opacity: 1; }
    .overlay-info { padding: 8px; width: 100%; }
    .title { font-size: 0.8rem; font-weight: 600; display: block; margin-bottom: 4px; }
    .actions { display: flex; gap: 4px; margin-bottom: 4px; }
    .actions button { width: 28px; height: 28px; }
    mat-icon { font-size: 20px; color: #fff; }
    .meta { display: flex; gap: 6px; font-size: 0.7rem; align-items: center; }
    .age {
      border: 1px solid #aaa; padding: 0 4px; font-size: 0.65rem; color: #aaa;
    }
    .type, .year { color: #aaa; }
  `]
})
export class ContentCardComponent {
  @Input({ required: true }) content!: Content;
  @Output() playClick = new EventEmitter<Content>();
  @Output() addListClick = new EventEmitter<Content>();

  play(e: Event) {
    e.stopPropagation();
    this.playClick.emit(this.content);
  }

  addList(e: Event) {
    e.stopPropagation();
    this.addListClick.emit(this.content);
  }

  onImgError(e: Event) {
    (e.target as HTMLImageElement).src = 'assets/placeholder.jpg';
  }
}
