import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/components/navbar/navbar.component';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, CommonModule],
  template: `
    <app-navbar *ngIf="auth.isLoggedIn()"></app-navbar>
    <router-outlet></router-outlet>
  `,
  styles: [`
    :host { display: block; min-height: 100vh; background: #141414; }
  `]
})
export class AppComponent {
  constructor(public auth: AuthService) {}
}
