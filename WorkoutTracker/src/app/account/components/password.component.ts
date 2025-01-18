import { Component, OnInit, signal } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { FormGroup, FormControl, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MainComponent } from 'src/app/shared/components/base/main.component';
import { UserService } from 'src/app/users/user.service';
import { catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { throwError } from 'rxjs';
import { PasswordModel } from '../models/password-model';
import { Router } from '@angular/router';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-password',
  templateUrl: './password.component.html',
  styleUrls: ['./password.component.css']
})
export class PasswordComponent extends MainComponent implements OnInit {
  errors?: string[];
  passwordForm!: FormGroup;

  readonly profilePath: string = '/profile';

  constructor(
    private router: Router, 
    private userService: UserService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar,)
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  ngOnInit() {
    this.passwordForm = new FormGroup({
      oldPassword: new FormControl('', [Validators.required, Validators.minLength(8)]),
      newPassword: new FormControl('', [Validators.required, Validators.minLength(8)]),
      passwordConfirmation: new FormControl('', [Validators.required, Validators.minLength(8)])
    }, { validators: this.passwordsMatchValidator });
  }

  passwordsMatchValidator(formGroup: AbstractControl): ValidationErrors | null {
    const password = formGroup.get('newPassword')?.value;
    const passwordConfirmation = formGroup.get('passwordConfirmation')?.value;
   
    return (password && passwordConfirmation && password === passwordConfirmation) ? null : { notMatching: true };
  }
  
  protected changePassword(): void{
    var passwordModel = <PasswordModel>{};
    passwordModel.oldPassword = this.passwordForm.controls['oldPassword'].value;
    passwordModel.newPassword = this.passwordForm.controls['newPassword'].value;
    passwordModel.confirmNewPassword = this.passwordForm.controls['passwordConfirmation'].value;

    this.userService.changePassword(passwordModel)
      .pipe(catchError((errorResponse: HttpErrorResponse) => {
        this.errors = errorResponse.error;

        var errorsStr = this.errors?.join(";");
        console.error('Bad Request Error:', errorsStr);
        this.showSnackbar(errorsStr!);

        return throwError(() => errorResponse);
      }))
      .subscribe(() => {
        this.router.navigate([this.profilePath]);
      })
  }
}
