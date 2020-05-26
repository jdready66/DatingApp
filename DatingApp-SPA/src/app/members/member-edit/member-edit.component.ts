import { Component, OnInit, ViewChild, HostListener } from '@angular/core';
import { User } from 'src/app/_models/user';
import { ActivatedRoute, Router } from '@angular/router';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { NgForm, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { UserService } from 'src/app/_services/user.service';
import { AuthService } from 'src/app/_services/auth.service';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker/public_api';
import { NgxGalleryThumbnailsComponent } from 'ngx-gallery';
import { EmailExistsValidator } from 'src/app/_directives/emailExistsValidator.directive';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  user: User;
  updateUser: User;
  photoUrl: string;
  editForm: FormGroup;
  bsConfig: Partial<BsDatepickerConfig>;

  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private alertify: AlertifyService,
    private userService: UserService,
    private authService: AuthService,
    private fb: FormBuilder
  ) {}

  ngOnInit() {
    this.route.data.subscribe((data) => {
      // tslint:disable-next-line: no-string-literal
      this.user = data['user'];
    });

    this.authService.currentPhotoUrl.subscribe(
      (photoUrl) => (this.photoUrl = photoUrl)
    );

    this.bsConfig = {
      containerClass: 'theme-red'
    };

    this.createEditForm();
  }

  createEditForm() {
    this.editForm = this.fb.group({
      knownAs: [this.user.knownAs, Validators.required],
      email: [this.user.email, [Validators.required, Validators.email], EmailExistsValidator(this.authService)],
      gender: [this.user.gender],
      dateOfBirth: [this.user.dateOfBirth, Validators.required],
      introduction: [this.user.introduction ? this.user.introduction : ''],
      lookingFor: [this.user.lookingFor ? this.user.lookingFor : ''],
      interests: [this.user.interests ? this.user.interests : ''],
      city: [this.user.city, Validators.required],
      country: [this.user.country, Validators.required]
    });
  }

  saveEditForm() {
    if (this.editForm.valid) {
      this.updateUser = Object.assign({}, this.editForm.value);

      if (this.user.email !== this.updateUser.email) {
        this.updateUser.emailConfirmed = false;
        console.log('email was changed!');
      }

      this.userService
        .updateUser(this.authService.decodedToken.nameid, this.updateUser)
        .subscribe(
          (user: User) => {
            this.alertify.success('Profile updated successfully');
            this.editForm.reset(this.updateUser);
            Object.assign(this.user, user);
            this.authService.currentUser = user;
            if (!user.emailConfirmed) {
              this.router.navigate(['/confirmEmail/unconfirmed']);
            }
          },
          (error) => {
            this.alertify.error(error);
          }
        );
    }
  }
}
