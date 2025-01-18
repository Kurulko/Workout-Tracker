import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

import { User } from './user';
import { EditModelComponent } from '../shared/components/base/edit-model.component';
import { UserService } from './user.service';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-user-edit',
  templateUrl: './edit-user.component.html',
})
export class EditUserComponent extends EditModelComponent<User> implements OnInit {
  user: User = <User>{};
  maxDate: Date = new Date();

  readonly usersPath = "/users";

  constructor(private activatedRoute: ActivatedRoute,  
    private userService: UserService, 
    router: Router,  
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(router, impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.id = idParam ?? undefined;

    if (this.id) {
      // Edit mode
      this.userService.getUserById(this.id)
      .pipe(this.catchLoadDataError(this.usersPath))
      .subscribe(result => {
        this.user = result;
        this.title = `Edit User '${this.user.userName}'`;
      });
    }
    else {
      // Add mode
      this.title = "Create new User";
    }
  }

  onSubmit() {
    if (this.id) {
      // Edit mode
      this.userService.updateUser(this.user)
      .pipe(this.catchError())
      .subscribe(_ => {
        console.log("User " + this.user!.userId + " has been updated.");
        this.router.navigate([this.usersPath]);
      });
    }
    else {
      // Add mode
      this.userService.addUser(this.user)
      .pipe(this.catchError())
      .subscribe(result => {
        console.log("User " + result.userId + " has been created.");
        this.router.navigate([this.usersPath]);
      });
    }
  }
}