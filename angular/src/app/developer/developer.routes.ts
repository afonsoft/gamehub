import { Routes } from '@angular/router';
import { authGuard } from '../core/auth/auth.guard';
import { developerGuard } from '../core/auth/developer.guard';
import { DeveloperDashboardComponent } from './dashboard/dashboard.component';

export const developerRoutes: Routes = [
  { path: '', component: DeveloperDashboardComponent },
  {
    path: 'games',
    loadComponent: () => import('./games/games.component').then(m => m.DeveloperGamesComponent),
  },
  {
    path: 'games/create',
    loadComponent: () => import('./game-create/game-create.component').then(m => m.GameCreateComponent),
  },
  {
    path: 'games/:id/edit',
    loadComponent: () => import('./game-edit/game-edit.component').then(m => m.GameEditComponent),
  },
  {
    path: 'games/:id/builds',
    loadComponent: () => import('./builds/builds.component').then(m => m.DeveloperBuildsComponent),
  },
  {
    path: 'profile',
    loadComponent: () => import('./profile/profile.component').then(m => m.DeveloperProfileComponent),
  },
];

export const developerRootRoute = {
  path: 'developer',
  loadChildren: () => import('./developer.routes').then(m => m.developerRoutes),
  canActivate: [authGuard, developerGuard],
};
