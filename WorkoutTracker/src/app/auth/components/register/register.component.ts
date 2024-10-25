import { Component, OnInit } from '@angular/core';import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RegisterModel } from '../../models/register.model';
import { AuthResult } from '../../models/auth-result.model';
import { TokenManager } from '../../../shared/helpers/token-manager';
import { AuthComponent } from '../auth.component';
import { Observable } from "rxjs";
import { MatSnackBar  } from '@angular/material/snack-bar';
import { FormGroup, FormControl, Validators, AbstractControl, ValidationErrors } from '@angular/forms';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent extends AuthComponent implements OnInit {
  passwordFieldType: string = 'password';
  confirmPasswordFieldType: string = 'password';

  constructor(tokenManager: TokenManager, router: Router, route: ActivatedRoute,  authService: AuthService, snackBar: MatSnackBar){
      super(tokenManager, router, route, authService, snackBar);
  }

  ngOnInit() {
    this.form = new FormGroup({
      rememberMe: new FormControl(''),
      name: new FormControl('', [Validators.required, Validators.minLength(3)]),
      email: new FormControl('', [Validators.required, Validators.email]),
      password: new FormControl('', [Validators.required, Validators.minLength(8)]),
      passwordConfirmation: new FormControl('', [Validators.required, Validators.minLength(8)])
    }, { validators: this.passwordsMatchValidator });
  }

  passwordsMatchValidator(formGroup: AbstractControl): ValidationErrors | null {
    const password = formGroup.get('password')?.value;
    const passwordConfirmation = formGroup.get('passwordConfirmation')?.value;
   
    return (password && passwordConfirmation && password === passwordConfirmation) ? null : { notMatching: true };
  }
  
  togglePasswordVisibility(): void {
    this.passwordFieldType = this.passwordFieldType === 'password' ? 'text' : 'password';
  }

  toggleConfirmPasswordVisibility(): void {
    this.confirmPasswordFieldType = this.confirmPasswordFieldType === 'password' ? 'text' : 'password';
  }

  getAuthResult() : Observable<AuthResult> {
      var registerModel = <RegisterModel>{};
      registerModel.name = this.form.controls['name'].value;
      registerModel.email = this.form.controls['email'].value;
      registerModel.password = this.form.controls['password'].value;
      registerModel.passwordConfirm = this.form.controls['passwordConfirmation'].value;

      return this.authService.register(registerModel);
  }
}