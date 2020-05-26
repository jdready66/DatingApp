import { Directive } from '@angular/core';
import { NG_ASYNC_VALIDATORS, AsyncValidator, AbstractControl, ValidatorFn, ValidationErrors, AsyncValidatorFn } from '@angular/forms';
import { Observable } from 'rxjs';
import { UserService } from '../_services/user.service';
import { map, debounceTime } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';

export function UsernameExistsValidator(authService: AuthService): AsyncValidatorFn {
  return (control: AbstractControl): Promise<ValidationErrors | null> | Observable<ValidationErrors | null> => {
    const interval = 1000;
    return authService.getUserByUsername(control.value).pipe(debounceTime(interval), map(user => {
      return (user) ? {usernameExistsValidator: true} : null;
    }));
  };
}

@Directive({
  // tslint:disable-next-line: directive-selector
  selector: '[usernameExistsValidator][ngModel],[usernameExistsValidator][formControl],[usernameExistsValidator][formControlName]',
  providers: [
    {provide: NG_ASYNC_VALIDATORS, useExisting: UsernameExistsValidatorDirective, multi: true}
]
})
export class UsernameExistsValidatorDirective implements AsyncValidator {
  constructor(private authService: AuthService) { }

  validate(control: AbstractControl): Promise<ValidationErrors | null> | Observable<ValidationErrors | null> {
    return UsernameExistsValidator(this.authService)(control);
  }
}
