import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { StatusCodes } from 'http-status-codes';
import { MainComponent } from '../shared/components/base/main.component';
import { ApiResult } from '../shared/models/api-result';
import { Role } from './role';
import { RoleService } from './role.service';
import { UserService } from '../users/user.service';
import { User } from '../users/user';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-role-details',
  templateUrl: './role-details.component.html',
})
export class RoleDetailsComponent extends MainComponent implements OnInit  {
  role!: Role;
  roleId!: string;

  constructor(private activatedRoute: ActivatedRoute, 
    private router: Router,
    private roleService: RoleService, 
    private userService: UserService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  ngOnInit(): void {
    this.loadRole();
  } 

  loadRole() {
    var roleId = this.activatedRoute.snapshot.paramMap.get('id');
    if (roleId) {
      // Edit mode
      this.roleId = roleId;
      this.roleService.getRoleById(roleId)
        .pipe(catchError((errorResponse: HttpErrorResponse) => {
          console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);

          if (errorResponse.status === StatusCodes.NOT_FOUND) {
            this.router.navigate(['roles']);
          }

          this.showSnackbar(errorResponse.message);
          return throwError(() => errorResponse);
        }))
        .subscribe((result: Role) => {
            this.role = result;
            this.loadUsers();
        });
    } 
    else {
      this.router.navigate(['/roles']);
    }
  }

  deleteMuscle() {
    this.roleService.deleteRole(this.roleId)
    .pipe(this.catchError())
    .subscribe(() => {
      this.modelDeletedSuccessfully("Role");
      this.router.navigate(['roles']);
    })
  }

  users: User[] = [];
  isUsers: boolean =  false;

  userPageIndex: number = 0;
  userPageSize: number = 10;
  userTotalCount!: number;

  userSortColumn: string = "userName";
  userSortOrder: "asc" | "desc" = "asc";
  userFilterColumn?: string;
  userFilterQuery?: string;
    
  displayedUserColumns!: string[];

  loadUsers() {
    this.userService.getUsersByRole(
      this.role.name,  
      this.userPageIndex, 
      this.userPageSize, 
      this.userSortColumn, 
      this.userSortOrder, 
      this.userFilterColumn ?? null, 
      this.userFilterQuery ?? null
    )
      .pipe(this.catchError())
      .subscribe((apiResult: ApiResult<User>) => {
        this.userTotalCount = apiResult.totalCount;
        this.userPageIndex = apiResult.pageIndex;
        this.userPageSize = apiResult.pageSize;

        this.users = apiResult.data;
        this.isUsers = apiResult.data.length !== 0
      });
  }
  
  onUserSortChange(event: { sortColumn: string; sortOrder: string }): void {
    this.userSortColumn = event.sortColumn;
    this.userSortOrder = event.sortOrder as 'asc' | 'desc';
    this.loadUsers();
  }

  onUserPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.userPageIndex = event.pageIndex;
    this.userPageSize = event.pageSize;
    this.loadUsers();
  }

  onDeleteUser(id: any): void {
    this.deleteUser(id);
  }

  deleteUser(id: string): void {
    this.userService.deleteUser(id)
    .pipe(this.catchError())
    .subscribe(() => {
      this.loadUsers();
      this.modelDeletedSuccessfully("User");
    })
  };
}
