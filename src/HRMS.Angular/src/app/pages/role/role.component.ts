import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PageData, PageRequest } from '../../models/util';
import { CreateRole, EditRole, Permission, PermissionInput, Role, RolePermissions } from '../../models/role';
import { HelperService } from '../../services/helper.service';
import { UserService } from '../../services/user.service';
import { CreateRoleComponent } from '../../components/create-role/create-role.component';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { EditRoleComponent } from '../../components/edit-role/edit-role.component';
import { Observable, of, mergeMap, lastValueFrom } from 'rxjs';
import { RoleService } from '../../services/role.service';

@Component({
  selector: 'app-role',  
  standalone: true,
  imports: [CreateRoleComponent, EditRoleComponent, CommonModule],
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
    name: new FormControl<string>("", [Validators.required]),
    permissionIds: new FormControl<number[]>([], [Validators.required])
  });
  rolePermissions: RolePermissions = {
    id: 0,
    name: "",
    permissions: []
  }
  editRoleRequest: EditRole = {
    roleName: "",
    addPermissionIds: [],
    removePermissionIds: []
  };
  initialPermissions: number[] = []
  editRoleForm = new FormGroup({
    name: new FormControl<string>("", [Validators.required]),
    permissionIds: new FormControl<number[]>([], [Validators.required]),
  });
  showCreateRole: boolean = false
  showEditRole: boolean = false

  headers: string[] = ['Id', 'Name', 'Actions']

  constructor(private roleService: RoleService, private helperService: HelperService
  ){}
  
  ngOnInit(){   
    this.loading = true;
    this.getRoles();    
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
        this.helperService.toastInfo(`Page ${this.pageData.page} of Roles Loaded`);
      },
      error: (e) => this.roleService.handleError(e)
    })
  }

  getPermissions(){
    if (this.permissions.length > 0)
      return;

    this.roleService.getAllPermissions()
    .subscribe({
      next: (res) => {
        this.permissions = res.map((x) => ({...x, checked: false}))
        this.loading = false;        
      },
      error: (e) => this.roleService.handleError(e)
    })
  }

  onCreateRole(payload: CreateRole){
    this.loading = true;
    this.roleService.createRoles(payload)
    .subscribe({
      next: () => {
        //clear Role Form
        this.createRoleForm.reset({...this.createRoleRequest})

        this.loading = false;
        this.showCreateRole = false;
        this.helperService.toastSuccess(`Role successfully Created`);
        this.getRoles();
        this.permissions.forEach(x => x.checked = false); //clear selection
      },
      error: (e) => {
        this.loading = false;
        this.roleService.handleError(e);
      }
    })
    return false;
  }

  deleteRole(roleName: string){
    this.loading = true;
    this.roleService.deleteRole(roleName)
    .subscribe({
      next: () => {
        this.loading = false;        
        this.getRoles();
      },
      error: (e) => {
        this.loading = false;
        this.roleService.handleError(e);
      }
    })
  }

  onEditRole(payload: EditRole){
    console.log("Edit Payload", payload)
    this.loading = true;
    this.roleService.updateRolePermissions(payload)
    .subscribe({
      next: () => {
        //clear Role Form
        this.editRoleForm.reset({
          name: "",
          permissionIds: []
        })

        this.loading = false;
        this.showEditRole = false;
        this.helperService.toastSuccess(`Role successfully Created`);
        this.getRoles();
      },
      error: (e) => {
        this.loading = false;
        this.roleService.handleError(e);
      }
    })
    return false;
  }

  onToggleCreateRoleForm(){
    this.showCreateRole = !this.showCreateRole
    this.getPermissions();
  }

  async showEditRoleForm(roleName: string){
    this.showEditRole = true
    this.loading = true;
    this.rolePermissions = {
      id: 0,
      name: roleName,
      permissions: []
    }
    this.editRoleForm.patchValue({
      name: roleName,
      permissionIds: []
    })

    this.roleService.getAllPermissions()
    .subscribe({
      next: (res) => {
        var permissions = res.map((x) => ({...x, checked: false}))      
        this.roleService.getRolePermissions(roleName)
        .subscribe({
          next: (rolePermissions) => {
            this.initialPermissions = rolePermissions.map(x => x.id)
            this.rolePermissions.permissions = permissions
            rolePermissions.forEach(x => {
              const permission = this.rolePermissions.permissions?.find(p => p.id == x.id)
              if (permission != undefined){      
                permission.checked = true;
              }
            })
            this.editRoleRequest.roleName = roleName,
            this.editRoleForm.patchValue({
              name: roleName,
              permissionIds: rolePermissions.map(x => x.id)
            })
            this.loading = false;       
          },
          error: (e) => {
            this.loading = false;
            this.roleService.handleError(e);
          }
        })
      },
      error: (e) => this.roleService.handleError(e)
    })  
  }

  onCloseEditRoleForm(){
    this.showEditRole = false;
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
