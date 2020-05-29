import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  photoUrl: string;

  constructor(public authService: AuthService, private alertify: AlertifyService, private router: Router) { }

  ngOnInit() {
    this.authService.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl);
  }

  login() {
    this.authService.login(this.model).subscribe(next => {
      this.model.username = '';
      this.model.password = '';
      this.alertify.success('Logged in successfully');
    }, error => {
      this.alertify.error(error);
    }, () => {
      if (this.authService.currentUser.emailConfirmed) {
        this.router.navigate(['/members']);
      } else {
        this.router.navigate(['/confirmEmail/unconfirmed']);
      }
    });
  }

  loggedIn() {
    return this.authService.loggedIn();
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.authService.decodedToken = null;
    this.authService.currentUser = null;
    this.alertify.message('logged out');
    this.router.navigate(['/home']);
  }

  confirmPasswordReset() {
    this.alertify.prompt('Forgot Password?',
    'Enter email address associated with your account:',
    'Email Address',
    (value) => {
      this.alertify.message('You entered: ' + value);
      this.authService.sendResetPasswordLink(value).subscribe(data => {
        // navigate to password reset landing page
        this.alertify.success('Password Reset EMail Sent');
      }, error => {
        this.alertify.error(error);
      });
    },
    () => {

    });
  }
}
