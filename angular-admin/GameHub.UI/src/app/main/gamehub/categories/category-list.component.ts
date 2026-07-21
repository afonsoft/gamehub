import { Component, OnInit } from '@angular/core';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-category-list',
  templateUrl: './category-list.component.html',
})
export class CategoryListComponent implements OnInit {
  categories: any[] = [];

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.adminService.getCategories().subscribe(result => {
      this.categories = result?.items || [];
    });
  }

  delete(category: any): void {
    this.adminService.deleteCategory(category.id).subscribe(() => {
      this.categories = this.categories.filter(c => c.id !== category.id);
    });
  }
}
