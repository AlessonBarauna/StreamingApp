import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { HttpClient } from '@angular/common/http';
import { ContentService, Content } from '../../shared/services/content.service';
import { ToastService } from '../../shared/services/toast.service';

interface EditForm {
  title: string;
  description: string;
  type: string;
  releaseYear: number;
  durationMinutes: number | null;
  ageRating: string;
  categoryId: string;
  isFeatured: boolean;
}

@Component({
  selector: 'app-admin-content',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatTableModule, MatButtonModule, MatIconModule,
            MatFormFieldModule, MatInputModule, MatSelectModule, MatProgressSpinnerModule, MatSlideToggleModule],
  template: `
    <div class="admin-container">
      <header class="admin-header">
        <h1>Gestão de Conteúdo</h1>
        <a mat-raised-button color="primary" routerLink="/admin/upload">
          <mat-icon>upload</mat-icon> Novo Upload
        </a>
      </header>

      <div *ngIf="loading()" class="loading-center"><mat-spinner diameter="40"></mat-spinner></div>

      <!-- Tabela de conteúdos -->
      <div class="table-wrapper" *ngIf="!loading()">
        <table mat-table [dataSource]="items()" class="content-table">

          <ng-container matColumnDef="title">
            <th mat-header-cell *matHeaderCellDef>Título</th>
            <td mat-cell *matCellDef="let row">
              <a [routerLink]="['/details', row.id]" class="title-link">{{ row.title }}</a>
            </td>
          </ng-container>

          <ng-container matColumnDef="type">
            <th mat-header-cell *matHeaderCellDef>Tipo</th>
            <td mat-cell *matCellDef="let row">{{ row.type }}</td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let row">
              <span class="status-badge" [class]="row.status.toLowerCase()">{{ row.status }}</span>
            </td>
          </ng-container>

          <ng-container matColumnDef="featured">
            <th mat-header-cell *matHeaderCellDef>Destaque</th>
            <td mat-cell *matCellDef="let row">
              <mat-slide-toggle
                [checked]="row.isFeatured"
                (change)="toggleFeatured(row, $event.checked)"
                color="primary">
              </mat-slide-toggle>
            </td>
          </ng-container>

          <ng-container matColumnDef="views">
            <th mat-header-cell *matHeaderCellDef>Views</th>
            <td mat-cell *matCellDef="let row">{{ row.viewCount | number }}</td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Ações</th>
            <td mat-cell *matCellDef="let row">
              <button mat-icon-button (click)="startEdit(row)" title="Editar"><mat-icon>edit</mat-icon></button>
              <button mat-icon-button (click)="deleteContent(row)" title="Excluir"><mat-icon>delete</mat-icon></button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns;" [class.editing-row]="editingId() === row.id"></tr>
        </table>
      </div>

      <!-- Painel de edição inline -->
      <div class="edit-panel" *ngIf="editingId()">
        <h2>Editar Conteúdo</h2>
        <form (ngSubmit)="saveEdit()" class="edit-form">
          <div class="form-row">
            <mat-form-field>
              <mat-label>Título</mat-label>
              <input matInput [(ngModel)]="form.title" name="title" required maxlength="200">
            </mat-form-field>
            <mat-form-field>
              <mat-label>Tipo</mat-label>
              <mat-select [(ngModel)]="form.type" name="type" required>
                <mat-option value="Movie">Filme</mat-option>
                <mat-option value="Series">Série</mat-option>
              </mat-select>
            </mat-form-field>
          </div>
          <mat-form-field class="full-width">
            <mat-label>Descrição</mat-label>
            <textarea matInput [(ngModel)]="form.description" name="description" rows="3" maxlength="2000"></textarea>
          </mat-form-field>
          <div class="form-row">
            <mat-form-field>
              <mat-label>Ano de lançamento</mat-label>
              <input matInput type="number" [(ngModel)]="form.releaseYear" name="releaseYear" required>
            </mat-form-field>
            <mat-form-field>
              <mat-label>Duração (min)</mat-label>
              <input matInput type="number" [(ngModel)]="form.durationMinutes" name="durationMinutes">
            </mat-form-field>
            <mat-form-field>
              <mat-label>Classificação</mat-label>
              <mat-select [(ngModel)]="form.ageRating" name="ageRating">
                <mat-option value="L">Livre</mat-option>
                <mat-option value="10">10+</mat-option>
                <mat-option value="12">12+</mat-option>
                <mat-option value="14">14+</mat-option>
                <mat-option value="16">16+</mat-option>
                <mat-option value="18">18+</mat-option>
              </mat-select>
            </mat-form-field>
          </div>
          <mat-slide-toggle [(ngModel)]="form.isFeatured" name="isFeatured" color="primary">
            Destacar na home
          </mat-slide-toggle>
          <div class="form-actions">
            <button mat-raised-button color="primary" type="submit" [disabled]="saving()">
              {{ saving() ? 'Salvando...' : 'Salvar' }}
            </button>
            <button mat-button type="button" (click)="cancelEdit()">Cancelar</button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .admin-container { padding: 100px 40px 60px; background: #141414; min-height: 100vh; color: #e5e5e5; }
    .admin-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 32px; }
    h1 { font-size: 1.8rem; font-weight: 900; margin: 0; }
    .loading-center { display: flex; justify-content: center; padding: 60px; }
    .table-wrapper { overflow-x: auto; background: rgba(255,255,255,0.03); border-radius: 8px; }
    .content-table { width: 100%; background: transparent; }
    .mat-mdc-header-row { background: rgba(255,255,255,0.06); }
    .mat-mdc-row:hover { background: rgba(255,255,255,0.04); }
    .editing-row { background: rgba(229,9,20,0.08) !important; }
    .title-link { color: #e5e5e5; text-decoration: none; }
    .title-link:hover { color: #fff; text-decoration: underline; }
    .status-badge { padding: 3px 10px; border-radius: 12px; font-size: 0.75rem; font-weight: 600; }
    .status-badge.ready { background: rgba(39,174,96,0.2); color: #2ecc71; }
    .status-badge.processing { background: rgba(241,196,15,0.2); color: #f1c40f; }
    .status-badge.draft { background: rgba(255,255,255,0.1); color: #aaa; }
    .status-badge.failed { background: rgba(231,76,60,0.2); color: #e74c3c; }
    .edit-panel { margin-top: 32px; background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.1); border-radius: 8px; padding: 28px; }
    .edit-panel h2 { font-size: 1.1rem; font-weight: 700; color: #e50914; margin-bottom: 20px; }
    .edit-form { display: flex; flex-direction: column; gap: 12px; }
    .form-row { display: flex; gap: 16px; }
    .form-row mat-form-field { flex: 1; }
    .full-width { width: 100%; }
    .form-actions { display: flex; gap: 12px; margin-top: 8px; }
  `]
})
export class AdminContentComponent implements OnInit {
  items = signal<Content[]>([]);
  loading = signal(true);
  saving = signal(false);
  editingId = signal<string | null>(null);

