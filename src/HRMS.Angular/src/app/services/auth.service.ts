import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HelperService } from './helper.service';
import { LoginRequest, LoginResponseModel, User } from '../models/auth';
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
			return JSON.parse(userString) as User;
		return undefined;
	}

  	storeAuthInLocalStorage(payload: LoginResponseModel): User{
		const user = this.toUser(jwtDecode (payload.access_token));
		this.setStoreItem('profile', JSON.stringify(user));
		this.setStoreItem('token', payload.access_token);
		this.setStoreItem('refresh_token', payload.refresh_token);
		this.setStoreItem('token_expiry', new Date(new Date().getTime() + ((payload.expires_in/60) * 60000)).toString());
		return user as User;
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

	toUser(u:any): User{
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
	access_token: "eyJhbGciOiJSUzI1NiIsImtpZCI6IkEwNTU5RDBCNjEwQTE4RDhFMzg0QzRBMkRENUY1RDBBNUZFNkVEMzAiLCJ4NXQiOiJvRldkQzJFS0dOampoTVNpM1Y5ZENsX203VEEiLCJ0eXAiOiJhdCtqd3QifQ.eyJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo3MjYyLyIsImV4cCI6MTcxMzgzNTU4NSwiaWF0IjoxNzEzODMxOTg1LCJhdWQiOlsiYXNzZXRfbWd0IiwiYXV0aCIsImVtcGxveWVlIl0sInNjb3BlIjoicm9sZXMgb2ZmbGluZV9hY2Nlc3MgZW1haWwgcHJvZmlsZSBQZXJtaXNzaW9uIGFwaSIsImp0aSI6IjI0NTZhYmNlLTdmZjUtNDE4Zi04YTljLThhNjNmOGU3Y2QxZSIsInN1YiI6IjEiLCJ1c2VybmFtZSI6ImFkbWluQGFwcC5jb20iLCJuYW1lIjoiQWRtaW4gTWFpbm1hbiIsImVtYWlsIjoiYWRtaW5AYXBwLmNvbSIsImF2YXRhciI6ImF2YXRhci0xLnBuZyIsInJvbGUiOiJBZG1pbiIsIlBlcm1pc3Npb24iOlsiRU1QTE9ZRUVfQ1JFQVRFIiwiRU1QTE9ZRUVfUkVBRCIsIkVNUExPWUVFX1VQREFURSIsIkVNUExPWUVFX0RFTEVURSIsIlVTRVJfQ1JFQVRFIiwiVVNFUl9SRUFEIiwiVVNFUl9VUERBVEUiLCJVU0VSX0RFTEVURSIsIlJPTEVfQ1JFQVRFIiwiUk9MRV9SRUFEIiwiUk9MRV9VUERBVEUiXSwib2lfYXVfaWQiOiIxMDA0OSIsIm9pX3Rrbl9pZCI6IjEwMTExIn0.43Ojs34BAIXIZnekdAGYH2XML8GWnUZODAnZ3kkrhpGaNFJRaJ8zbGDur7yi8Gb-6EZyTmvibv2pR0_KJhSVAHV5m8W7OlPTXvrc3nhpAnw6nRkMivvQWmD1ugVWrtpqnbFmJcRwYxSNrfvrZfGUVIRuvQWRzC2K17GSrZb50BmqKKSXgJg-IlI1d6Ggt4bZJTOzCr2EonkadiVS1E8SVB1RHWsvWgANmutpqxz63wb1aDN_pRWB9jd5hrd6lGAWhAlz3WZ7dp9ly2l611t8sGNfZy8GNaSl_Vc5JwPrDi1emi1A1KxWoTn4OHw9SwE46BMLUNGW4J6wuOrOe7QJ-A",
	token_type: "Bearer",
    expires_in: 3599,
    scope: "roles offline_access email profile Permission api",
    refresh_token: "eyJhbGciOiJSU0EtT0FFUCIsImVuYyI6IkEyNTZDQkMtSFM1MTIiLCJraWQiOiJERUMyRDdBNTNGNDFCRjAxMDQ4MDkzNTM2RjE2OEUyQjJCRTFFQ0JCIiwidHlwIjoib2lfcmVmdCtqd3QiLCJjdHkiOiJKV1QifQ.vX76K_H1WeiccRHT6_TrRxocJ5IpZAVLpVOsQ_ZE8oSyy6nGsEOFCgPsArDfyQDE3GckEwLZNyNeLqFR48ibsmIA5Zl8zNWJL2l8XadnE3uwQSWoNmN8bEMNrpFPBdcQROsR_SLxH57X3FqmWlXxNMx6LkA75PpVL1Jo6MhP0AOBaKZONwHO9i2HVFZT-QMLjuqdwfqFx-h49k5_Q6lAzUV524o8rOd-JAA2e5moca2r-oVcf7E65TaNKZd26kHOixECcQx-Wlvg_mnwTlR47JOHxZ5zg7W0anZQFKpre7pO9xrhTmgtK9lvoPTeQO7NH26Wqvbbe00ArF6tY9VnrQ.6WMu3eBB6mjuaZWGtPP_qw.6O7i4QwKXfBe9l60XkrppFIcGkU401Kdk8BghZ132N7lXUeprSrm_sd3U8n1HoPMb-KFMdbXludJbdJ_zybfq1Tt4-2WqW8dSKtjaO0fzNus2HW1D_VAUE8f2lYV_wtyx_vNziC3XHfNtS4Ndk_DStLkb3kvm7_-2bsbDwk0VYZGC_VnfY70XpoJzzGzAEWxjKYu5Ny1uuy1fJgv9a04DYPcL7PuNUIF3fyDkGup1AbrAI_bF2zxEG4j1_AsHJhuPnpHAW2xwQ1ftljKi6fmrhySd0uwUvv1aPGRVIdRr3isOs6WSNf0FrOWwb1rtf27rTcoeH4s_BvWQxp169D61H0YvzRzImf1u7aB7vvlRmftLBfJ_JLHiFLrV4N-M4ihbpWGV3X9xub0XqiICpyUYMJP5kxpE99DK2-kaZg-GsTrdh2c1aZ4gQKRry-dWGixjPt6QD3acbDSwrHTzmSb3fwoxdeW2ZiMuuyDcJct8OCzAxFTEDVNugVcavp9vtPrWLVs3_lpWeYAPdBB5CP1SkLDfKGKz3pjk3JxJHKmkw1ZLdEWlLspWZFvS4M77fL-C6apGdm2SsFykYeBaC68jyDtFQgYxuOCFyh8uGLKaiyBG5pjLFamttb-EIPWj-L4_0KrrO1Qx1KGKA7RzDkQqk31EurLk5mCe0W8SQoyEKKWnmWf7FO4UhtdMsDSU4rUVYEmbivfxy_UncTQAjSc-R9CVyE8lgz-DmWMikaS-JM_tUkr78-64uca570vWYgEC8HVtONHOz2jhj4YX1mlTZWVxIcYRQ_V3-sSIjSt0rjiB5_zxUqYqO-kEJyk9UfvNap8zQa_n4gPdK6gaxpUEjuSJ-Tvuj6KOqNCmOO3CmiFssrLM6Sni579BjtCkOtyWa4zUWzU-aeXb10YVvWscMB0fxsBdbv4HNFcdHx0EPN19WI-h3DR66Cg_El0ezxbrmL_A_faJ4R0dhjym_i7CmVLYUprnExJ63ybQD6ErzyVh98XLYDM8On2kFm9PWl6U20pIC3FMNSf94YG8yoCRfrjOigzP74Uk5MbxOMyF7QL98o62mMNfgbvLfGP13jsKUaRX2UEbEtaxYBnS2zuS7SdlqmEmgyLlOF1YqhhT4B36k-H1Alq6CCtaWxkLiX32vc8NOB-ObGCPM_hc8moF8jLuKFC53N8gypQy8aWlgA4aJfGsrWrynlzCvRJTYF7t3MeHJ1FLohduotrNNdJ40oIFkGq0bCvB7KSiMXr1baFAw-M1X3IDH_a17Spy3DEtuIcI5gBg6CbxedyZTVPRAV76xiXF44AnFbBpu8L22fvR1yBLV1kc2rY0SI0ON4YmF0G-G_ZVE3y8wF7LfMTQBxIBsVE1ELQMHpRh94zWnz5fbuf_AX9CR8yryEdu9NtBHya-kV0eBNAKjopHJyTIa3bPFTjbfyjQSvWJvDy--Zu26Up9Yz_XhLloA6bQjtMpEn9100DB6BjLn8l73_KFboy8Sa-QyTWh9WTS66ZNwN1DeNpsygMIhdYZJTs1921cZ4dNyhhSrBEXeYjVqmAyec8hoPOUf2XF9eiVYkNz-Y9SC-X7ZdW32KQM-XWaQ8GJRSUspBF4V-cLZlpiO3Fh4NYPqC2QJ8MDmJj0lKBichksI_fHObBoWKUgkHW6SBSfWMtdPMmVsT3RbADD28FhDfy5uNmFq-hzdjH4TMByTHV8Lc62AD_MvmjsPbttjXUEtev77c9vbd0RkJUgXP4Na4QYPx25D0So2TzdVemOVlGymyPK4BxupkjoSV8bzw1YtGKBFgFKvskHb39UVJ-aa4uRLlc4BHA1wj5lGEh-fKPEmj1KoGxbRfUDDmV3NMUdGAf6iGi7scy-2RppVcn46I5rcEIWiSY1PBKV3lDHYLZIQIXgzvE6SGRVTzItbaxKIR8wmrmskFieNiecg2MuIb23ki8bleW2xOTlAFUqJ4ojUXmk2zMmYI448yR3iOzhHU8X1p6n8k4W0Or477GfeZsTL-Qt5_MEJn6qSOHMPk.c8HdHDWxaFmrNmy-QeIrN8X6xcwala5fhOC3WrNupIQ"
}
//TODO Create Wrapper Logic for localstorage setitem and get item
