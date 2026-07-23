import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { I18nService, SupportedLanguage } from '../../../core/i18n/i18n.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-language-selector',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  template: `
    <div class="language-selector">
      <span>{{ 'language' | translate }}</span>
      <select [value]="currentLang" (change)="onChange($any($event).target.value)">
        <option value="pt-BR">Português</option>
        <option value="en-US">English</option>
      </select>
    </div>
  `,
  styles: [
    `.language-selector { display: flex; align-items: center; gap: 0.5rem; font-size: 0.875rem; }`,
    `select { padding: 0.25rem 0.5rem; border-radius: var(--gh-radius); border: 1px solid var(--gh-border); background: var(--gh-surface); }`,
  ],
})
export class LanguageSelectorComponent {
  private readonly i18n = inject(I18nService);
  currentLang = this.i18n.getCurrentLang();

  async onChange(value: SupportedLanguage): Promise<void> {
    await this.i18n.setLanguage(value);
    this.currentLang = value;
  }
}
