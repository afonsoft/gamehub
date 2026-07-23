import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-tag-edit',
  templateUrl: './tag-edit.component.html',
})
export class TagEditComponent implements OnInit {
  tag: any = {};
  saving = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly adminService: GameHubAdminService,
  ) {}

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      if (data['tag']) {
        this.tag = { ...data['tag'] };
      }
    });
  }

  onNameChange(): void {
    if (!this.tag.id && this.tag.name) {
      this.tag.slug = this.toSlug(this.tag.name);
    }
  }

  save(): void {
    this.saving = true;
    this.adminService.createOrUpdateTag(this.tag).subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/app/main/gamehub/tags']);
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
