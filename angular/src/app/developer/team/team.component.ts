import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { DeveloperService, DeveloperTeam } from '../../core/services/developer.service';

@Component({
  selector: 'app-developer-team',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './team.component.html',
  styleUrl: './team.component.css',
})
export class DeveloperTeamComponent implements OnInit {
  team: DeveloperTeam = {
    name: '',
    primaryContactEmail: '',
    country: '',
  };
  loading = false;
  saving = false;
  error = '';
  saved = false;

  private readonly developerService = inject(DeveloperService);

  ngOnInit(): void {
    this.loading = true;
    this.developerService.getTeamGeneralSettings().subscribe({
      next: team => {
        this.loading = false;
        if (team) {
          this.team = team;
        }
      },
      error: () => {
        this.loading = false;
        this.error = 'Unable to load team settings.';
      },
    });
  }

  save(): void {
    this.error = '';
    this.saved = false;

    if (!this.team.name || !this.team.primaryContactEmail) {
      this.error = 'Name and email are required.';
      return;
    }

    this.saving = true;
    this.developerService.updateTeamGeneralSettings(this.team).subscribe({
      next: team => {
        this.saving = false;
        this.saved = true;
        if (team) {
          this.team = team;
        }
      },
      error: () => {
        this.saving = false;
        this.error = 'Unable to save team settings. Please try again.';
      },
    });
  }
}
