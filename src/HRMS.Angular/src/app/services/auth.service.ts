import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HelperService } from './helper.service';
import { LoginRequest, LoginResponseModel, AuthUser } from '../models/auth';
import { jwtDecode } from 'jwt-decode';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService implements BaseService {

	private SERVER_URL = "https://localhost:7178/auth"//config.apiBaseUrl;
	constructor(private httpClient: HttpClient, private helperService: HelperService) { 
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
		//return of<LoginResponseModel>(TOKEN);
	}

	logout() {
			this.removeStoreItem('profile');
			this.removeStoreItem('token');
			this.removeStoreItem('refresh_token');
			this.removeStoreItem('token_expiry');
			window.location.reload();
	}

  	getUserProfile () {
		const userString = this.getStoreItem('profile');
		if (userString != undefined)
			return JSON.parse(userString) as AuthUser;
		return undefined;
	}

  	storeAuthInLocalStorage(payload: LoginResponseModel): AuthUser{
		const user = this.toUser(jwtDecode (payload.access_token));
		this.setStoreItem('profile', JSON.stringify(user));
		this.setStoreItem('token', payload.access_token);
		this.setStoreItem('refresh_token', payload.refresh_token);
		this.setStoreItem('token_expiry', new Date(new Date().getTime() + ((payload.expires_in/60) * 60000)).toString());
		return user as AuthUser;
	}

	getToken() : string{
		return this.getStoreItem('token') as string;
	}
	
	isTokenExpired() {
		const expiryDate = this.getStoreItem('token_expiry');
		if (!expiryDate)
			return true;

		return (new Date().getTime() > Date.parse(expiryDate));
	}

	refreshAccessTokenAndStoreToken() {
		const requestOptions = {
			headers: {
			  'Content-Type': 'application/x-www-form-urlencoded',
			},
		};

		const refresh_token = this.getStoreItem('refresh_token');
		const formPayload = new URLSearchParams();
		formPayload.append('grant_type', 'refresh_token');
		formPayload.append('refresh_token', refresh_token as string);

		this.httpClient.post<LoginResponseModel>(this.SERVER_URL + '/connect/token', formPayload, requestOptions)
			.subscribe({
				next: (res) => {
            console.log("refreshed Token", res.access_token)
						this.storeAuthInLocalStorage(res as LoginResponseModel);
					},
				error: (e) => this.helperService.handleError(e)
			});
	}

	refreshAccessToken() {
		const requestOptions = {
			headers: {
			  'Content-Type': 'application/x-www-form-urlencoded',
			},
		};

		const refresh_token = this.getStoreItem('refresh_token');

		const formPayload = new URLSearchParams();
		formPayload.append('grant_type', 'refresh_token');
		formPayload.append('refresh_token', refresh_token as string);

		return this.httpClient.post<LoginResponseModel>(this.SERVER_URL + '/connect/token', formPayload, requestOptions);
	}

	setStoreItem(key: string, value: string){
		localStorage.setItem(`${this.appname}-${key}`, value);
	}

	removeStoreItem(key: string){
		localStorage.removeItem(`${this.appname}-${key}`);
	}

	getStoreItem(key: string){
		return localStorage.getItem(`${this.appname}-${key}`)
	}

	toUser(u:any): AuthUser{
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

const TOKEN : LoginResponseModel = <LoginResponseModel>{
  access_token: "",
  token_type: "",
    expires_in: 0,
    scope: "",
    refresh_token: ""}
//TODO Create Wrapper Logic for localstorage setitem and get item
