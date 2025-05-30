import { Component, EventEmitter, Output, Input, OnInit, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';
import { BaseRoleSelectorComponent } from '../base-role-selector.component';
import { RoleService } from 'src/app/roles/services/role.service';

@Component({
  selector: 'app-role-selector',
  templateUrl: './role-selector.component.html',
  styleUrls: ['./role-selector.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => RoleSelectorComponent),
      multi: true,
    },
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

  validate() {
    return this.validateItemId(this.selectedRoleId)
  }
}
