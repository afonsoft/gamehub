import { Component, OnInit } from '@angular/core';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-category-edit',
  templateUrl: './category-edit.component.html',
})
export class CategoryEditComponent implements OnInit {
  category: any = { isActive: true };

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
  }

  save(): void {
    this.adminService.createOrUpdateCategory(this.category).subscribe(() => {
    });
  }
}
