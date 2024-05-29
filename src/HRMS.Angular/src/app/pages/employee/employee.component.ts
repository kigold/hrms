import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { AddQualification, EditEmployee, EmployeeDetail } from '../../models/employee';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmployeeService } from '../../services/employee.service';
import { HelperService } from '../../services/helper.service';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ViewEmployeeDocComponent } from '../../employee/view-employee-doc/view-employee-doc.component';
import { MediaFile } from '../../models/util';

@Component({
  selector: 'app-employee',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule, ViewEmployeeDocComponent],
  templateUrl: './employee.component.html',
  styleUrl: './employee.component.css'
})
export class EmployeeComponent {
  loading: boolean = false;
  employeeId: number = 0;
  employeeDetail: EmployeeDetail = <EmployeeDetail>{}
  editEmployeeForm = this.formBuilder.nonNullable.group({
    firstName: [this.employeeDetail.firstName, [Validators.required, Validators.pattern(/^(\s+\S+\s*)*(?!\s).*$/)]],
    lastName: [this.employeeDetail.lastName, [Validators.required, Validators.pattern(/^(\s+\S+\s*)*(?!\s).*$/)]],
    email : [this.employeeDetail.email, [Validators.required, Validators.email]],
    phone : '',
    address: '',
    country: '',
    position: '',
    department: ''
  })
  today = new Date();
  addQualificationForm = this.formBuilder.nonNullable.group({
    title: ['', Validators.required],
    description: ['', Validators.required],    
    qualificationType: [0, Validators.required],
    employeeId: 0,
    educationLevel : [0, Validators.required],
    dateReceived: undefined,
    expiryDate: undefined,
    mediaFile: undefined
  })
  fileToUpload: any = null;
  doc: MediaFile = <MediaFile>{};
  showDoc: boolean = false;
  headers: string[] = ['Title', 'Description', 'Type', 'Level', 'Date Received', 'Expiry Date', 'Actions' ]

  constructor(
    private route: ActivatedRoute,
    private formBuilder: FormBuilder,
    private employeeService: EmployeeService,
    private helperService: HelperService
  ){}

  ngOnInit() {
    this.employeeId = parseInt(this.route.snapshot.paramMap.get('id')??"0");
    this.getEmployee(this.employeeId);    
  }

  editEmployee(){
    this.loading = true;
    const request = this.editEmployeeForm.getRawValue() as EditEmployee;
    request.employeeId = this.employeeId
    this.employeeService.updateEmployee(request)
    .subscribe({
      next: (res) => {
        this.ngOnInit();
      },
      error: (e) => this.employeeService.handleError(e)
    })
  }

  getEmployee(id: number){
    this.editEmployeeForm.reset({});
    this.employeeDetail = <EmployeeDetail>{};
    this.loading = true;
    this.employeeService.getEmployee(id)
    .subscribe({
      next: (res) => {
        this.employeeDetail = res;
        this.editEmployeeForm.patchValue({
          ...this.employeeDetail
        })
        this.loading = false;
      },
      error: (e) => this.employeeService.handleError(e)
    })
  }

  addQualification(){
    this.loading = true;
    const request = this.addQualificationForm.value as AddQualification;
    request.employeeId = this.employeeId;    
    request.qualificationType = parseInt(request.qualificationType.toString())
    request.educationLevel = parseInt(request.educationLevel.toString())
    request.mediaFile = this.fileToUpload;
    console.log(request)
    this.employeeService.addQualification(request)
    .subscribe({
      next: () => {
        this.addQualificationForm.reset({
        })
        this.fileToUpload = null;
        this.ngOnInit();
      },
      error: (e) => {
        this.loading = false;
        this.employeeService.handleError(e)
      }
    })
  } 

  changeFile(e: any){
    this.fileToUpload = e.target.files[0];
  }

  previewDocument(id: number){
    this.loading = true;
    this.showDoc = true;
    const qualification = this.employeeDetail.qualifications.filter(x => x.id == id)[0];
    this.doc = { fileId: qualification.mediaFileId, filePath : qualification.mediaFile, fileName: qualification.title, fileType: "" }
    this.loading = false;
  }

  closeDocView(){
    this.showDoc = false;
  }

  deleteQualification(id: number){
    this.loading = true;
    this.employeeService.removeQualification(id)
    .subscribe({
      next: () => {
        this.ngOnInit();
      },
      error: (e) => this.employeeService.handleError(e)
    })
  }
}
