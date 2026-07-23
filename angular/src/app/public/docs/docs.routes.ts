import { Routes } from '@angular/router';
import { DocsComponent } from './docs.component';
import { UserGuideComponent } from './user-guide/user-guide.component';
import { ApiGuideComponent } from './api-guide/api-guide.component';
import { AdminGuideComponent } from './admin-guide/admin-guide.component';
import { SdkGuideComponent } from './sdk-guide/sdk-guide.component';

export const docsRoutes: Routes = [
  {
    path: '',
    component: DocsComponent,
    children: [
      { path: '', redirectTo: 'user-guide', pathMatch: 'full' },
      { path: 'user-guide', component: UserGuideComponent },
      { path: 'api-guide', component: ApiGuideComponent },
      { path: 'admin-guide', component: AdminGuideComponent },
      { path: 'sdk-guide', component: SdkGuideComponent },
    ],
  },
];
