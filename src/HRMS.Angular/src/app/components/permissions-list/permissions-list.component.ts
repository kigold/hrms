import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Permission, PermissionInput } from '../../models/role';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-permissions-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './permissions-list.component.html',
  styleUrl: './permissions-list.component.css'
})
export class PermissionsListComponent {
  @Input() permissions: PermissionInput[] = []
  @Output() onSelectPermission = new EventEmitter<Permission>()
  @Output() onSelectAllPermissions = new EventEmitter<Permission>()
  @Output() onDeselectAllPermissions = new EventEmitter<Permission>()

  selectAllPermissions: boolean = false;

  ngOnChanges(){
    this.selectAllPermissions = this.checkedAll();
  }

  selectPermission(permission: Permission){
    this.onSelectPermission.emit(permission);
  }

  toggleSelectAllPermissions(){    
    this.selectAllPermissions = !this.selectAllPermissions;
    if (this.selectAllPermissions){
      this.selectAll();
    }
    else{
      this.deselectAll()
    }
  }

  selectAll(){ 
    this.onSelectAllPermissions.emit();
  }

  deselectAll(){ 
    this.onDeselectAllPermissions.emit();
  }

  checkedAll(){
    //If Empty return false 
    return this.permissions.length == 0 ? false : this.permissions.every((x) => x.checked)
  }
}
