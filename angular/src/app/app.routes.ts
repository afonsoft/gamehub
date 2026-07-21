import { Routes } from '@angular/router';
import { HomeComponent } from './public/home/home.component';
import { GamesComponent } from './public/games/games.component';
import { GameDetailComponent } from './public/game-detail/game-detail.component';
import { GameFrameComponent } from './player/game-frame/game-frame.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'games', component: GamesComponent },
  { path: 'game/:slug', component: GameDetailComponent },
  { path: 'play/:slug', component: GameFrameComponent },
  { path: '**', redirectTo: '', pathMatch: 'full' }
];
