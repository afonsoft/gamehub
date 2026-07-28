import { Component, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-user-list',
  templateUrl: './user-list.component.html',
  animations: [appModuleAnimation()],
})
export class UserListComponent implements OnInit {
  users: any[] = [];
  totalCount = 0;
  loading = false;

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(event?: any): void {
    const skipCount = event?.first || 0;
    const maxResultCount = event?.rows || 10;
    this.loading = true;
    this.adminService.getUsers(skipCount, maxResultCount).subscribe({
      next: result => {
        this.users = result?.items || [];
        this.totalCount = result?.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.users = [];
        this.totalCount = 0;
        this.loading = false;
      },
    });
  }
}
