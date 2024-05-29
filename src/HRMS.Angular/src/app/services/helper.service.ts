import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { throwError } from 'rxjs';
import { ToastService } from './toast.service';
import { ToastType } from '../models/toast';

@Injectable({
  providedIn: 'root'
})
export class HelperService {

  constructor(private toastService: ToastService) { }
  
  toastInfo( title: string){
    this.toast(ToastType.info, title);
  }

  toastSuccess( title: string){
	this.toast(ToastType.success, title);
  }

  toastError( title: string, errors: string[]){
    this.toast(ToastType.error, title, errors);
  }

  toast(type: ToastType, message: string, errors?: string[]){
	this.toastService.initiate({
		title: message,
		content: message,
		errors: errors,
		type: type
	})
  }

  handleError(error: any){
		if (error.status === 0) {
			// A client-side or network error occurred. Handle it accordingly.
			console.error('An error occurred:', error.error);
			this.toastError(error.message, []);
		  } 
		  else if (error.status === 400) {
			if (error.error.errors && error.error.errors['Error'])
				this.toastError(error.message, error.error.errors['Error']);
			else if (error.error && error.error['detail'])
				{
					this.toastError(error.message, [error.error['title'], error.error['detail']]);
				}
			else
				this.toastError(error.message, []);
		  } 
		  else {
			// The backend returned an unsuccessful response code.
			// The response body may contain clues as to what went wrong.
			console.error(`Backend returned code ${error.status}, body was: `, error.error, error);
			this.toastError(error.message, []);
		  }
		  // Return an observable with a user-facing error message.
		  return throwError(() => new Error('Something bad happened; please try again later.'));
	}
}
