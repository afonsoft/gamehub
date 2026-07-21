import { Attribute, Directive, forwardRef } from '@angular/core';
import { AbstractControl, NG_VALIDATORS, Validator } from '@angular/forms';

//Got from: https://scotch.io/tutorials/how-to-implement-a-custom-validator-directive-confirm-password-in-angular-2

@Directive({
  standalone: false,
  selector: '[validateEqual][formControlName],[validateEqual][formControl],[validateEqual][ngModel]',
  providers: [{ provide: NG_VALIDATORS, useExisting: forwardRef(() => EqualValidator), multi: true }],
})
export class EqualValidator implements Validator {
  constructor(
    @Attribute('validateEqual') public validateEqual: string,
    @Attribute('reverse') public reverse: string,
  ) {}

  private get isReverse() {
    if (!this.reverse) {
      return false;
    }

    return this.reverse === 'true';
  }

  validate(control: AbstractControl): { [key: string]: any } {
    const pairControl = control.root.get(this.validateEqual);
    if (!pairControl) {
      return null;
    }

    const value = control.value;
    const pairValue = pairControl.value;

    if (!value && !pairValue) {
      return null;
    }

    return this.isReverse ? this.validateReverse(pairControl, value, pairValue) : this.validateForward(value, pairValue);
  }

  private validateReverse(pairControl: AbstractControl, value: any, pairValue: any): null {
    if (value === pairValue) {
      if (pairControl.errors) {
        delete pairControl.errors['validateEqual'];
      }

      if (!Object.keys(pairControl.errors).length) {
        pairControl.setErrors(null);
      }
    } else {
      pairControl.setErrors({
        validateEqual: true,
      });
    }

    return null;
  }

  private validateForward(value: any, pairValue: any): { [key: string]: any } {
    return value !== pairValue ? { validateEqual: true } : null;
  }
}
