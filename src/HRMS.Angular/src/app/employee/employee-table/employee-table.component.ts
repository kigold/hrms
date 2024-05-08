import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { Employee } from '../../models/employee';
import { PageData } from '../../models/util';

@Component({
  selector: 'app-employee-table',
  standalone: true,
  imports: [ CommonModule],
  templateUrl: './employee-table.component.html',
  styleUrl: './employee-table.component.css'
})
export class EmployeeTableComponent {

  headers: string[] = ['Firstname', 'Lastname', 'Email', 'StaffId', 'Position', 'Department' ]
  @Input() employees: Employee[] = [];
  @Input() pageData: PageData = {
    page: 0,
    totalPage: 0,
    hasNextPage: false,
    hasPrevPage: false
  };
  @Output() nextPage = new EventEmitter()
  @Output() prevPage = new EventEmitter()
    


  ngOnInit(){    
  }

  ngOnChanges(){    
  }

  onNextPage(){
    this.nextPage.emit();
  }

  onPrevPage(){
    this.prevPage.emit();
  }
}
