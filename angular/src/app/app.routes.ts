import { Routes } from '@angular/router';
import { HomeComponent } from './public/home/home.component';
import { GamesComponent } from './public/games/games.component';
import { GameDetailComponent } from './public/game-detail/game-detail.component';
import { GameFrameComponent } from './player/game-frame/game-frame.component';
import { SearchPageComponent } from './public/search-page/search-page.component';
import { NotFoundComponent } from './public/not-found/not-found.component';
import { authGuard } from './core/auth/auth.guard';
import { developerGuard } from './core/auth/developer.guard';
import { guestGuard } from './core/auth/guest.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'games', component: GamesComponent },
  { path: 'games/:slug', component: GameDetailComponent },
  { path: 'game/:slug', redirectTo: 'games/:slug', pathMatch: 'full' },
  { path: 'search', component: SearchPageComponent },
  { path: 'play/:slug', component: GameFrameComponent },
  {
    path: 'leaderboard/:gameId',
    loadComponent: () => import('./player/leaderboard/leaderboard.component').then(m => m.LeaderboardComponent),
  },
  {
    path: 'login',
    loadComponent: () => import('./public/login/login.component').then(m => m.LoginComponent),
    canActivate: [guestGuard],
  },
  {
    path: 'register',
    loadComponent: () => import('./public/register/register.component').then(m => m.RegisterComponent),
    canActivate: [guestGuard],
  },
  {
    path: 'developer',
    loadComponent: () => import('./developer/dashboard/dashboard.component').then(m => m.DeveloperDashboardComponent),
    canActivate: [authGuard, developerGuard],
  },
  {
    path: 'developer/games',
    loadComponent: () => import('./developer/games/games.component').then(m => m.DeveloperGamesComponent),
    canActivate: [authGuard, developerGuard],
  },
  {
    path: 'developer/games/create',
    loadComponent: () => import('./developer/game-create/game-create.component').then(m => m.GameCreateComponent),
    canActivate: [authGuard, developerGuard],
  },
  {
    path: 'developer/games/:id/edit',
    loadComponent: () => import('./developer/game-edit/game-edit.component').then(m => m.GameEditComponent),
    canActivate: [authGuard, developerGuard],
  },
  {
    path: 'developer/games/:id/builds',
    loadComponent: () => import('./developer/builds/builds.component').then(m => m.DeveloperBuildsComponent),
    canActivate: [authGuard, developerGuard],
  },
  {
    path: 'developer/profile',
    loadComponent: () => import('./developer/profile/profile.component').then(m => m.DeveloperProfileComponent),
    canActivate: [authGuard, developerGuard],
  },
  { path: '**', component: NotFoundComponent },
];
