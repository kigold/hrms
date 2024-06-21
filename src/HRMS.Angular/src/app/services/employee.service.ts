import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HelperService } from './helper.service';
import { AddQualification, CreateEmployee, EditEmployee, Employee, EmployeeDetail } from '../models/employee';
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

  updateEmployee(payload: EditEmployee){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.put(this.SERVER_URL + '/employee', payload, requestOptions);
	}

  addQualification(payload: AddQualification){
    console.log(payload)
    if (payload.mediaFile == null)
      return this.addQualificationWithoutFile(payload);
    return this.addQualificationfile(payload);
	}

  addQualificationfile(payload: AddQualification){
    console.log("Add Qual File")
		const requestOptions = {
        headers: {
          // 'Content-Type': 'multipart/form-data;',
        },
		  };

      const formPayload = new FormData();
      formPayload.append('employeeId', payload.employeeId.toString());
      formPayload.append('title', payload.title);
      formPayload.append('description', payload.description);      
      formPayload.append('educationLevel', payload.educationLevel.toString());
      formPayload.append('qualificationType', payload.qualificationType.toString());
      formPayload.append('dateReceived', payload.dateReceived?.toString() ?? "");
      formPayload.append('expiryDate', payload.expiryDate?.toString() ?? "");
      formPayload.append('file', payload.mediaFile);

		return this.httpClient.post<Employee>(this.SERVER_URL + '/employee/qualification/file', formPayload, requestOptions);
	}

  addQualificationWithoutFile(payload: AddQualification){
    console.log("Add Qul NO File")
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.post<Employee>(this.SERVER_URL + '/employee/qualification', payload, requestOptions);
	}

  removeQualification(id: number){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.delete<Employee>(this.SERVER_URL + `/employee/qualification/${id}`, requestOptions);
	}

  getEmployees(payload: PageRequest){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.get<PagedList<Employee[]>>(this.SERVER_URL + `/employee?pageSize=${payload.pageSize}&pageNumber=${payload.page}`, requestOptions);
	}

  getEmployee(id: number){
		const requestOptions = {
        headers: {
          'Content-Type': 'application/json',
        },
		  };

		return this.httpClient.get<EmployeeDetail>(this.SERVER_URL + `/employee/${id}`, requestOptions);
	}

  handleError(error: HttpErrorResponse) {
    this.helperService.handleError(error);
  }
}
