import { Directive, forwardRef, Input } from '@angular/core';
import { AbstractControl, NG_VALIDATORS, Validator } from '@angular/forms';

@Directive({
  standalone: false,
  selector: '[requireDigit],[requireLowercase],[requireNonAlphanumeric],[requireUppercase],[requiredLength]',
  providers: [{ provide: NG_VALIDATORS, useExisting: forwardRef(() => PasswordComplexityValidator), multi: true }],
})
export class PasswordComplexityValidator implements Validator {
  @Input() requireDigit: boolean;
  @Input() requireUppercase: boolean;
  @Input() requireLowercase: boolean;
  @Input() requireNonAlphanumeric: boolean;
  @Input() requiredLength: number;

  validate(control: AbstractControl): { [key: string]: any } {
    const givenPassword = control.value;
    let validationResult = null;

    const requireDigit = this.requireDigit;
    if (requireDigit && givenPassword && !/\d/.test(givenPassword)) {
      validationResult = validationResult ?? {};
      validationResult.requireDigit = true;
    }

    const requireUppercase = this.requireUppercase;
    if (requireUppercase && givenPassword && !/[A-Z]/.test(givenPassword)) {
      validationResult = validationResult ?? {};
      validationResult.requireUppercase = true;
    }

    const requireLowercase = this.requireLowercase;
    if (requireLowercase && givenPassword && !/[a-z]/.test(givenPassword)) {
      validationResult = validationResult ?? {};
      validationResult.requireLowercase = true;
    }

    const requiredLength = this.requiredLength;
    if (requiredLength && givenPassword && givenPassword.length < requiredLength) {
      validationResult = validationResult ?? {};
      validationResult.requiredLength = true;
    }

    // use upperCaseLetters
    const requireNonAlphanumeric = this.requireNonAlphanumeric;
    if (requireNonAlphanumeric && givenPassword && /^[\da-zA-Z]+$/.test(givenPassword)) {
      validationResult = validationResult ?? {};
      validationResult.requireNonAlphanumeric = true;
    }

    return validationResult;
  }
}
