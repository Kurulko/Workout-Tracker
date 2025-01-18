import { Component, Input, OnInit } from '@angular/core';
import { map, Observable } from 'rxjs';
import { Role } from 'src/app/roles/role';
import { RoleService } from 'src/app/roles/role.service';
import { ModelsSelectorComponent } from '../models-selector.component';

@Component({
  template: ''
})
export abstract class BaseRoleSelectorComponent<T extends string|Role[]> extends ModelsSelectorComponent<T> implements OnInit {
  roles!: Observable<Role[]>;

  constructor(private roleService: RoleService) {
    super();
  }

  loadData(){
    this.roles = this.roleService
      .getRoles(this.pageIndex, this.pageSize, this.sortColumn, this.sortOrder, this.filterColumn, this.filterQuery)
      .pipe(map(x => x.data));
  }
}