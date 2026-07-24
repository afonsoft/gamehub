import { Routes } from '@angular/router';
import { GameFrameComponent } from './player/game-frame/game-frame.component';
import { NotFoundComponent } from './public/not-found/not-found.component';

export const routes: Routes = [
  {
    path: 'play/:slug',
    component: GameFrameComponent,
  },
  {
    path: 'preview/:slug/:version',
    component: GameFrameComponent,
  },
  {
    path: 'leaderboard/:gameId',
    loadComponent: () => import('./player/leaderboard/leaderboard.component').then(m => m.LeaderboardComponent),
  },
  {
    path: 'developer',
    loadChildren: () => import('./developer/developer.routes').then(m => m.developerRoutes),
  },
  {
    path: '',
    loadChildren: () => import('./public/public.routes').then(m => m.publicRoutes),
  },
  { path: '**', component: NotFoundComponent },
];
