import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PageData, PageRequest } from '../../models/util';
import { CreateRole, Permission, PermissionInput, Role } from '../../models/role';
import { HelperService } from '../../services/helper.service';
import { UserService } from '../../services/user.service';
import { CreateRoleComponent } from '../../components/create-role/create-role.component';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-role',  
  standalone: true,
  imports: [CreateRoleComponent, CommonModule],
  templateUrl: './role.component.html',
  styleUrl: './role.component.css'
})
export class RoleComponent {
  roles: Role[] = []
  permissions: PermissionInput[] = []
  pageRequest: PageRequest = { page: 1, pageSize: 10 }  
  loading: boolean = false;
  pageData: PageData = {
    page: 0,
    totalPage: 0,
    hasNextPage: false,
    hasPrevPage: false
  }
  createRoleRequest: CreateRole = {
    name: "",
    permissionIds: []
  };
  createRoleForm = new FormGroup({
    name: new FormControl<string>(this.createRoleRequest.name??"", [Validators.required]),
    permissionIds: new FormControl<number[]>(this.createRoleRequest.permissionIds??[], [Validators.required])
  });
  showCreateRole: boolean = false

  headers: string[] = ['Id', 'Name', 'Actions']

  constructor(private userService: UserService, private helperService: HelperService
  ){}
  
  ngOnInit(){   
    this.loading = true;
    this.getRoles();    
  }

  getRoles(){
    this.userService.getRoles(this.pageRequest)
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
        this.helperService.toastInfo(`Page ${this.pageData.page} of Roles Loaded`);
      },
      error: (e) => this.userService.handleError(e)
    })
  }

  getPermissions(){
    if (this.permissions.length > 0)
      return;

    this.userService.getAllPermissions()
    .subscribe({
      next: (res) => {
        this.permissions = res.map((x) => ({...x, checked: false}))
        this.loading = false;        
      },
      error: (e) => this.userService.handleError(e)
    })
  }

  onCreateRole(payload: CreateRole){
    this.loading = true;
    this.userService.createRoles(payload)
    .subscribe({
      next: () => {
        //clear Role Form
        this. createRoleRequest = { name: "", permissionIds: [] }
        this.createRoleForm.reset({...this. createRoleRequest})

        this.loading = false;
        this.showCreateRole = false;
        this.helperService.toastSuccess(`Role successfully Created`);
        this.getRoles();
        this.permissions.forEach(x => x.checked = false); //clear selection
      },
      error: (e) => {
        this.loading = false;
        this.userService.handleError(e);
      }
    })
    return false;
  }

  onDeleteRole(roleName: string){
    this.loading = true;
    this.userService.deleteRole(roleName)
    .subscribe({
      next: () => {
        this.loading = false;        
        this.getRoles();
      },
      error: (e) => {
        this.loading = false;
        this.userService.handleError(e);
      }
    })
  }

  editRole(roleName: string){
    console.log(">>>>> edit role", roleName)
  }

  onSelectPermission(permission: Permission){
    this.permissions.forEach(element => {
        if (permission.id == element.id){
            element.checked = !element.checked;
        }
    });
    const index = this.createRoleRequest.permissionIds?.indexOf(permission.id) ?? -1
    if (index > -1){      
      this.createRoleRequest.permissionIds?.splice(index, 1)
    }else{
      this.createRoleRequest.permissionIds?.push(permission.id);
    }    
    this.createRoleForm.patchValue({permissionIds: this.createRoleRequest.permissionIds})
  }

  onSelectAllPermissions(){
    this.permissions.forEach(permission => {   
      permission.checked = true;
    });
    this.createRoleRequest.permissionIds = this.permissions.map(x => x.id);
    this.createRoleForm.patchValue({permissionIds: this.createRoleRequest.permissionIds})
  }

  onDeselectAllPermissions(){
    this.permissions.forEach(permission => {   
      permission.checked = false;
    });
    this.createRoleRequest.permissionIds = [];
    this.createRoleForm.patchValue({permissionIds: []})
  }

  onToggleCreateRoleForm(){
    this.showCreateRole = !this.showCreateRole
    this.getPermissions();
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
