import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  GameHubAdminService,
  InspectorChecklistAnswer,
  InspectorChecklistCompletion,
  InspectorSessionDetail,
  InspectorSdkEvent,
  InspectorWarning,
} from '../shared/services/gamehub-admin.service';

interface ChecklistQuestion {
  id: string;
  label: string;
}

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
  completion: InspectorChecklistCompletion | null = null;

  presets = [
    { label: 'Desktop 1024x768', value: 'desktop', resolution: '1024x768' },
    { label: 'Poki 640x360', value: 'poki640', resolution: '640x360' },
    { label: 'Poki 836x470', value: 'poki836', resolution: '836x470' },
    { label: 'Poki 1031x580', value: 'poki1031', resolution: '1031x580' },
    { label: 'Portrait 360x640', value: 'portrait360', resolution: '360x640' },
    { label: 'Landscape 580x1031', value: 'landscape580', resolution: '580x1031' },
    { label: 'Mobile', value: 'mobile', resolution: '390x844' },
    { label: 'Tablet', value: 'tablet', resolution: '820x1180' },
  ];

  checklistQuestions: ChecklistQuestion[] = [
    { id: 'indexHtml', label: 'index.html present at root' },
    { id: 'viewport', label: 'Responsive viewport / scaling' },
    { id: 'loadingTime', label: 'First load under 5s' },
    { id: 'eventSequence', label: 'SDK event sequence correct' },
    { id: 'muteUnmute', label: 'Audio mute/unmute around ads' },
    { id: 'adBreakFlow', label: 'Ad break flow works' },
    { id: 'externalRequests', label: 'No unwanted external requests' },
    { id: 'cleanBuild', label: 'Build is clean (no debug artifacts)' },
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
        this.loadCompletion(sessionId);
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  loadCompletion(sessionId: string): void {
    this.adminService.getInspectorChecklistCompletion(sessionId).subscribe({
      next: result => {
        this.completion = result;
      },
      error: () => {
        this.completion = null;
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

  getPreviewUrl(): string {
    if (!this.session) return '';
    return `/play/${this.session.gameId}?inspector=1&inspectorSession=${this.session.id}`;
  }

  getAnswer(questionId: string): string {
    return this.session?.checklistAnswers?.find(a => a.questionId === questionId)?.answer ?? '';
  }

  saveAnswer(questionId: string, value: string): void {
    if (!this.session) return;
    this.adminService.saveInspectorChecklistAnswer(this.session.id, questionId, value).subscribe({
      next: () => {
        this.updateLocalAnswer(questionId, value);
        this.loadCompletion(this.session!.id);
      },
      error: () => {},
    });
  }

  private updateLocalAnswer(questionId: string, value: string): void {
    if (!this.session) return;
    const answers = this.session.checklistAnswers ?? [];
    const existing = answers.find(a => a.questionId === questionId);
    if (existing) {
      existing.answer = value;
    } else {
      answers.push({
        id: '',
        sessionId: this.session.id,
        questionId,
        answer: value,
        updatedAt: new Date().toISOString(),
      });
    }
    this.session.checklistAnswers = answers;
  }
}
