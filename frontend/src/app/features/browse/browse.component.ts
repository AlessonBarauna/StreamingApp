import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { ContentService, Content, Category } from '../../shared/services/content.service';
import { ContentCardComponent } from '../../shared/components/content-card/content-card.component';
import { Router } from '@angular/router';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-browse',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatChipsModule, MatProgressSpinnerModule, MatButtonModule, ContentCardComponent],
  template: `
    <div class="browse-container">
      <div class="filters">
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>Buscar...</mat-label>
          <input matInput [(ngModel)]="searchQuery" (ngModelChange)="onSearch($event)" placeholder="Títulos, descrições...">
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Categoria</mat-label>
          <mat-select [(ngModel)]="selectedCategory" (ngModelChange)="resetAndLoad()">
            <mat-option value="">Todas</mat-option>
            <mat-option *ngFor="let cat of categories()" [value]="cat.id">{{ cat.name }}</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Tipo</mat-label>
          <mat-select [(ngModel)]="selectedType" (ngModelChange)="resetAndLoad()">
            <mat-option value="">Todos</mat-option>
            <mat-option value="Movie">Filmes</mat-option>
            <mat-option value="Series">Séries</mat-option>
          </mat-select>
        </mat-form-field>
      </div>

      <div class="results-info" *ngIf="!loading()">
        {{ total() }} resultado(s) encontrado(s)
      </div>

      <div *ngIf="loading() && items().length === 0" class="loading-center">
        <mat-spinner diameter="40"></mat-spinner>
      </div>

      <div class="grid" *ngIf="items().length > 0">
        <app-content-card
          *ngFor="let item of items(); trackBy: trackById"
          [content]="item"
          (playClick)="onPlay($event)">
        </app-content-card>
      </div>

      <div class="empty-state" *ngIf="!loading() && items().length === 0">
        <p>Nenhum conteúdo encontrado.</p>
      </div>

      <!-- "Carregar mais" -->
      <div class="load-more" *ngIf="hasMore()">
        <button mat-stroked-button (click)="loadMore()" [disabled]="loadingMore()">
          {{ loadingMore() ? 'Carregando...' : 'Carregar mais' }}
        </button>
        <span class="load-more-info">Exibindo {{ items().length }} de {{ total() }}</span>
      </div>
    </div>
  `,
  styles: [`
    .browse-container { padding: 100px 40px 40px; background: #141414; min-height: 100vh; }
    .filters { display: flex; gap: 16px; flex-wrap: wrap; margin-bottom: 24px; }
    .search-field { min-width: 300px; }
    mat-form-field { background: rgba(255,255,255,0.05); border-radius: 4px; }
    .results-info { color: #aaa; font-size: 0.85rem; margin-bottom: 16px; }
    .loading-center { display: flex; justify-content: center; padding: 40px; }
    .grid { display: flex; flex-wrap: wrap; gap: 12px; }
    .empty-state { text-align: center; padding: 60px 0; color: #777; }
    .load-more { display: flex; flex-direction: column; align-items: center; gap: 8px; margin-top: 40px; padding-bottom: 40px; }
    .load-more button { border-color: rgba(255,255,255,0.3); color: #e5e5e5; padding: 0 40px; height: 44px; }
    .load-more button:hover:not([disabled]) { border-color: #fff; }
    .load-more-info { color: #777; font-size: 0.8rem; }
    ::ng-deep .mat-mdc-form-field-label { color: #aaa !important; }
    ::ng-deep .mat-mdc-input-element { color: #fff !important; }
  `]
})
export class BrowseComponent implements OnInit {
  items = signal<Content[]>([]);
  categories = signal<Category[]>([]);
  total = signal(0);
  loading = signal(false);
  loadingMore = signal(false);

  readonly hasMore = computed(() => this.items().length < this.total());

  searchQuery = '';
  selectedCategory = '';
  selectedType = '';

  private currentPage = 1;
  private searchSubject = new Subject<string>();

  constructor(private contentService: ContentService, private router: Router) {}

  ngOnInit(): void {
    this.contentService.getCategories().subscribe(cats => this.categories.set(cats));
    this.resetAndLoad();
    this.searchSubject.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => this.resetAndLoad());
  }

  onSearch(value: string): void {
    this.searchSubject.next(value);
  }

  resetAndLoad(): void {
    this.currentPage = 1;
    this.items.set([]);
    this.loading.set(true);
    this.contentService.getAll(1, PAGE_SIZE, this.selectedCategory || undefined, this.searchQuery || undefined, this.selectedType || undefined)
      .subscribe(result => {
        this.items.set(result.items);
        this.total.set(result.total);
        this.loading.set(false);
      });
  }

  loadMore(): void {
    this.loadingMore.set(true);
    this.currentPage++;
    this.contentService.getAll(this.currentPage, PAGE_SIZE, this.selectedCategory || undefined, this.searchQuery || undefined, this.selectedType || undefined)
      .subscribe(result => {
        this.items.update(current => [...current, ...result.items]);
        this.total.set(result.total);
        this.loadingMore.set(false);
      });
  }

  onPlay(content: Content): void {
    this.router.navigate(['/watch', content.id]);
  }

  trackById(_: number, item: Content): string { return item.id; }
}
