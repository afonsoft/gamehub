import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-admin-guide',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './admin-guide.component.html',
})
export class AdminGuideComponent {}
