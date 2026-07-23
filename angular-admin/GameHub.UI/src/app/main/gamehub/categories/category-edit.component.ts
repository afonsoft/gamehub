import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-category-edit',
  templateUrl: './category-edit.component.html',
})
export class CategoryEditComponent implements OnInit {
  category: any = { isActive: true };
  saving = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly adminService: GameHubAdminService,
  ) {}

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      if (data['category']) {
        this.category = { ...data['category'] };
      }
    });
  }

  onNameChange(): void {
    if (!this.category.id && this.category.name) {
      this.category.slug = this.toSlug(this.category.name);
    }
  }

  save(): void {
    this.saving = true;
    this.adminService.createOrUpdateCategory(this.category).subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/app/main/gamehub/categories']);
      },
      error: () => {
        this.saving = false;
      },
    });
  }

  private toSlug(value: string): string {
    return value
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }
}
