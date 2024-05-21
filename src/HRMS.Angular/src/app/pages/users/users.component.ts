import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { PageData, PageRequest, SearchPageRequest } from '../../models/util';
import { User } from '../../models/user';
import { HelperService } from '../../services/helper.service';
import { UserService } from '../../services/user.service';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [ CommonModule, RouterLink, FormsModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent {

  users: User[] = [];
  pageRequest: SearchPageRequest = { page: 1, pageSize: 10, query: '' }  
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
    this.getUsers();   
  }

  getUsers(){
    console.log(this.pageRequest)
    this.loading = true;
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

  updateUserStatus(userId: number, lockout: boolean){
    this.loading = true;
    this.userService.updateUserStatus({userId: userId, lockout: lockout})
    .subscribe({
      next: () => {
        this.loading = false;
        this.getUsers();
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
