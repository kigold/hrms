import { Injectable } from '@angular/core';
import { ToastData, ToastType } from '../models/toast';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ToastService {

  data: ToastData = {};
  public open = new Subject<ToastData>();

  initiate(data: ToastData){
    data.show = true;
    data.progressWidth =  100;
    this.data = data;
    this.open.next(data);
  }

  hide(){
    this.data.show = false;
    this.open.next(this.data)
  }
}
