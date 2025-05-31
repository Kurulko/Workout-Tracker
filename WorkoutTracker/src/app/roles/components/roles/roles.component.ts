import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';

import { ModelsTableComponent } from '../../../shared/components/base/models-table.component';
import { Role } from '../../models/role';
import { ApiResult } from '../../../shared/models/api-result';
import { ImpersonationManager } from '../../../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../../../shared/helpers/managers/token-manager';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { PreferencesManager } from '../../../shared/helpers/managers/preferences-manager';
import { RoleService } from '../../services/role.service';

@Component({
  selector: 'app-roles',
  templateUrl: './roles.component.html',
  styleUrls: ['./roles.component.css']
})
export class RolesComponent extends ModelsTableComponent<Role> implements OnInit {
  constructor(public roleService: RoleService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.filterColumn = "name";
    this.sortColumn = 'name';
    this.displayedColumns = ['index', 'name', 'actions'];
  }

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Role>> {
    return this.roleService.getRoles(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  getData(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;

    this.loadRoles();
  }

  loadRoles(){
    if(this.sort){
      this.sortColumn = this.sort.active;
      this.sortOrder = this.sort.direction as 'asc'|'desc';
    }

    this.loadData();
  }

  ngOnInit() {
    this.loadRoles();
  }

  private editingRoleId: string | null = null;
  editingRole: Role | null = null;

  isEditingRole(id: string): boolean {
    return this.editingRoleId === id;
  }

  startEditingRole(role: Role): void {
    this.editingRoleId = role.id;
    this.editingRole = {...role};
  }

  cancelEditingRole(): void {
    this.editingRoleId = null;
    this.editingRole = null;
  }

  isEditingNameValid: boolean = true;
  onNameChange(nameEditing: any): void {
    this.isEditingNameValid = nameEditing.valid; 
  }

  saveRole(): void {
    this.roleService.updateRole(this.editingRole!)
    .pipe(this.catchError())
    .subscribe(_ => {
        console.log("Role " + this.editingRole!.id + " has been updated.");
        this.editingRoleId = null;
        this.editingRole = null;
        this.loadData();
    });
  }
  
  isAddingRole: boolean = false;
  addingRoleName: string | null = null;

  startAddingRole(): void {
    this.isAddingRole = true;
  }

  cancelAddingRole(): void {
    this.isAddingRole = false;
    this.addingRoleName = null;
  }

  addRole(): void {
    var role = <Role>{ name: this.addingRoleName };
    this.roleService.createRole(role)
    .pipe(this.catchError())
    .subscribe(result => {
      console.log("Role " + result.id + " has been created.");
      this.isAddingRole = false;
      this.addingRoleName = null;

      this.loadData();
    });
  }

  deleteItem = async (id: string): Promise<void> => {
    this.roleService.deleteRole(id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.loadData();
        this.modelDeletedSuccessfully("Role");
      })
  };
}
