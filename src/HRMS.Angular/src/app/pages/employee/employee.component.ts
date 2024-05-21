import { Component, signal  } from '@angular/core';
import { EmployeeTableComponent } from "../../employee/employee-table/employee-table.component";
import { CreateEmployee, Employee } from '../../models/employee';
import { EmployeeService } from '../../services/employee.service';
import { PageData, PageRequest } from '../../models/util';
import { CreateEmployeeComponent } from "../../employee/create-employee/create-employee.component";
import { CommonModule } from '@angular/common';
import { HelperService } from '../../services/helper.service';

@Component({
    selector: 'app-employee',
    standalone: true,
    templateUrl: './employee.component.html',
    styleUrl: './employee.component.css',
    imports: [EmployeeTableComponent, CreateEmployeeComponent, CommonModule]
})
export class EmployeeComponent {
  employees: Employee[] =[];
  pageRequest: PageRequest = { page: 1, pageSize: 10 }  
  loading: boolean = false;
  showCreateEmployee: boolean = false;
  pageData: PageData = {
    page: 0,
    totalPage: 0,
    hasNextPage: false,
    hasPrevPage: false
  }

  constructor(private employeeService: EmployeeService, private helperService: HelperService
  ){}

  ngOnInit(){    
    this.loading = true;
    this.getEmployees();
  }

  getEmployees(){
    this.employeeService.getEmployee(this.pageRequest)
    .subscribe({
      next: (res) => {
        this.employees = res.items;
        this.pageData = {
          page: res.currentPage,
          totalPage: res.totalPages,
          hasNextPage: res.totalPages > res.currentPage,
          hasPrevPage: (res.currentPage - 1) > 0
        }
        this.loading = false;
        this.helperService.toastInfo(`Page ${this.pageData.page} of Employees Loaded`);
      },
      error: (e) => this.employeeService.handleError(e)
    })
  }

  onCreateEmployee(request: CreateEmployee){
    this.loading = true;
    this.employeeService.createEmployee(request)
    .subscribe({
      next: (res) => {
        this.loading = false;        
        this.showCreateEmployee = false;
        this.helperService.toastSuccess(`Employee successfully Created`);
      },
      error: (e) => this.employeeService.handleError(e)
    })
  }

  onToggleEmployeeForm(){    
    this.showCreateEmployee = !this.showCreateEmployee
  }

  onNextPage(){
    this.pageRequest.page += 1;    
    this.getEmployees();
  }

  onPrevPage(){
    this.pageRequest.page -= 1;
    this.getEmployees();
  }
}
