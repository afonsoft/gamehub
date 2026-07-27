import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { GamesComponent } from './games/games.component';
import { GameDetailComponent } from './game-detail/game-detail.component';
import { SearchPageComponent } from './search-page/search-page.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { SelectTenantComponent } from './select-tenant/select-tenant.component';
import { CompanyComponent } from './company/company.component';
import { NotFoundComponent } from './not-found/not-found.component';
import { PlayerComponent } from './player/player.component';
import { guestGuard } from '../core/auth/guest.guard';

export const publicRoutes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'docs', loadChildren: () => import('./docs/docs.routes').then(m => m.docsRoutes) },
  { path: 'games', component: GamesComponent },
  { path: 'games/:slug', component: GameDetailComponent },
  { path: 'game/:slug', redirectTo: 'games/:slug', pathMatch: 'full' },
  { path: 'search', component: SearchPageComponent },
  { path: 'login', loadComponent: () => import('./login/login.component').then(m => m.LoginComponent), canActivate: [guestGuard] },
  { path: 'select-tenant', loadComponent: () => import('./select-tenant/select-tenant.component').then(m => m.SelectTenantComponent), canActivate: [guestGuard] },
  { path: 'register', loadComponent: () => import('./register/register.component').then(m => m.RegisterComponent), canActivate: [guestGuard] },
  { path: 'company/:tenancyName', loadComponent: () => import('./company/company.component').then(m => m.CompanyComponent) },
  { path: 'player', component: PlayerComponent },
  { path: '**', component: NotFoundComponent },
];
