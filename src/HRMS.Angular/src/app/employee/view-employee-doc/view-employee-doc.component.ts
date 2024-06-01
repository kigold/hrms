import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { EditEmployee, EmployeeDetail } from '../../models/employee';
import { CommonModule } from '@angular/common';
import { MediaFile } from '../../models/util';
import { HelperService } from '../../services/helper.service';

@Component({
  selector: 'app-view-employee-doc',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './view-employee-doc.component.html',
  styleUrl: './view-employee-doc.component.css'
})
export class ViewEmployeeDocComponent {

  @Input() show: boolean = false;
  @Input() mediaFile: MediaFile = <MediaFile>{};
  @Output() onClose = new EventEmitter()

constructor(private helperService: HelperService) {}


  getPath(file: MediaFile){
    return this.helperService.getFilePath(file)
  }

  closeView(){
    this.onClose.emit();
  }

}
