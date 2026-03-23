import { Component, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatStepperModule } from '@angular/material/stepper';
import { MatIconModule } from '@angular/material/icon';
import { HttpClient } from '@angular/common/http';
import { ContentService, Category, Content } from '../../shared/services/content.service';
import * as signalR from '@microsoft/signalr';
import { AuthService } from '../../core/auth/auth.service';

type UploadStep = 'form' | 'uploading' | 'transcoding' | 'done' | 'error';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatProgressBarModule, MatStepperModule, MatIconModule],
  template: `
    <div class="upload-container">
      <h1>Upload de Conteúdo</h1>

      <div class="step-indicator">
        <div class="step" [class.active]="step() === 'form'" [class.done]="stepIndex() > 0">
          <mat-icon>edit</mat-icon> Detalhes
        </div>
        <div class="step" [class.active]="step() === 'uploading'" [class.done]="stepIndex() > 1">
          <mat-icon>upload</mat-icon> Upload
        </div>
        <div class="step" [class.active]="step() === 'transcoding'" [class.done]="stepIndex() > 2">
          <mat-icon>video_settings</mat-icon> Transcodificação
        </div>
        <div class="step" [class.active]="step() === 'done'">
          <mat-icon>check_circle</mat-icon> Concluído
        </div>
      </div>

      <div class="form-section" *ngIf="step() === 'form'">
        <form [formGroup]="form" (ngSubmit)="startUpload()">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Título</mat-label>
            <input matInput formControlName="title">
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Descrição</mat-label>
            <textarea matInput formControlName="description" rows="3"></textarea>
          </mat-form-field>
          <div class="row-fields">
            <mat-form-field appearance="outline">
              <mat-label>Tipo</mat-label>
              <mat-select formControlName="type">
                <mat-option value="Movie">Filme</mat-option>
                <mat-option value="Series">Série</mat-option>
              </mat-select>
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Categoria</mat-label>
              <mat-select formControlName="categoryId">
                <mat-option *ngFor="let cat of categories()" [value]="cat.id">{{ cat.name }}</mat-option>
              </mat-select>
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Ano</mat-label>
              <input matInput type="number" formControlName="releaseYear">
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Classificação</mat-label>
              <mat-select formControlName="ageRating">
                <mat-option value="L">Livre</mat-option>
                <mat-option value="10">10</mat-option>
                <mat-option value="12">12</mat-option>
                <mat-option value="14">14</mat-option>
                <mat-option value="16">16</mat-option>
                <mat-option value="18">18</mat-option>
              </mat-select>
            </mat-form-field>
          </div>
          <div class="file-drop" (dragover)="$event.preventDefault()" (drop)="onDrop($event)" (click)="fileInput.click()">
            <input #fileInput type="file" accept="video/*" style="display:none" (change)="onFileSelect($event)">
            <mat-icon>cloud_upload</mat-icon>
            <p>{{ selectedFile() ? selectedFile()!.name : 'Arraste o vídeo aqui ou clique para selecionar' }}</p>
            <small *ngIf="selectedFile()">{{ (selectedFile()!.size / 1024 / 1024).toFixed(1) }} MB</small>
          </div>
          <button mat-raised-button type="submit" [disabled]="form.invalid || !selectedFile()" class="upload-btn">
            Iniciar Upload
          </button>
        </form>
      </div>

      <div class="progress-section" *ngIf="step() === 'uploading'">
        <h3>Enviando vídeo...</h3>
        <mat-progress-bar mode="determinate" [value]="uploadProgress()"></mat-progress-bar>
        <p>{{ uploadProgress() }}%</p>
      </div>

      <div class="progress-section" *ngIf="step() === 'transcoding'">
        <h3>{{ transcodingStatus() }}</h3>
        <mat-progress-bar mode="indeterminate" color="warn"></mat-progress-bar>
        <p class="status-msg">{{ transcodingMessage() }}</p>
      </div>

      <div class="done-section" *ngIf="step() === 'done'">
        <mat-icon class="done-icon">check_circle</mat-icon>
        <h3>Conteúdo publicado com sucesso!</h3>
        <button mat-raised-button (click)="reset()" class="upload-btn">Novo Upload</button>
      </div>

      <div class="error-section" *ngIf="step() === 'error'">
        <mat-icon class="error-icon">error</mat-icon>
        <h3>{{ errorMessage() }}</h3>
        <button mat-raised-button (click)="reset()">Tentar Novamente</button>
      </div>
    </div>
  `,
  styles: [`
    .upload-container { padding: 100px 40px 60px; background: #141414; min-height: 100vh; max-width: 800px; margin: 0 auto; }
    h1 { font-size: 2rem; font-weight: 900; margin-bottom: 32px; }
    .step-indicator { display: flex; gap: 8px; margin-bottom: 40px; }
    .step { display: flex; align-items: center; gap: 8px; padding: 8px 16px; border-radius: 4px; color: #666; font-size: 0.85rem; flex: 1; justify-content: center; border: 1px solid #333; }
    .step.active { color: #e50914; border-color: #e50914; font-weight: 700; }
    .step.done { color: #4caf50; border-color: #4caf50; }
    .full-width { width: 100%; margin-bottom: 16px; }
    .row-fields { display: flex; gap: 12px; flex-wrap: wrap; margin-bottom: 16px; }
    .row-fields mat-form-field { flex: 1; min-width: 150px; }
    .file-drop {
      border: 2px dashed #555; border-radius: 8px; padding: 40px; text-align: center;
      cursor: pointer; margin-bottom: 24px; transition: border-color 0.2s;
    }
    .file-drop:hover { border-color: #e50914; }
    .file-drop mat-icon { font-size: 48px; width: 48px; height: 48px; color: #555; margin-bottom: 12px; }
    .upload-btn { background: #e50914 !important; color: #fff !important; width: 100%; height: 48px; font-size: 1rem; font-weight: 700; }
    .progress-section { text-align: center; padding: 40px 0; }
    .progress-section h3 { margin-bottom: 24px; font-size: 1.2rem; }
    .progress-section p { margin-top: 12px; color: #aaa; }
    .status-msg { color: #e5e5e5; }
    .done-section, .error-section { text-align: center; padding: 60px 0; }
    .done-icon { font-size: 72px; width: 72px; height: 72px; color: #4caf50; margin-bottom: 16px; }
    .error-icon { font-size: 72px; width: 72px; height: 72px; color: #e50914; margin-bottom: 16px; }
    ::ng-deep .mat-mdc-form-field-outline { color: #555 !important; }
    ::ng-deep .mat-mdc-input-element { color: #fff !important; }
  `]
})
export class UploadComponent implements OnInit, OnDestroy {
  categories = signal<Category[]>([]);
  selectedFile = signal<File | null>(null);
  step = signal<UploadStep>('form');
  uploadProgress = signal(0);
  transcodingStatus = signal('Iniciando transcodificação...');
  transcodingMessage = signal('Aguarde, isso pode levar alguns minutos.');
  errorMessage = signal('');
  private hubConnection?: signalR.HubConnection;
  private createdContentId?: string;

