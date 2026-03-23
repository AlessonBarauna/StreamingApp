import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { ToastService, Toast } from '../../services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="toast-container">
      <div class="toast" *ngFor="let t of toast.toasts()" [class]="t.type" (click)="toast.dismiss(t.id)">
        <mat-icon class="toast-icon">{{ iconFor(t) }}</mat-icon>
        <span>{{ t.message }}</span>
        <mat-icon class="close-icon">close</mat-icon>
      </div>
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed; bottom: 24px; right: 24px;
      display: flex; flex-direction: column; gap: 12px;
      z-index: 9999; max-width: 360px;
    }
    .toast {
      display: flex; align-items: center; gap: 12px;
      padding: 14px 16px; border-radius: 6px;
      font-size: 0.9rem; cursor: pointer;
      animation: slideIn 0.2s ease-out;
      box-shadow: 0 4px 16px rgba(0,0,0,0.5);
    }
    .error   { background: #c0392b; color: #fff; }
    .warning { background: #e67e22; color: #fff; }
    .info    { background: #2980b9; color: #fff; }
    .toast-icon { font-size: 20px; width: 20px; height: 20px; flex-shrink: 0; }
    .close-icon { font-size: 18px; width: 18px; height: 18px; margin-left: auto; opacity: 0.7; flex-shrink: 0; }
    span { flex: 1; line-height: 1.4; }
    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to   { transform: translateX(0);   opacity: 1; }
    }
  `]
})
export class ToastComponent {
  constructor(public toast: ToastService) {}

  iconFor(t: Toast): string {
    const icons: Record<string, string> = { error: 'error', warning: 'warning', info: 'info' };
    return icons[t.type];
  }
}
