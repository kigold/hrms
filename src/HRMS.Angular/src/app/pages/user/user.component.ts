import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PageData, PageRequest, SearchPageRequest } from '../../models/util';
import { User } from '../../models/user';
import { HelperService } from '../../services/helper.service';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-user',
  standalone: true,
  imports: [ CommonModule],
  templateUrl: './user.component.html',
  styleUrl: './user.component.css'
})
export class UserComponent {

  users: User[] = [];
  pageRequest: SearchPageRequest = { page: 1, pageSize: 10 }  
  loading: boolean = false;
  showCreateEmployee: boolean = false;
  pageData: PageData = {
    page: 0,
    totalPage: 0,
    hasNextPage: false,
    hasPrevPage: false
  }
  headers: string[] = ['Id', 'Firstname', 'Lastname', 'Email', 'Actions' ]

  constructor(private userService: UserService, private helperService: HelperService
  ){}
  
  ngOnInit(){ 
    this.loading = true;
    this.getUsers();   
  }

  getUsers(){
    this.userService.getUsers(this.pageRequest)
    .subscribe({
      next: (res) => {
        this.users = res.items;
        this.pageData = {
          page: res.currentPage,
          totalPage: res.totalPages,
          hasNextPage: res.totalPages > res.currentPage,
          hasPrevPage: (res.currentPage - 1) > 0
        }
        this.loading = false;
        this.helperService.toastInfo(`Page ${this.pageData.page} of Users Loaded`);
      },
      error: (e) => this.userService.handleError(e)
    })
  }

  lockuser(userId: number){
    this.loading = true;
    this.userService.lockoutUser({userId: userId, lockout: true})
    .subscribe({
      next: () => {
        this.loading = false;
      },
      error: (e) => this.userService.handleError(e)
    })
  }

  onNextPage(){
    this.pageRequest.page += 1;
    console.log(">>>>>>> USer Search Query", this.pageRequest)
    this.getUsers();
  }

  onPrevPage(){
    this.pageRequest.page -= 1;
    this.getUsers();
  }
}
