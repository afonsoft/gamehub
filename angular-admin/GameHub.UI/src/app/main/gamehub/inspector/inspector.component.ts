import { Component, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Router } from '@angular/router';
import {
  GameHubAdminService,
  ValidationReport,
  InspectorSession,
} from '../shared/services/gamehub-admin.service';

@Component({
  selector: 'app-inspector',
  standalone: false,
  templateUrl: './inspector.component.html',
  animations: [appModuleAnimation()],
})
export class InspectorComponent implements OnInit {
  reports: ValidationReport[] = [];
  sessions: InspectorSession[] = [];
  loading = false;
  selectedGameId: string = '';

  constructor(
    private readonly adminService: GameHubAdminService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.loading = true;
    this.adminService.getValidationReports().subscribe({
      next: (result: ValidationReport[]) => {
        this.reports = result ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  loadSessions(): void {
    if (!this.selectedGameId) return;
    this.loading = true;
    this.adminService.getInspectorSessions(this.selectedGameId).subscribe({
      next: (result: InspectorSession[]) => {
        this.sessions = result ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  startSession(gameId: string): void {
    this.adminService
      .startInspectorSession({ gameId, devicePreset: 'desktop', resolution: '1024x768' })
      .subscribe(session => {
        this.router.navigate(['/app/main/gamehub/inspector/session', session.id]);
      });
  }

  openSession(sessionId: string): void {
    this.router.navigate(['/app/main/gamehub/inspector/session', sessionId]);
  }
}