  form = this.fb.group({
    title: ['', Validators.required],
    description: ['', Validators.required],
    type: ['Movie', Validators.required],
    categoryId: ['', Validators.required],
    releaseYear: [new Date().getFullYear(), Validators.required],
    ageRating: ['L', Validators.required]
  });

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private contentService: ContentService,
    private auth: AuthService
  ) {}

  ngOnInit() {
    this.contentService.getCategories().subscribe(cats => this.categories.set(cats));
  }

  get stepIndex(): () => number {
    return () => {
      const steps: UploadStep[] = ['form', 'uploading', 'transcoding', 'done'];
      return steps.indexOf(this.step());
    };
  }

  onFileSelect(e: Event) {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (file) this.selectedFile.set(file);
  }

  onDrop(e: DragEvent) {
    e.preventDefault();
    const file = e.dataTransfer?.files[0];
    if (file && file.type.startsWith('video/')) this.selectedFile.set(file);
  }

  async startUpload() {
    if (this.form.invalid || !this.selectedFile()) return;

    const formValue = this.form.value;
    const file = this.selectedFile()!;

    // Step 1: Create content
    this.contentService['http'].post<any>('/api/content', {
      title: formValue.title,
      description: formValue.description,
      type: formValue.type,
      categoryId: formValue.categoryId,
      releaseYear: formValue.releaseYear,
      ageRating: formValue.ageRating
    }).subscribe(content => {
      this.createdContentId = content.id;
      this.getPresignedAndUpload(content.id, file);
    });
  }

  private getPresignedAndUpload(contentId: string, file: File) {
    this.step.set('uploading');
    this.http.post<{ uploadUrl: string; objectKey: string }>('/api/upload/presigned', {
      fileName: file.name,
      contentType: file.type
    }).subscribe(({ uploadUrl, objectKey }) => {
      const xhr = new XMLHttpRequest();
      xhr.upload.onprogress = (e) => {
        if (e.lengthComputable) this.uploadProgress.set(Math.round(e.loaded / e.total * 100));
      };
      xhr.onload = () => {
        if (xhr.status === 200) {
          this.confirmAndTranscode(contentId, objectKey);
        } else {
          this.step.set('error');
          this.errorMessage.set('Erro no upload do arquivo.');
        }
      };
      xhr.onerror = () => { this.step.set('error'); this.errorMessage.set('Erro de rede no upload.'); };
      xhr.open('PUT', uploadUrl);
      xhr.setRequestHeader('Content-Type', file.type);
      xhr.send(file);
    });
  }

  private confirmAndTranscode(contentId: string, objectKey: string) {
    this.step.set('transcoding');
    this.connectSignalR(contentId);
    this.http.post('/api/upload/confirm', { contentId, objectKey }).subscribe();
  }

  private connectSignalR(contentId: string) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/transcoding', { accessTokenFactory: () => this.auth.token() ?? '' })
      .build();

    this.hubConnection.on('TranscodingProgress', (data: any) => {
      this.transcodingStatus.set(`Transcodificando... ${data.progress}%`);
      this.transcodingMessage.set(data.currentStep || '');
      if (data.status === 'completed') this.step.set('done');
      if (data.status === 'failed') { this.step.set('error'); this.errorMessage.set('Transcodificação falhou.'); }
    });

    this.hubConnection.start().then(() => {
      this.hubConnection!.invoke('JoinContentGroup', contentId);
    });
  }

  reset() {
    this.step.set('form');
    this.selectedFile.set(null);
    this.uploadProgress.set(0);
    this.form.reset({ type: 'Movie', ageRating: 'L', releaseYear: new Date().getFullYear() });
    this.hubConnection?.stop();
  }

  ngOnDestroy() {
    this.hubConnection?.stop();
  }
}
