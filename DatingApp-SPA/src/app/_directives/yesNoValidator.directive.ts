import { Directive } from '@angular/core';
import {
  NG_VALIDATORS,
  Validator,
  AbstractControl,
  ValidationErrors
} from '@angular/forms';

export function YesNoValidator(control: AbstractControl): ValidationErrors | null {
  const val = control.value;
  if (!val || val.toLowerCase() === 'yes') {
    return null; // valid
  }

  // invalid - validation errors display
  return { yesNoValidator: 'You chose no, no, no!' };
}


@Directive({
  // tslint:disable-next-line: directive-selector
  selector: '[yesNoValidator]',
  providers: [
    { provide: NG_VALIDATORS, useExisting: YesNoValidatorDirective, multi: true }
  ]
})
export class YesNoValidatorDirective implements Validator {
  constructor() {}
  validate(control: AbstractControl): ValidationErrors | null {
    return YesNoValidator(control);
  }
}
