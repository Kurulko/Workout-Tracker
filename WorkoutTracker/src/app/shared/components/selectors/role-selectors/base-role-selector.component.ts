import { Component, Input, OnInit } from '@angular/core';
import { map, Observable } from 'rxjs';
import { Role } from 'src/app/roles/models/role';
import { ModelsSelectorComponent } from '../models-selector.component';
import { RoleService } from 'src/app/roles/services/role.service';

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