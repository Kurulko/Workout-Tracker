import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ImpersonationService } from '../shared/services/impersonation.service';
import { Router } from '@angular/router';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { ModelsTableComponent } from '../shared/components/base/models-table.component';
import { User } from './user';
import { UserService } from './user.service';
import { ApiResult } from '../shared/models/api-result';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';

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
