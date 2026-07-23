import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-sdk-guide',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './sdk-guide.component.html',
})
export class SdkGuideComponent {}
