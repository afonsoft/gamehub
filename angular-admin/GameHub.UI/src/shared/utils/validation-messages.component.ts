import { Component, Input } from '@angular/core';
import { AppLocalizationService } from '@app/shared/common/localization/app-localization.service';


class ErrorDef {
  error: string;
  localizationKey: string;
  errorProperty: string;
}

@Component({
  standalone: false,
  selector: '<validation-messages>',
  template: `<div class="has-danger" *ngIf="formCtrl.invalid && (formCtrl.dirty || formCtrl.touched)">
    <div *ngFor="let errorDef of errorDefsInternal">
      <div *ngIf="getErrorDefinitionIsInValid(errorDef)" class="form-control-feedback">
        {{ getErrorDefinitionMessage(errorDef) }}
      </div>
    </div>
  </div>`,
})
export class ValidationMessagesComponent {
  _errorDefs: ErrorDef[] = [];

  @Input() formCtrl;
  @Input() set errorDefs(value: ErrorDef[]) {
    this._errorDefs = value;
  }

  readonly standartErrorDefs: ErrorDef[] = [
    { error: 'required', localizationKey: 'ThisFieldIsRequired' } as ErrorDef,
    { error: 'minlength', localizationKey: 'PleaseEnterAtLeastNCharacter', errorProperty: 'requiredLength' } as ErrorDef,
    { error: 'maxlength', localizationKey: 'PleaseEnterNoMoreThanNCharacter', errorProperty: 'requiredLength' } as ErrorDef,
    { error: 'email', localizationKey: 'InvalidEmailAddress' } as ErrorDef,
    { error: 'pattern', localizationKey: 'InvalidPattern', errorProperty: 'requiredPattern' } as ErrorDef,
  ];

  get errorDefsInternal(): ErrorDef[] {
    const standarts = this.standartErrorDefs.filter(ed => !this._errorDefs.some(edC => edC.error === ed.error));
    const all = <ErrorDef[]>standarts.concat(this._errorDefs);

    return all;
  }

  constructor(private readonly appLocalizationService: AppLocalizationService) {}

  getErrorDefinitionIsInValid(errorDef: ErrorDef): boolean {
    return !!this.formCtrl.errors[errorDef.error];
  }

  getErrorDefinitionMessage(errorDef: ErrorDef): string {
    const errorRequirement = this.formCtrl.errors[errorDef.error][errorDef.errorProperty];
    return errorRequirement
      ? this.appLocalizationService.l(errorDef.localizationKey, errorRequirement)
      : this.appLocalizationService.l(errorDef.localizationKey);
  }
}
