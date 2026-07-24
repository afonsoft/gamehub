import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  GameHubAdminService,
  InspectorSessionDetail,
  InspectorSdkEvent,
  InspectorWarning,
} from '../shared/services/gamehub-admin.service';

@Component({
  selector: 'app-inspector-session',
  standalone: false,
  templateUrl: './inspector-session.component.html',
})
export class InspectorSessionComponent implements OnInit {
  session: InspectorSessionDetail | null = null;
  loading = false;
  validating = false;
  devicePreset = 'desktop';
  resolution = '1024x768';
  presets = [
    { label: 'Desktop', value: 'desktop', resolution: '1024x768' },
    { label: 'Mobile', value: 'mobile', resolution: '390x844' },
    { label: 'Tablet', value: 'tablet', resolution: '820x1180' },
  ];

  constructor(
    private readonly route: ActivatedRoute,
    private readonly adminService: GameHubAdminService,
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const sessionId = params.get('id');
      if (sessionId) {
        this.loadSession(sessionId);
      }
    });
  }

  loadSession(sessionId: string): void {
    this.loading = true;
    this.adminService.getInspectorSession(sessionId).subscribe({
      next: session => {
        this.session = session;
        this.devicePreset = session.devicePreset || 'desktop';
        this.resolution = session.resolution || '1024x768';
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  rerunValidation(): void {
    if (!this.session) return;
    this.validating = true;
    this.adminService.validateInspectorSession(this.session.id).subscribe({
      next: warnings => {
        if (this.session) {
          this.session.warnings = warnings;
        }
        this.validating = false;
      },
      error: () => {
        this.validating = false;
      },
    });
  }

  selectPreset(value: string): void {
    const preset = this.presets.find(p => p.value === value);
    if (preset) {
      this.devicePreset = value;
      this.resolution = preset.resolution;
    }
  }

  getResolutionStyle(): { width: string; height: string } | null {
    const parts = this.resolution?.split('x');
    if (!parts || parts.length !== 2) return null;
    return { width: `${parts[0]}px`, height: `${parts[1]}px` };
  }
}
