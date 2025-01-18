import { Component, EventEmitter, Output, Input, OnInit, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { Role } from 'src/app/roles/role';
import { RoleService } from 'src/app/roles/role.service';
import { BaseRoleSelectorComponent } from '../base-role-selector.component';

@Component({
  selector: 'app-role-selector',
  templateUrl: './role-selector.component.html',
  styleUrls: ['./role-selector.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => RoleSelectorComponent),
      multi: true,
    },
  ],
})
export class RoleSelectorComponent extends BaseRoleSelectorComponent<string> {
  @Output() roleIdChange = new EventEmitter<string>();
  
  selectedRoleId?: string;

  constructor(roleService: RoleService) {
    super(roleService);
  }

  onRoleIdSelected() {
    this.roleIdChange.emit(this.selectedRoleId);
    this.onChange(this.selectedRoleId);
    this.onTouched();
  }

  writeValue(value?: string): void {
    this.selectedRoleId = value;
  }
}
