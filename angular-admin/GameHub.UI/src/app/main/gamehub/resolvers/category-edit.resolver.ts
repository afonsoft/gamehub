import { ResolveFn } from '@angular/router';
import { inject } from '@angular/core';
import { of } from 'rxjs';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

export const categoryEditResolver: ResolveFn<any> = (route) => {
  const adminService = inject(GameHubAdminService);
  const id = route.paramMap.get('id');
  if (!id) {
    return of({ isActive: true });
  }
  return adminService.getCategoryById(id);
};
