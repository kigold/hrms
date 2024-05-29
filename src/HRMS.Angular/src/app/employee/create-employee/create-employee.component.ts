import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CreateEmployee, EmployeeBase } from '../../models/employee';
import { FormGroup, FormControl, FormBuilder, Validators, ValidationErrors } from '@angular/forms';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-create-employee',
  standalone: true,
  imports: [ ReactiveFormsModule, CommonModule],
  templateUrl: './create-employee.component.html',
  styleUrl: './create-employee.component.css'
})
export class CreateEmployeeComponent {
  constructor(private formBuilder: FormBuilder) {}

  employee: CreateEmployee = {
    firstName: "",
    lastName: "",
    email : "",
    phone: "",
    address: "",
    country: "",
    position: "",
    department: ""
  };

  // employeeForm = new FormGroup({
  //   firstName: new FormControl<string>(this.employee.firstName, {nonNullable: true}),
  //   lastName: new FormControl<string>(this.employee.lastName, {nonNullable: true}),
  //   email : new FormControl<string>(this.employee.email, {nonNullable: true}),
  //   phone: new FormControl<string>(this.employee.phone, {nonNullable: true}),
  //   address: new FormControl<string>(this.employee.address, {nonNullable: true}),
  //   country: new FormControl<string>(this.employee.country, {nonNullable: true}),
  // });

  employeeForm = this.formBuilder.nonNullable.group({
    firstName: [this.employee.firstName, [Validators.required, Validators.pattern(/^(\s+\S+\s*)*(?!\s).*$/)]],
    lastName: ['', [Validators.required, Validators.pattern(/^(\s+\S+\s*)*(?!\s).*$/)]],
    email : ['', [Validators.required, Validators.email]],
    phone : '',
    address: '',
    country: '',
    position: '',
    department: ''
  })

  @Input() showForm: boolean = true;
  @Output() onCreateEmployee = new EventEmitter<CreateEmployee>()
  @Output() onToggleForm = new EventEmitter()

  // ngOnInit(){
  //   console.log(">>>>>>>> INiting", this.showForm)
  // }

  createEmployee() {
    this.employee = this.employeeForm.value as EmployeeBase;
    this.onCreateEmployee.emit(this.employee);
  }

  toggleForm(){
    this.onToggleForm.emit();
  }
}
