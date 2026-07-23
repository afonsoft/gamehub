import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-user-guide',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './user-guide.component.html',
})
export class UserGuideComponent {}
