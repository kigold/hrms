import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HelperService } from './helper.service';
import { LoginRequest, LoginResponseModel, User } from '../models/auth';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthService implements BaseService {

	private SERVER_URL = "https://localhost:7178/auth"//config.apiBaseUrl;
	constructor(private httpClient: HttpClient, private helperService: HelperService) { 
    console.log("Initiating Auth Service", httpClient)
  }

  appname: string = "hrms";

	login(payload: LoginRequest){
		const requestOptions = {
			headers: {
			  'Content-Type': 'application/x-www-form-urlencoded',
			},
		  };

		const formPayload = new URLSearchParams();
		formPayload.append('grant_type', 'password');
		formPayload.append('password', payload.password);
		formPayload.append('username', payload.email);

		return this.httpClient.post<LoginResponseModel>(this.SERVER_URL + '/connect/token', formPayload, requestOptions);
	}

  logout() {
		localStorage.removeItem(`${this.appname}-profile`);
		localStorage.removeItem(`${this.appname}-token`);
		localStorage.removeItem(`${this.appname}-refresh_token`);
		localStorage.removeItem(`${this.appname}-token_expiry`);
		window.location.reload();
	}

  getUserProfile () {
		const userString = localStorage.getItem(`${this.appname}-profile`);
		if (userString != undefined)
			return JSON.parse(userString) as User;
		return undefined;
	}

  storeAuthInLocalStorage(payload: LoginResponseModel): User{
		console.log(payload)
		const user = this.toUser(jwtDecode (payload.access_token));
		localStorage.setItem(`${this.appname}-profile`, JSON.stringify(user));
		localStorage.setItem(`${this.appname}-token`, payload.access_token);
		localStorage.setItem(`${this.appname}-refresh_token`, payload.refresh_token);
		localStorage.setItem(`${this.appname}-token_expiry`, new Date(new Date().getTime() + ((payload.expires_in/60) * 60000)).toString());
		return user as User;
	}

  private toUser(u:any): User{
		return {
			id: parseInt(u.sub as string),
			name: u.name,
			username: u.username,
      avatar: u.avatar
		}
	}

  handleError(error: HttpErrorResponse) {
    this.helperService.handleError(error);
  }
  
}
//TODO Create Wrapper Logic for localstorage setitem and get item
