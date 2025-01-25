import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/services/auth.service';
import { ImpersonationService } from '../shared/services/impersonation.service';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MainComponent } from '../shared/components/base/main.component';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})
export class NavMenuComponent extends MainComponent {
  constructor(
    private authService: AuthService, 
    private impersonationService: ImpersonationService, 
    private router: Router,
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  onLogout(): void {
    this.authService.logout();
    this.tokenManager.logout();
    this.router.navigate(["/login"]);
  }

  stopImpersonating(): void {
    this.impersonationService.revert()
      .subscribe((token) => {
        this.tokenManager.setToken(token);
        this.impersonationManager.finishImpersonating();
        this.operationDoneSuccessfully("Impersonating", 'stopped');
        // this.router.navigate(["/"]);
        window.location.reload();   
      })
  }
}