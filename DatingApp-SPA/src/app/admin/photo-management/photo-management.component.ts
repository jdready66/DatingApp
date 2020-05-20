import { Component, OnInit } from '@angular/core';
import { AdminService } from 'src/app/_services/admin.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: any[];

  constructor(private adminService: AdminService, private alertify: AlertifyService) { }

  ngOnInit() {
    this.getPhotosForModeration();
  }

  getPhotosForModeration() {
    this.adminService.getPhotosForModeration().subscribe((photos: any[]) => {
      this.photos = photos;
    }, error => {
      console.log(error);
      this.alertify.error('Error retrieving photos');
    });
  }

  approvePhoto(id: number) {
    this.adminService.approvePhoto(id).subscribe(() => {
      this.photos.splice(this.photos.findIndex(p => p.id == id), 1);
    }, error => {
      this.alertify.error(error);
    });
  }

  rejectPhoto(id: number) {
    this.alertify.confirm('Are you sure you want to reject this photo?', () => {
      this.adminService.rejectPhoto(id).subscribe(() => {
        this.photos.splice(this.photos.findIndex(p => p.id == id), 1);
      }, error => {
        this.alertify.error(error);
      });
    });
  }

}
