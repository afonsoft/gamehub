import { Component, OnInit } from '@angular/core';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-user-list',
  templateUrl: './user-list.component.html',
})
export class UserListComponent implements OnInit {
  users: any[] = [];
  totalCount = 0;
  loading = false;
  skipCount = 0;
  maxResultCount = 10;

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.adminService.getUsers(this.skipCount, this.maxResultCount).subscribe({
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

  previousPage(): void {
    if (this.skipCount >= this.maxResultCount) {
      this.skipCount -= this.maxResultCount;
      this.loadUsers();
    }
  }

  nextPage(): void {
    if (this.skipCount + this.maxResultCount < this.totalCount) {
      this.skipCount += this.maxResultCount;
      this.loadUsers();
    }
  }
}
