import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HelperService } from './helper.service';
import { PagedList, PageRequest, SearchPageRequest } from '../models/util';
import { UpdateUserRole, UpdateUserStatus, User, UserDetails } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class UserService implements BaseService{

	private SERVER_URL = "https://localhost:7178/auth";
	constructor(private httpClient: HttpClient, private helperService: HelperService) { 

  }

  getUsers(payload: SearchPageRequest){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

      const searchQuery = payload.query === undefined || null || "" ? "" : `&query=${payload.query}`
		return this.httpClient.get<PagedList<User[]>>(this.SERVER_URL + `/user?pageSize=${payload.pageSize}&pageNumber=${payload.page}${searchQuery}`, requestOptions);
	}

  getUserDetails(userId: number){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.get<UserDetails>(this.SERVER_URL + `/user/${userId}`, requestOptions);
	}
  
  getUserRoles(userId: number){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.get<string[]>(this.SERVER_URL + `/user/roles/${userId}`, requestOptions);
	}

  updateUserStatus(payload: UpdateUserStatus){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.put(this.SERVER_URL + '/user/status', payload, requestOptions);
	}

  updateUseRole(payload: UpdateUserRole){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.put(this.SERVER_URL + '/user/role/add', payload, requestOptions);
	}


  handleError(error: HttpErrorResponse) {
    this.helperService.handleError(error);
  }
}
