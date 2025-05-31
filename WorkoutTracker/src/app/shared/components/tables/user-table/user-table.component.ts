import { Component, EventEmitter, Input, Output, TemplateRef, ViewChild } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { BaseTableComponent } from '../base-table.component';
import { MatDialog } from '@angular/material/dialog';
import { User } from 'src/app/users/models/user';
import { UserService } from 'src/app/users/services/user.service';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-user-table',
  templateUrl: './user-table.component.html',
  styleUrls: ['./user-table.component.css']
})
export class UserTableComponent  extends BaseTableComponent<User> {
  constructor(
    private dialog: MatDialog,
    private userService: UserService,
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.displayedColumns = ['index', 'id', 'userName', 'email', 'registered', 'startedWorkingOut', 'actions'];
  }

  currentUserId!: string;

  @Output() impersonate = new EventEmitter<string>();
  @ViewChild('impersonateButtonDialogTemplate') impersonateButtonDialogTemplate!: TemplateRef<boolean>;

  override ngOnInit() {
    this.userService.getCurrentUser()
      .pipe(this.catchError())
      .subscribe((currentUser) => {
        this.currentUserId = currentUser.userId;
      });

      this.onDataChanges();
  }

  impersonatedUserId!: string;
  openImpersonateButtonDialog(impersonatedUserId: string) {
    this.impersonatedUserId = impersonatedUserId;    
    this.dialog.open(this.impersonateButtonDialogTemplate, { width: '300px' });
  }

  onImpersonate(): void {
    this.impersonate.emit(this.impersonatedUserId); 
  }

  impersonateUser = async (): Promise<void> => {
    this.onImpersonate();
  };

  deleteUser = async (id: string): Promise<void> => {
    this.onDelete(id);
  };
}
