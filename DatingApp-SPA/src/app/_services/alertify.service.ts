import { Injectable } from '@angular/core';
import * as alertify from 'alertifyjs';

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {
  constructor() {
    // override defaults
    alertify.defaults.transition = 'slide';
    alertify.defaults.theme.ok = 'btn btn-primary';
    alertify.defaults.theme.cancel = 'btn btn-danger';
    alertify.defaults.theme.input = 'form-control';
  }

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

  prompt(title: string, message: string, initValue: string, okCallback: (value) => any, cancelCallback: () => any) {
    alertify.prompt(
      title,
      message,
      initValue,
      (evt, value: string) => {
        if (okCallback) {
          okCallback(value);
        } else {
        }
      },
      () => {
        if (cancelCallback) {
          cancelCallback();
        } else {
        }
      }
    );
  }
}
