import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

import { ModelsTableComponent } from 'src/app/shared/components/base/models-table.component';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { ApiResult } from 'src/app/shared/models/api-result';
import { ImpersonationService } from 'src/app/shared/services/impersonation.service';
import { User } from '../../models/user';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent extends ModelsTableComponent<User> implements OnInit {
  constructor(
    private router: Router, 
    private impersonationService: ImpersonationService, 
    private userService: UserService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,                                                                           
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar)                                                                              
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    
    this.sortColumn = 'userId';
    this.filterColumn = "userName";
    this.displayedColumns = ['id', 'userName', 'email', 'registered', 'startedWorkingOut', 'actions'];
  }

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<User>> {
    return this.userService.getUsers(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  ngOnInit() {
    this.loadData();
  }

  impersonateUser(id: string) {
    this.impersonationService.impersonate(id)
      .pipe(this.catchError())
      .subscribe((token) => {
        this.tokenManager.setToken(token);
        this.impersonationManager.startImpersonating();
        this.operationDoneSuccessfully("User", 'impersonated');
        this.router.navigate(['/']);
      })
  }

  deleteItem(id: string): void {
    this.userService.deleteUser(id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.loadData();
        this.modelDeletedSuccessfully("User");
      })
  };
}