  columns = ['title', 'type', 'status', 'featured', 'views', 'actions'];
  form: EditForm = this.emptyForm();

  constructor(
    private contentService: ContentService,
    private http: HttpClient,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadAll();
  }

  toggleFeatured(content: Content, isFeatured: boolean): void {
    this.http.put(`/api/content/${content.id}`, { ...content, isFeatured }).subscribe({
      next: () => {
        this.items.update(list => list.map(c => c.id === content.id ? { ...c, isFeatured } : c));
        this.toast.show(isFeatured ? 'Conteúdo destacado na home.' : 'Destaque removido.', 'info');
      }
    });
  }

  startEdit(content: Content): void {
    this.editingId.set(content.id);
    this.form = {
      title: content.title,
      description: content.description,
      type: content.type,
      releaseYear: content.releaseYear,
      durationMinutes: content.durationMinutes ?? null,
      ageRating: content.ageRating,
      categoryId: content.categoryId,
      isFeatured: content.isFeatured
    };
    setTimeout(() => document.querySelector('.edit-panel')?.scrollIntoView({ behavior: 'smooth' }), 50);
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.form = this.emptyForm();
  }

  saveEdit(): void {
    const id = this.editingId();
    if (!id) return;
    this.saving.set(true);
    this.http.put<Content>(`/api/content/${id}`, this.form).subscribe({
      next: updated => {
        this.items.update(list => list.map(c => c.id === id ? updated : c));
        this.saving.set(false);
        this.cancelEdit();
        this.toast.show('Conteúdo atualizado.', 'info');
      },
      error: () => this.saving.set(false)
    });
  }

  deleteContent(content: Content): void {
    if (!confirm(`Excluir "${content.title}"? Esta ação não pode ser desfeita.`)) return;
    this.http.delete(`/api/content/${content.id}`).subscribe({
      next: () => {
        this.items.update(list => list.filter(c => c.id !== content.id));
        this.toast.show('Conteúdo excluído.', 'info');
      }
    });
  }

  private loadAll(): void {
    this.loading.set(true);
    this.contentService.getAll(1, 100).subscribe(result => {
      this.items.set(result.items);
      this.loading.set(false);
    });
  }

  private emptyForm(): EditForm {
    return { title: '', description: '', type: 'Movie', releaseYear: new Date().getFullYear(), durationMinutes: null, ageRating: 'L', categoryId: '', isFeatured: false };
  }
}
