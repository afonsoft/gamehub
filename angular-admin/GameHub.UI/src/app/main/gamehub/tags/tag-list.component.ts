import { Component, OnInit } from '@angular/core';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-tag-list',
  templateUrl: './tag-list.component.html',
})
export class TagListComponent implements OnInit {
  tags: any[] = [];

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadTags();
  }

  loadTags(): void {
    this.adminService.getTags().subscribe(result => {
      this.tags = result?.items || [];
    });
  }

  delete(tag: any): void {
    this.adminService.deleteTag(tag.id).subscribe(() => {
      this.tags = this.tags.filter(t => t.id !== tag.id);
    });
  }
}
