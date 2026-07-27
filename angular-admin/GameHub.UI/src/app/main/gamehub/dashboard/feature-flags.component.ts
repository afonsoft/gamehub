import { Component, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-feature-flags',
  templateUrl: './feature-flags.component.html',
  animations: [appModuleAnimation()],
})
export class FeatureFlagsComponent implements OnInit {
  flags: any[] = [];

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadFlags();
  }

  loadFlags(): void {
    this.adminService.getFeatureFlags().subscribe(result => {
      this.flags = result?.items || [];
    });
  }

  toggle(flag: any): void {
    this.adminService.toggleFeatureFlag(flag.id, !flag.isEnabled).subscribe(() => {
      flag.isEnabled = !flag.isEnabled;
    });
  }
}
