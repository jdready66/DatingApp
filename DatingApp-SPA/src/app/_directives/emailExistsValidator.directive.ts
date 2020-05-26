import { Directive } from '@angular/core';
import { NG_ASYNC_VALIDATORS, AsyncValidator, AbstractControl, ValidationErrors, AsyncValidatorFn } from '@angular/forms';
import { AuthService } from '../_services/auth.service';
import { Observable } from 'rxjs';
import { debounceTime, map } from 'rxjs/operators';

export function EmailExistsValidator(authService: AuthService): AsyncValidatorFn {
  return (control: AbstractControl): Promise<ValidationErrors | null> | Observable<ValidationErrors | null> => {
    const interval = 1000;
    return authService.getUserByEmail(control.value).pipe(debounceTime(interval), map(user => {
      return (user) ? {emailExistsValidator: true} : null;
    }));
  };
}

@Directive({
  // tslint:disable-next-line: directive-selector
  selector: '[emailExistsValidator][ngModel],[emailExistsValidator][formControl],[emailExistsValidator][formControlName]',
  providers: [
    {provide: NG_ASYNC_VALIDATORS, useExisting: EmailExistsValidatorDirective, multi: true}
  ]
})
export class EmailExistsValidatorDirective implements AsyncValidator {

  constructor(private authService: AuthService) { }

  validate(control: AbstractControl): Promise<ValidationErrors | null> | Observable<ValidationErrors | null> {
    return EmailExistsValidator(this.authService)(control);
  }

}
