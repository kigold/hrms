import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HelperService } from './helper.service';
import { CreateEmployee, Employee } from '../models/employee';
import { PagedList, PageRequest } from '../models/util';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService implements BaseService{

	private SERVER_URL = "https://localhost:7178/employee";
	constructor(private httpClient: HttpClient, private helperService: HelperService) { 

  }

  createEmployee(payload: CreateEmployee){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.post<Employee>(this.SERVER_URL + '/employee', payload, requestOptions);
	}

  getEmployee(payload: PageRequest){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.get<PagedList<Employee[]>>(this.SERVER_URL + `/employee?pageSize=${payload.pageSize}&pageNumber=${payload.page}`, requestOptions);
	}

  handleError(error: HttpErrorResponse) {
    this.helperService.handleError(error);
  }
}
