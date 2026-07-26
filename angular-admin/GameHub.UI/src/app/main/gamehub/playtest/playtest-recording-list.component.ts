import { Component, OnInit } from '@angular/core';
import { LazyLoadEvent } from 'primeng/api';
import { GameHubAdminService, PagedPlaytestRecordings, PlaytestRecording } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'app-playtest-recording-list',
  templateUrl: './playtest-recording-list.component.html',
})
export class PlaytestRecordingListComponent implements OnInit {
  recordings: PlaytestRecording[] = [];
  selectedRecording: PlaytestRecording | null = null;
  totalCount = 0;
  loading = false;
  skipCount = 0;
  maxResultCount = 25;
  gameIdFilter = '';
  deviceTypeFilter = '';
  notes = '';

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadRecordings();
  }

  loadRecordings(event?: LazyLoadEvent): void {
    this.skipCount = event?.first ?? 0;
    this.maxResultCount = event?.rows ?? this.maxResultCount;
    this.loading = true;
    this.adminService
      .getPlaytestRecordings(this.skipCount, this.maxResultCount, this.gameIdFilter || undefined, this.deviceTypeFilter || undefined)
      .subscribe({
        next: (result: PagedPlaytestRecordings) => {
          this.recordings = result.items ?? [];
          this.totalCount = result.totalCount ?? 0;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        },
      });
  }

  onSearch(): void {
    this.skipCount = 0;
    this.loadRecordings();
  }

  selectRecording(recording: PlaytestRecording): void {
    this.selectedRecording = recording;
    this.notes = recording.notes ?? '';
  }

  closeDetail(): void {
    this.selectedRecording = null;
  }

  saveNotes(): void {
    if (!this.selectedRecording) {
      return;
    }

    this.adminService.addPlaytestRecordingNotes(this.selectedRecording.id, this.notes).subscribe({
      next: (updated: PlaytestRecording) => {
        this.selectedRecording = updated;
        const index = this.recordings.findIndex(r => r.id === updated.id);
        if (index >= 0) {
          this.recordings[index] = updated;
        }
      },
    });
  }

  formatDuration(seconds: number): string {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, '0')}`;
  }
}
