import { Component, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { HelperService } from '../../services/helper.service';
import { UserDetails } from '../../models/user';
import { FormBuilder, Validators } from '@angular/forms';
import { RoleService } from '../../services/role.service';
import { Role } from '../../models/role';
import { PageData, PageRequest } from '../../models/util';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrl: './user.component.css'
})
export class UserComponent {  
  loading: boolean = false;
  userId: number = 0;
  user: UserDetails = <UserDetails>{};
  showUserRoleForm: boolean = false;
  addUserRoleForm = this.formBuilder.nonNullable.group({
    roleName: ['', Validators.required],
  })
  roles: Role[] = []
  pageRequest: PageRequest = { page: 1, pageSize: 4 }  
  pageData: PageData = {
    page: 0,
    totalPage: 0,
    hasNextPage: false,
    hasPrevPage: false
  }
  selectedRole: string = ''
  headers: string[] = ['Id', 'Name']

  constructor(
    private route: ActivatedRoute,
    private userService: UserService,
    private roleService: RoleService,
    private helperService: HelperService,
    private formBuilder: FormBuilder) {}
  
  ngOnInit() {
    this.userId = parseInt(this.route.snapshot.paramMap.get('id')??"0");
    console.log("Loading User Details")
    this.getUserDetails();
  }

  getUserDetails(){    
    this.loading = true;
    this.userService.getUserDetails(this.userId)
    .subscribe({
      next: (res) => {
        this.user = res;
        this.loading = false;
      },
      error: (e) => this.userService.handleError(e)
    })
  }

  getRoles(){
    this.roleService.getRoles(this.pageRequest)
    .subscribe({
      next: (res) => {
        this.roles = res.items;
        this.pageData = {
          page: res.currentPage,
          totalPage: res.totalPages,
          hasNextPage: res.totalPages > res.currentPage,
          hasPrevPage: (res.currentPage - 1) > 0
        }
        this.loading = false;
      },
      error: (e) => this.roleService.handleError(e)
    })
  }

  toggleAddUserRoleForm(){
    this.showUserRoleForm = !this.showUserRoleForm;
    if (this.showUserRoleForm){
      this.getRoles();
    }
  }

  selectRole(role:string){
    this.addUserRoleForm.get('roleName')?.setValue(role)
  }

  AddUserToRole(){    
    this.loading = true;
    const role = this.addUserRoleForm.get('roleName')?.value;
    this.userService.updateUseRole({userId: this.userId, roleName: role ?? ""})
    .subscribe({
      next: () => {
        this.loading = false;
        this.toggleAddUserRoleForm();
        this.getUserDetails();
        this.addUserRoleForm.reset({
          roleName: ""
        })
      },
      error: (e) => this.userService.handleError(e)
    })
  }

  onNextPage(){
    this.pageRequest.page += 1;    
    this.getRoles();
  }

  onPrevPage(){
    this.pageRequest.page -= 1;
    this.getRoles();
  }
}

//TODO Get ROle from List of Roles
