import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '../button/button.component';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, ButtonComponent, TranslatePipe],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.css',
})
export class ConfirmDialogComponent {
  isOpen = signal(false);
  titleKey = signal('common.confirmTitle');
  messageKey = signal('');

  private resolve?: (value: boolean) => void;

  open(titleKey: string, messageKey: string): Promise<boolean> {
    this.titleKey.set(titleKey);
    this.messageKey.set(messageKey);
    this.isOpen.set(true);
    return new Promise<boolean>(resolve => {
      this.resolve = resolve;
    });
  }

  confirm(): void {
    this.isOpen.set(false);
    this.resolve?.(true);
    this.resolve = undefined;
  }

  cancel(): void {
    this.isOpen.set(false);
    this.resolve?.(false);
    this.resolve = undefined;
  }
}
