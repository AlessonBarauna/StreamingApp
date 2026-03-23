import { Component, OnInit, OnDestroy, ElementRef, ViewChild, signal, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSliderModule } from '@angular/material/slider';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { ContentService, Content, Progress } from '../../shared/services/content.service';
import { debounceTime, Subject } from 'rxjs';

declare var videojs: any;

@Component({
  selector: 'app-player',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatSliderModule, MatSelectModule, FormsModule],
  template: `
    <div class="player-container" (mousemove)="showControls()" (click)="togglePlay()" #playerContainer>
      <video #videoEl class="video-js vjs-big-play-centered" controls preload="auto" style="width:100%;height:100vh"></video>
      <div class="back-btn" (click)="goBack($event)">
        <mat-icon>arrow_back</mat-icon>
        <span>{{ content()?.title }}</span>
      </div>
      <div class="loading-overlay" *ngIf="loading()">
        <div class="spinner"></div>
      </div>
    </div>
  `,
  styles: [`
    .player-container {
      background: #000; min-height: 100vh; position: relative; cursor: pointer;
    }
    .back-btn {
      position: absolute; top: 20px; left: 20px; z-index: 10;
      display: flex; align-items: center; gap: 8px; color: #fff;
      background: rgba(0,0,0,0.6); padding: 8px 16px; border-radius: 4px;
      cursor: pointer; font-size: 1rem; font-weight: 600;
    }
    .loading-overlay {
      position: absolute; inset: 0; display: flex; align-items: center;
      justify-content: center; background: rgba(0,0,0,0.7); z-index: 5;
    }
    .spinner {
      width: 48px; height: 48px; border: 4px solid rgba(255,255,255,0.3);
      border-top-color: #e50914; border-radius: 50%; animation: spin 1s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    ::ng-deep .vjs-control-bar { background: linear-gradient(transparent, rgba(0,0,0,0.8)); }
    ::ng-deep .vjs-play-progress { background: #e50914; }
  `]
})
export class PlayerComponent implements OnInit, OnDestroy {
  @ViewChild('videoEl') videoEl!: ElementRef<HTMLVideoElement>;

  content = signal<Content | null>(null);
  loading = signal(true);

  private player: any;
  private contentId!: string;
  private progressSave$ = new Subject<{ seconds: number; total: number }>();
  private progressSub: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private contentService: ContentService
  ) {}

  ngOnInit() {
    this.contentId = this.route.snapshot.params['contentId'];
    this.contentService.getById(this.contentId).subscribe(content => {
      this.content.set(content);
      this.loading.set(false);
      setTimeout(() => this.initPlayer(content), 100);
    });

    this.progressSub = this.progressSave$
      .pipe(debounceTime(10000))
      .subscribe(({ seconds, total }) => {
        this.contentService.saveProgress(this.contentId, seconds, total).subscribe();
      });
  }

  private initPlayer(content: Content) {
    if (typeof videojs === 'undefined') {
      this.loadVideoJs(content);
      return;
    }
    this.setupPlayer(content);
  }

  private loadVideoJs(content: Content) {
    const script = document.createElement('script');
    script.src = 'https://vjs.zencdn.net/8.6.0/video.min.js';
    script.onload = () => this.setupPlayer(content);
    document.head.appendChild(script);
    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = 'https://vjs.zencdn.net/8.6.0/video-js.css';
    document.head.appendChild(link);
  }

  private setupPlayer(content: Content) {
    const manifestUrl = content.hlsManifestUrl
      ? `/api/stream/${content.id}/manifest`
      : '';

    this.player = (window as any)['videojs'](this.videoEl.nativeElement, {
      controls: true,
      autoplay: true,
      preload: 'auto',
      fluid: false,
      responsive: false,
      sources: manifestUrl ? [{ src: manifestUrl, type: 'application/x-mpegURL' }] : []
    });

    this.contentService.getProgress(this.contentId).subscribe(prog => {
      if (prog.progressSeconds > 0 && prog.percentComplete > 5 && prog.percentComplete < 95) {
        this.player.currentTime(prog.progressSeconds);
      }
    });

    this.player.on('timeupdate', () => {
      const current = Math.floor(this.player.currentTime());
      const total = Math.floor(this.player.duration() || 0);
      if (total > 0) this.progressSave$.next({ seconds: current, total });
    });

    document.addEventListener('keydown', this.handleKeydown);
  }

  private handleKeydown = (e: KeyboardEvent) => {
    if (!this.player) return;
    if (e.code === 'Space') { e.preventDefault(); this.togglePlay(); }
    if (e.code === 'ArrowRight') this.player.currentTime(this.player.currentTime() + 10);
    if (e.code === 'ArrowLeft') this.player.currentTime(this.player.currentTime() - 10);
    if (e.code === 'KeyF') this.player.isFullscreen() ? this.player.exitFullscreen() : this.player.requestFullscreen();
    if (e.code === 'KeyM') this.player.muted(!this.player.muted());
  };

  togglePlay() {
    if (!this.player) return;
    this.player.paused() ? this.player.play() : this.player.pause();
  }

  showControls() {}

  goBack(e: Event) {
    e.stopPropagation();
    this.router.navigate(['/details', this.contentId]);
  }

  ngOnDestroy() {
    if (this.player) this.player.dispose();
    this.progressSub?.unsubscribe();
    document.removeEventListener('keydown', this.handleKeydown);
  }
}
