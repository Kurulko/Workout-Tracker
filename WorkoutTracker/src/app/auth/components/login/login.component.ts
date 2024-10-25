import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { LoginModel } from '../../models/login.model';
import { AuthResult } from '../../models/auth-result.model';
import { TokenManager } from '../../../shared/helpers/token-manager';
import { AuthComponent } from '../auth.component';
import { Observable } from "rxjs";
import { MatSnackBar  } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  providers: [AuthService]
})
export class LoginComponent extends AuthComponent {
  loginModel: LoginModel = <LoginModel>{};
  passwordFieldType: string = 'password';

  constructor(tokenManager: TokenManager, router: Router, route: ActivatedRoute, authService: AuthService, snackBar: MatSnackBar){
    super(tokenManager, router, route, authService, snackBar);
  }

  togglePasswordVisibility(): void {
    this.passwordFieldType = this.passwordFieldType === 'password' ? 'text' : 'password';
  }
  
  getAuthResult() : Observable<AuthResult> {
      return this.authService.login(this.loginModel);
  }
}
