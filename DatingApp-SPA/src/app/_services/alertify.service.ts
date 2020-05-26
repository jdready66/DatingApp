import { Injectable } from '@angular/core';
import * as alertify from 'alertifyjs';

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {
  constructor() {}

  confirmWithTitle(
    title: string,
    message: string,
    okCallback: () => any,
    cancelCallback: () => any
  ) {
    alertify.confirm(
      title,
      message,
      (o: any) => {
        if (o) {
          okCallback();
        } else {
        }
      },
      (c: any) => {
        if (c) {
          cancelCallback();
        } else {
        }
      }
    );
  }

  confirm(message: string, okCallback: () => any) {
    this.confirmWithTitle('Confirm', message, okCallback, () => {});
  }

  success(message: string) {
    alertify.success(message);
  }

  error(message: string) {
    alertify.error(message);
  }

  warning(message: string) {
    alertify.warning(message);
  }

  message(message: string) {
    alertify.message(message);
  }
}
