import { Component, ElementRef, EventEmitter, Injector, Output, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import {
  ApplicationLanguageEditDto,
  ComboboxItemDto,
  CreateOrUpdateLanguageInput,
  LanguageServiceProxy,
} from '@shared/service-proxies/service-proxies';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  selector: 'createOrEditLanguageModal',
  templateUrl: './create-or-edit-language-modal.component.html',
})
export class CreateOrEditLanguageModalComponent extends AppComponentBase {
  @ViewChild('createOrEditModal', { static: true }) modal: ModalDirective;
  @ViewChild('languageCombobox', { static: true }) languageCombobox: ElementRef;
  @ViewChild('iconCombobox', { static: true }) iconCombobox: ElementRef;

  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

  active = false;
  saving = false;

  language: ApplicationLanguageEditDto = new ApplicationLanguageEditDto();

  languageNames: ComboboxItemDto[] = [];
  flags: ComboboxItemDto[] = [];

  constructor(
    injector: Injector,
    private readonly _languageService: LanguageServiceProxy,
  ) {
    super(injector);
  }

  show(languageId?: number): void {
    this.active = true;

    this._languageService.getLanguageForEdit(languageId).subscribe(result => {
      this.language = result.language;
      this.languageNames = result.languageNames;
      this.flags = result.flags;

      if (!languageId) {
        this.language.isEnabled = true;
      }

      this.modal.show();
    });
  }

  save(): void {
    const input = new CreateOrUpdateLanguageInput();
    input.language = this.language;

    this.saving = true;
    this._languageService.createOrUpdateLanguage(input).subscribe(() => {
      this._languageService
        .getAllLanguages()
        .pipe(finalize(() => (this.saving = false)))
        .subscribe(result => {
          eaf.localization.languages = result;
          eaf.event.trigger('app.languagesChanged');

          this.notify.success(this.l('SavedSuccessfully'));
          this.close();
          this.modalSave.emit(null);
        });
    });
  }

  close(): void {
    this.active = false;
    this.modal.hide();
  }
}
