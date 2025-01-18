import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { LoginModel } from '../../models/login.model';
import { AuthResult } from '../../models/auth-result.model';
import { AuthComponent } from '../auth.component';
import { Observable } from "rxjs";
import { MatSnackBar  } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  providers: [AuthService]
})
export class LoginComponent extends AuthComponent {
  loginModel: LoginModel = <LoginModel>{};

  constructor(
    router: Router, 
    route: ActivatedRoute, 
    authService: AuthService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(router, route, authService, impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  
  getAuthResult() : Observable<AuthResult> {
    return this.authService.login(this.loginModel);
  }

  login(): void{
    this.pipeAuthResult()
    .subscribe((authResult: AuthResult) => {
      this.authResult = authResult;

      this.showSnackbar(authResult.message)
      this.tokenManager.setToken(authResult.token!);

      const redirectUrl = this.returnUrl || '/';
      this.router.navigateByUrl(redirectUrl);
    })
  }
}
