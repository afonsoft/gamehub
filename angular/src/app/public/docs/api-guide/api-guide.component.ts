import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-api-guide',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './api-guide.component.html',
})
export class ApiGuideComponent {}
