import { Component, Input } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { BaseTableComponent } from '../base-table.component';
import { Role } from 'src/app/roles/models/role';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-role-table',
  templateUrl: './role-table.component.html',
  styleUrls: ['./role-table.component.css']
})
export class RoleTableComponent extends BaseTableComponent<Role> {
  constructor(
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.displayedColumns = ['index', 'name', 'actions'];
  }

  deleteRole = async (id: string): Promise<void> => {
    this.onDelete(id);
  };
}