import { ResolveFn } from '@angular/router';
import { inject } from '@angular/core';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

export const gameDetailResolver: ResolveFn<any> = (route) => {
  const adminService = inject(GameHubAdminService);
  const id = route.paramMap.get('id') || '';
  return adminService.getGameDetail(id);
};
