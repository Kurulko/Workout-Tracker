import { Component, EventEmitter, Output, Input, OnInit, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Role } from 'src/app/roles/role';
import { RoleService } from 'src/app/roles/role.service';
import { BaseRoleSelectorComponent } from '../base-role-selector.component';

@Component({
  selector: 'app-multiple-role-selector',
  templateUrl: './multiple-role-selector.component.html',
  styleUrls: ['./multiple-role-selector.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => MultipleRoleSelectorComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MultipleRoleSelectorComponent),
      multi: true,
    },
  ],
})
export class MultipleRoleSelectorComponent extends BaseRoleSelectorComponent<Role[]> {
  @Output() rolesChange = new EventEmitter<Role[]>();
  
  selectedRoles?: Role[];

  constructor(roleService: RoleService) {
    super(roleService);
  }

  onRolesSelected() {
    var roles = this.isNoneOptionSelected ? [] : this.selectedRoles;

    this.rolesChange.emit(roles);
    this.onChange(roles);
    this.onTouched();
  }

  writeValue(value?: Role[]): void {
    this.selectedRoles = value;
  }

  isDisabledNoneOption(): boolean {
    if(!this.selectedRoles)
      return false;

    return this.selectedRoles.length > 0 && !this.isNoneOptionSelected;
  }

  isNoneOptionSelected = false;
  noneOptionSelected(){
    this.isNoneOptionSelected = !this.isNoneOptionSelected;
  }

  validate() {
    return this.validateItems(this.selectedRoles, this.isNoneOptionSelected);
  }
}