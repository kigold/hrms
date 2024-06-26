import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HelperService } from './helper.service';
import { PagedList, PageRequest, SearchPageRequest } from '../models/util';
import { UpdateUserStatus, User } from '../models/user';
import { CloneRole, CreateRole, EditRole, Permission, Role } from '../models/role';

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private SERVER_URL = "https://localhost:7178/auth";
	constructor(private httpClient: HttpClient, private helperService: HelperService) { 

  }

  getRoles(payload: PageRequest){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.get<PagedList<Role[]>>(this.SERVER_URL + `/role?pageSize=${payload.pageSize}&pageNumber=${payload.page}`, requestOptions);
	}

  getAllPermissions(){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.get<Permission[]>(this.SERVER_URL + "/role/permissions/all", requestOptions);
	}

  getRolePermissions(role: string){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.get<Permission[]>(this.SERVER_URL + `/role/permissions/${role}`, requestOptions);
	}

  createRoles(payload: CreateRole){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.post(this.SERVER_URL + '/role', payload, requestOptions);
	}

  updateRolePermissions(payload: EditRole){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.put(this.SERVER_URL + '/role/permissions', payload, requestOptions);
	}

  cloneRoles(payload: CloneRole){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.post(this.SERVER_URL + '/role/cloneroles', payload, requestOptions);
	}

  deleteRole(roleName: string){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.delete(this.SERVER_URL + `/role/${roleName}`, requestOptions);
	}

  handleError(error: HttpErrorResponse) {
    this.helperService.handleError(error);
  }
}
