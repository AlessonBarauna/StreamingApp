import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ContentCardComponent } from '../content-card/content-card.component';
import { Content } from '../../services/content.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-content-row',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatIconModule, MatButtonModule, ContentCardComponent],
  template: `
    <section class="row-section">
      <h2 class="row-title">{{ title }}</h2>
      <div class="scroll-container" #scrollContainer>
        <button class="scroll-btn left" mat-icon-button (click)="scroll(-1)" *ngIf="items.length > 4">
          <mat-icon>chevron_left</mat-icon>
        </button>
        <div class="cards-track" #track>
          <app-content-card
            *ngFor="let item of items; trackBy: trackById"
            [content]="item"
            (playClick)="onPlay($event)"
            (addListClick)="onAddList($event)">
          </app-content-card>
        </div>
        <button class="scroll-btn right" mat-icon-button (click)="scroll(1)" *ngIf="items.length > 4">
          <mat-icon>chevron_right</mat-icon>
        </button>
      </div>
    </section>
  `,
  styles: [`
    .row-section { margin: 8px 0 24px; padding: 0 40px; position: relative; }
    .row-title { font-size: 1rem; font-weight: 600; color: #e5e5e5; margin-bottom: 12px; }
    .scroll-container { position: relative; overflow: hidden; }
    .cards-track {
      display: flex; gap: 8px; overflow-x: auto; scroll-behavior: smooth;
      scrollbar-width: none; padding: 12px 0;
    }
    .cards-track::-webkit-scrollbar { display: none; }
    .scroll-btn {
      position: absolute; top: 50%; transform: translateY(-50%);
      z-index: 5; background: rgba(0,0,0,0.6) !important;
      color: #fff !important;
    }
    .scroll-btn.left { left: -12px; }
    .scroll-btn.right { right: -12px; }
  `]
})
export class ContentRowComponent {
  @Input({ required: true }) title!: string;
  @Input({ required: true }) items!: Content[];

  constructor(private router: Router) {}

  scroll(dir: number) {
    const track = document.querySelector('.cards-track') as HTMLElement;
    if (track) track.scrollBy({ left: dir * 640, behavior: 'smooth' });
  }

  onPlay(content: Content) {
    this.router.navigate(['/watch', content.id]);
  }

  onAddList(content: Content) {}

  trackById(_: number, item: Content) { return item.id; }
}
