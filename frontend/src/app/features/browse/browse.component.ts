import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { ContentService, Content, Category } from '../../shared/services/content.service';
import { ContentCardComponent } from '../../shared/components/content-card/content-card.component';
import { Router } from '@angular/router';

@Component({
  selector: 'app-browse',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatChipsModule, MatProgressSpinnerModule, ContentCardComponent],
  template: `
    <div class="browse-container">
      <div class="filters">
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>Buscar...</mat-label>
          <input matInput [(ngModel)]="searchQuery" (ngModelChange)="onSearch($event)" placeholder="Títulos, descrições...">
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Categoria</mat-label>
          <mat-select [(ngModel)]="selectedCategory" (ngModelChange)="loadContent()">
            <mat-option value="">Todas</mat-option>
            <mat-option *ngFor="let cat of categories()" [value]="cat.id">{{ cat.name }}</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Tipo</mat-label>
          <mat-select [(ngModel)]="selectedType" (ngModelChange)="loadContent()">
            <mat-option value="">Todos</mat-option>
            <mat-option value="Movie">Filmes</mat-option>
            <mat-option value="Series">Séries</mat-option>
          </mat-select>
        </mat-form-field>
      </div>
      <div class="results-info" *ngIf="!loading()">
        {{ total() }} resultado(s) encontrado(s)
      </div>
      <div *ngIf="loading()" class="loading-center">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
      <div class="grid" *ngIf="!loading()">
        <app-content-card
          *ngFor="let item of items(); trackBy: trackById"
          [content]="item"
          (playClick)="onPlay($event)">
        </app-content-card>
      </div>
      <div class="load-more-trigger" #loadMoreRef></div>
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
    ::ng-deep .mat-mdc-form-field-label { color: #aaa !important; }
    ::ng-deep .mat-mdc-input-element { color: #fff !important; }
  `]
})
export class BrowseComponent implements OnInit {
  items = signal<Content[]>([]);
  categories = signal<Category[]>([]);
  total = signal(0);
  loading = signal(false);
  page = 1;

  searchQuery = '';
  selectedCategory = '';
  selectedType = '';

  private searchSubject = new Subject<string>();

  constructor(private contentService: ContentService, private router: Router) {}

  ngOnInit() {
    this.contentService.getCategories().subscribe(cats => this.categories.set(cats));
    this.loadContent();

    this.searchSubject.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => this.loadContent());
  }

  onSearch(value: string) {
    this.searchSubject.next(value);
  }

  loadContent() {
    this.loading.set(true);
    this.page = 1;
    this.contentService.getAll(1, 40, this.selectedCategory || undefined, this.searchQuery || undefined, this.selectedType || undefined)
      .subscribe(result => {
        this.items.set(result.items);
        this.total.set(result.total);
        this.loading.set(false);
      });
  }

  onPlay(content: Content) {
    this.router.navigate(['/watch', content.id]);
  }

  trackById(_: number, item: Content) { return item.id; }
}
