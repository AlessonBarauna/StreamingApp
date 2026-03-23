import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent),
    canActivate: [authGuard]
  },
  {
    path: 'browse',
    loadComponent: () => import('./features/browse/browse.component').then(m => m.BrowseComponent),
    canActivate: [authGuard]
  },
  {
    path: 'watch/:contentId',
    loadComponent: () => import('./features/player/player.component').then(m => m.PlayerComponent),
    canActivate: [authGuard]
  },
  {
    path: 'details/:contentId',
    loadComponent: () => import('./features/details/details.component').then(m => m.DetailsComponent),
    canActivate: [authGuard]
  },
  {
    path: 'profile',
    loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/upload',
    loadComponent: () => import('./features/upload/upload.component').then(m => m.UploadComponent),
    canActivate: [adminGuard]
  },
  {
    path: 'admin/content',
    loadComponent: () => import('./features/admin/admin-content.component').then(m => m.AdminContentComponent),
    canActivate: [adminGuard]
  },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'auth/register',
    loadComponent: () => import('./features/auth/register.component').then(m => m.RegisterComponent)
  },
  { path: '**', redirectTo: '' }
];
