import { Component, EventEmitter, input, Input, Output } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CloneRole, CreateRole, Permission, PermissionInput } from '../../models/role';
import { CommonModule } from '@angular/common';
import { PermissionsListComponent } from '../permissions-list/permissions-list.component';

@Component({
  selector: 'app-create-role',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, PermissionsListComponent],
  templateUrl: './create-role.component.html',
  styleUrl: './create-role.component.css'
})
export class CreateRoleComponent {
  constructor(private formBuilder: FormBuilder) {}

  @Input() showForm: boolean = true;
  @Input() permissions: PermissionInput[] = []
  @Input() createRoleForm: FormGroup = new FormGroup({});
  @Output() onCreateRole = new EventEmitter<CreateRole>()
  @Output() onToggleForm = new EventEmitter()
  @Output() onSelectPermission = new EventEmitter<Permission>()
  @Output() onSelectAllPermissions = new EventEmitter()
  @Output() onDeselectAllPermissions = new EventEmitter()

  // createRoleForm = new FormGroup({
  //   name: new FormControl<string>(this.createRoleRequest.name??"", [Validators.required]),
  //   permissionIds: new FormControl<number[]>(this.createRoleRequest.permissionIds??[], [Validators.required])
  // });

  ngOnInit(){ 
  }

  createRole() {
    const createRoleRequest = this.createRoleForm.value as CreateRole;
    this.onCreateRole.emit(createRoleRequest);
  }

  selectPermission(permission: Permission){    
    this.permissions.forEach(element => {
      if (permission.id == element.id){
          element.checked = !element.checked;
      }
    });

    this.createRoleForm.patchValue({permissionIds: this.permissions.filter(x => x.checked).map(y =>y.id)})
  }

  selectAllPermissions(){           
    this.permissions.forEach(permission => {   
      permission.checked = true;
    });
    this.createRoleForm.patchValue({permissionIds: this.permissions.filter(x => x.checked).map(y =>y.id)})        
  }

  deselectAllPermissions(){
    this.permissions.forEach(permission => {   
      permission.checked = false;
    });
    this.createRoleForm.patchValue({permissionIds: []})
  }

  toggleForm(){
    this.onToggleForm.emit();
  }
}
