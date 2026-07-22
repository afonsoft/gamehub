import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { DeveloperService, DeveloperProfile, CreateOrUpdateProfileInput } from '../../core/services/developer.service';

@Component({
  selector: 'app-developer-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class DeveloperProfileComponent implements OnInit {
  profile: CreateOrUpdateProfileInput = {
    displayName: '',
    legalName: '',
    websiteUrl: '',
    supportEmail: '',
  };
  loading = false;
  saving = false;
  error = '';
  saved = false;

  private readonly developerService = inject(DeveloperService);

  ngOnInit(): void {
    this.loading = true;
    this.developerService.getProfile().subscribe({
      next: p => {
        this.loading = false;
        if (p) {
          this.profile = {
            displayName: p.displayName,
            legalName: p.legalName ?? '',
            websiteUrl: p.websiteUrl ?? '',
            supportEmail: p.supportEmail ?? '',
          };
        }
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  save(): void {
    this.error = '';
    this.saved = false;
    if (!this.profile.displayName) {
      this.error = 'Display name is required.';
      return;
    }
    this.saving = true;
    this.developerService.createOrUpdateProfile(this.profile).subscribe({
      next: () => {
        this.saving = false;
        this.saved = true;
      },
      error: () => {
        this.saving = false;
        this.error = 'Unable to save profile. Please try again.';
      },
    });
  }
}
