import { Component, OnInit } from '@angular/core';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-tag-edit',
  templateUrl: './tag-edit.component.html',
})
export class TagEditComponent implements OnInit {
  tag: any = {};

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
  }

  save(): void {
    this.adminService.createOrUpdateTag(this.tag).subscribe(() => {
    });
  }
}
