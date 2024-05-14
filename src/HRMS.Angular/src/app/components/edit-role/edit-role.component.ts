import { Component, EventEmitter, input, Input, Output } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { EditRole, Permission, PermissionInput, RolePermissions } from '../../models/role';
import { CommonModule } from '@angular/common';
import { PermissionsListComponent } from '../permissions-list/permissions-list.component';

@Component({
  selector: 'app-edit-role',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, PermissionsListComponent],
  templateUrl: './edit-role.component.html',
  styleUrl: './edit-role.component.css'
})
export class EditRoleComponent {

  @Input() showForm: boolean = true;
  @Input() rolePermissions: RolePermissions = {id: 0, name: "", permissions: []}
  @Input() initialPermissions: number[] = []
  @Input() editRoleForm: FormGroup = new FormGroup({});
  @Output() onEditRole = new EventEmitter<EditRole>()
  @Output() onCloseForm = new EventEmitter()

  ngOnInit(){    
  }

  editRole() {
    const checkedPermissions = this.rolePermissions.permissions.filter(x => x.checked).map(x => x.id);
    const addedPermissions = checkedPermissions.filter(x => !this.initialPermissions.includes(x))
    const removedPermissions = this.initialPermissions.filter(x => !checkedPermissions.includes(x))
    const createRoleRequest: EditRole = {
      roleName: this.rolePermissions.name,
      addPermissionIds: addedPermissions,
      removePermissionIds: removedPermissions
    }
    this.onEditRole.emit(createRoleRequest);
  }

  selectPermission(permission: Permission){   
    this.rolePermissions.permissions.forEach(element => {
      if (permission.id == element.id){
          element.checked = !element.checked;
      }
    });

    this.editRoleForm.patchValue({permissionIds: this.rolePermissions.permissions.filter(x => x.checked).map(y =>y.id)})
  }

  selectAllPermissions(){     
    this.rolePermissions.permissions.forEach(permission => {   
      permission.checked = true;
    });
    this.editRoleForm.patchValue({permissionIds: this.rolePermissions.permissions.filter(x => x.checked).map(y =>y.id)})
  }

  deselectAllPermissions(){
    this.rolePermissions.permissions.forEach(permission => {   
      permission.checked = false;
    });
    this.editRoleForm.patchValue({permissionIds: []})
  }

  closeForm(){
    this.onCloseForm.emit();
  }
}