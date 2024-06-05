import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { EditEmployee, EmployeeDetail } from '../../models/employee';
import { CommonModule } from '@angular/common';
import { MediaFile } from '../../models/util';
import { HelperService } from '../../services/helper.service';
import { PdfViewerComponent } from '../../components/pdf-viewer/pdf-viewer.component';

@Component({
  selector: 'app-view-employee-doc',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, PdfViewerComponent],
  templateUrl: './view-employee-doc.component.html',
  styleUrl: './view-employee-doc.component.css'
})
export class ViewEmployeeDocComponent {

  extension: string = "";
  filePath: string = "";
  @Input() show: boolean = false;
  @Input() mediaFile: MediaFile = <MediaFile>{};
  @Output() onClose = new EventEmitter()
  imageExtensions: string[] = ['jpg', 'jpeg', 'png', 'gif']

constructor(private helperService: HelperService) {}

  ngOnChanges(){
    if (!this.show)
      return;

    this.setPath();
  }

  setPath(){
    this.extension = this.mediaFile.filePath?.split(".").at(-1) ?? "";
    this.filePath = this.helperService.getFilePath(this.mediaFile)    
  }

  closeView(){
    this.onClose.emit();
  }

  isImage(){
    return  this.show && this.imageExtensions.includes(this.extension);
  }

  isPdf(){
    return this.show && this.extension == 'pdf';
  }

}
