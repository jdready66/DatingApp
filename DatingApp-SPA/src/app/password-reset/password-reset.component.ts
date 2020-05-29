import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { PasswordReset } from '../_models/PasswordReset';

@Component({
  selector: 'app-password-reset',
  templateUrl: './password-reset.component.html',
  styleUrls: ['./password-reset.component.css']
})
export class PasswordResetComponent implements OnInit {
  mode = 'form';
  email = '';
  token = '';
  resetPasswordForm: FormGroup;
  passwordReset: PasswordReset;
  errorList: any;
  errorText = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private fb: FormBuilder,
    private alertify: AlertifyService
  ) {}

  ngOnInit() {
    this.email = this.route.snapshot.params.email;
    this.token = this.route.snapshot.params.token;
    this.initializeResetPasswordForm();
  }

  initializeResetPasswordForm() {
    this.resetPasswordForm = this.fb.group(
      {
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(4),
            Validators.maxLength(8)
          ]
        ],
        confirmPassword: ['', Validators.required]
      },
      { validator: this.passwordMatchValidator }
    );
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password').value === g.get('confirmPassword').value
      ? null
      : { mismatch: true };
  }

  changePassword() {
    if (this.resetPasswordForm.valid) {
      this.passwordReset = Object.assign({}, this.resetPasswordForm.value);
      this.passwordReset.token = this.token;
      this.passwordReset.email = this.email;
      this.authService.resetPassword(this.passwordReset).subscribe(
        (data) => {
          this.mode = 'success';
        },
        (error) => {
          if (typeof error === 'object') {
            this.errorText = JSON.stringify(error);
            this.errorList = error;
          } else {
            this.errorText = error;
            this.errorList = [{ code: 'UNK', description: 'Unknown Error' }];
          }
          this.mode = 'failed';
        }
      );
    }
  }
}
