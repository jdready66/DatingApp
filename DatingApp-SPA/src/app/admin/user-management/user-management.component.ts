import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from '../roles-modal/roles-modal.component';
import { AuthService } from 'src/app/_services/auth.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css'],
})
export class UserManagementComponent implements OnInit {
  users: User[];
  bsModalRef: BsModalRef;

  constructor(
    private adminService: AdminService,
    private alertify: AlertifyService,
    private modalService: BsModalService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe(
      (users: User[]) => {
        this.users = users;
      },
      (error) => {
        console.log(error);
        this.alertify.error('Problem retrieving users');
      }
    );
  }

  editRolesModal(user: User) {
    const initialState = {
      user,
      roles: this.getRolesArray(user)
    };
    this.bsModalRef = this.modalService.show(RolesModalComponent, {
      initialState,
    });
    this.bsModalRef.content.updateSelectedRoles.subscribe((values) => {
      const rolesToUpdate = {
        roleNames: [...values.filter(el => el.checked === true).map(el => el.name)]
      }
      if (rolesToUpdate) {
        this.adminService.updateUserRoles(user, rolesToUpdate).subscribe(() => {
          user.roles = [...rolesToUpdate.roleNames];
        }, error => {
          console.log(error);
          this.alertify.error('Error updating user roles');
        });
      }
    });
  }

  private getRolesArray(user) {
    const roles = [];
    const userRoles = user.roles;
    const availableRoles: any[] = [
      {name: 'Admin', value: 'Admin'},
      {name: 'Moderator', value: 'Moderator'},
      {name: 'Member', value: 'Member'},
      {name: 'VIP', value: 'VIP'}
    ];

    // for (let i = 0; i < availableRoles.length; i++) {
    availableRoles.forEach((role) => {
      let isMatch = false;
      user.roles.foreach((userRole) => {
        if (role.value === userRole.value) {
          isMatch = true;
          roles.push(role);
        }
      });
      if (!isMatch) {
        role.checked = false;
        roles.push(role);
      }
    });

    return roles;
  }

  impersonate(user: User) {
    this.authService.impersonateLogin(user.id).subscribe((data: any) => {

      const win = window.open(window.location.origin + '/members');
      win.sessionStorage.setItem('token', data.token);
      win.sessionStorage.setItem('user', JSON.stringify(data.user));
      this.alertify.message('Impersonating in New Window');
    }, error => {
      this.alertify.error(error);
    });
  }
}
