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

  createRoleRequest: CreateRole = {
    name: "",
    permissionIds: []
  };

  @Input() showForm: boolean = true;
  @Input() permissions: PermissionInput[] = []
  @Input() createRoleForm: FormGroup = new FormGroup({});
  @Output() onCreateRole = new EventEmitter<CreateRole>()
  @Output() onToggleForm = new EventEmitter()
  @Output() getPermissions = new EventEmitter()
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
    this.createRoleRequest = this.createRoleForm.value as CreateRole;
    this.onCreateRole.emit(this.createRoleRequest);
  }

  selectPermission(permission: Permission){    
    this.onSelectPermission.emit(permission);
  }

  selectAllPermissions(){        
    this.onSelectAllPermissions.emit();
  }

  deselectAllPermissions(){
    this.onDeselectAllPermissions.emit();
  }

  toggleForm(){
    this.onToggleForm.emit();
  }
}
