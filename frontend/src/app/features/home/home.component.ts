import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HeroBannerComponent } from '../../shared/components/hero-banner/hero-banner.component';
import { ContentRowComponent } from '../../shared/components/content-row/content-row.component';
import { ContentService, Content } from '../../shared/services/content.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, HeroBannerComponent, ContentRowComponent],
  template: `
    <div class="home-container">
      <app-hero-banner [content]="featuredContent()"></app-hero-banner>
      <div class="rows-container">
        <app-content-row title="Em Alta Agora" [items]="trending()"></app-content-row>
        <app-content-row title="Lançamentos" [items]="newReleases()"></app-content-row>
        <app-content-row title="Todos os Conteúdos" [items]="allContent()"></app-content-row>
      </div>
    </div>
  `,
  styles: [`
    .home-container { background: #141414; min-height: 100vh; padding-top: 0; }
    .rows-container { margin-top: -80px; position: relative; z-index: 1; padding-bottom: 40px; }
  `]
})
export class HomeComponent implements OnInit {
  featuredContent = signal<Content | null>(null);
  trending = signal<Content[]>([]);
  newReleases = signal<Content[]>([]);
  allContent = signal<Content[]>([]);

  constructor(private contentService: ContentService) {}

  ngOnInit() {
    this.contentService.getFeatured().subscribe(items => {
      if (items.length > 0) this.featuredContent.set(items[0]);
    });
    this.contentService.getTrending().subscribe(items => this.trending.set(items));
    this.contentService.getNewReleases().subscribe(items => this.newReleases.set(items));
    this.contentService.getAll().subscribe(result => this.allContent.set(result.items));
  }
}
