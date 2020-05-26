import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { User } from '../_models/user';

@Component({
  selector: 'app-confirm-email',
  templateUrl: './confirm-email.component.html',
  styleUrls: ['./confirm-email.component.css']
})
export class ConfirmEmailComponent implements OnInit {
  link = '';
  emailConfirmed = 'confirming';
  user: User = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private alertify: AlertifyService
  ) {}

  ngOnInit() {
    if (this.authService.currentUser) {
      this.user = this.authService.currentUser;
    }
    // tslint:disable-next-line: no-string-literal
    if (this.route.snapshot.params.link === 'unconfirmed') {
      if (this.authService.currentUser.emailConfirmed) {
        this.router.navigate(['/members']);
      }
      this.emailConfirmed = 'unconfirmed';
    } else {
      this.attemptConfirmation();
    }
  }

  attemptConfirmation() {
    console.log(this.route.snapshot.params.link);
    this.link = this.route.snapshot.params.link;
    this.authService.confirmEmail(this.link).subscribe(
      (data) => {
        this.emailConfirmed = 'confirmed';
      },
      (error) => {
        console.log(error);
        switch (error) {
          case 'Email already confirmed':
            this.emailConfirmed = 'already';
            break;

          default:
            this.emailConfirmed = 'failed';
            break;
        }
      }
    );
  }

  generateNewConfirmation() {
    this.alertify.confirm(
      'Have you checked you INBOX and JUNK folders?<br>'
      + 'It may be possible tht you received the email confirmation<br>'
      + 'request, but your email program determined that it looked<br>'
      + 'like "junk" and removed it from your Inbox.<br><br>'
      + '<span class="text-primary"><strong>I understand, please generate a new confirmation email.</strong></span>', () => {
        console.log('Send new confirmation here!');
        this.authService.resendConfirmationEmail().subscribe(() => {
          this.alertify.message('Confirmation email resent');
        }, error => {
          this.alertify.error(error);
        });
      });
  }
}
