import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  baseUrl = environment.apiUrl + 'auth/';
  jwtHelper = new JwtHelperService();
  decodedToken: any;
  currentUser: User;
  photoUrl = new BehaviorSubject<string>('../../assets/user.png');
  currentPhotoUrl = this.photoUrl.asObservable();

  constructor(private http: HttpClient) {}

  changeMemberPhoto(photoUrl: string) {
    this.photoUrl.next(photoUrl);
  }

  login(model: any) {
    return this.http.post(this.baseUrl + 'login', model).pipe(
      map((response: any) => {
        const user = response;
        if (user) {
          this.currentUser = user.user;
          // Only fully login if eamil has been confirmed
          localStorage.setItem('token', user.token);
          localStorage.setItem('user', JSON.stringify(user.user));
          this.decodedToken = this.jwtHelper.decodeToken(user.token);
          this.currentUser = user.user;
          this.changeMemberPhoto(this.currentUser.photoUrl);
        }
      })
    );
  }

  register(user: User) {
    let params = new HttpParams();
    params = params.append('baseClientUrl', encodeURI(window.location.origin));

    return this.http.post(this.baseUrl + 'register', user, { params });
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token);
  }

  roleMatch(allowedRoles): boolean {
    let isMatch = false;

    const userRoles = this.decodedToken.role as Array<string>;

    allowedRoles.forEach((element) => {
      if (userRoles.includes(element)) {
        isMatch = true;
        return;
      }
    });
    return isMatch;
  }

  confirmEmail(link: string) {
    return this.http.get(link);
  }

  resendConfirmationEmail() {
    let params = new HttpParams();
    params = params.append('baseClientUrl', encodeURI(window.location.origin));
    return this.http.get(
      this.baseUrl + 'resendConfirmation/' + this.currentUser.id, { params });
  }

  getUserByUsername(username: string) {
    return this.http.get<User>(this.baseUrl + 'getUserByUsername/' + username);
  }
  getUserByEmail(email: string) {
    return this.http.get<User>(this.baseUrl + 'getUserByEmail/' + encodeURI(email));
  }
}
