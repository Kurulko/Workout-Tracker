import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { User } from 'src/app/users/models/user';
import { UserService } from 'src/app/users/services/user.service';
import { EditModelComponent } from 'src/app/shared/components/base/edit-model.component';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent extends EditModelComponent<User> implements OnInit {
  user: User = <User>{};

  readonly accountPath: string = '/account';

  constructor(
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
    this.userService.getCurrentUser()
      .pipe(this.catchError())
      .subscribe(result => {
        this.user = result;
      });
  }

  onSubmit() {
    this.userService.updateUser(this.user)
      .pipe(this.catchError())
      .subscribe(_ => {
        this.router.navigate([this.accountPath]);
      });
  }
}