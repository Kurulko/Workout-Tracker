import { Component, OnInit } from '@angular/core';import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup, FormControl, Validators, AbstractControl, ValidationErrors, FormBuilder } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { RegisterModel } from '../../models/register.model';
import { AuthResult } from '../../models/auth-result.model';
import { AuthComponent } from '../auth.component';
import { Observable } from "rxjs";
import { MatSnackBar  } from '@angular/material/snack-bar';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { UserDetailsService } from 'src/app/account/services/user-details.service';
import { UserDetails } from 'src/app/account/models/user-details';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { minAge } from 'src/settings';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent extends AuthComponent implements OnInit {
  maxBirthdayDate!: Date;

  constructor(private fb: FormBuilder,
    private userDetailsService: UserDetailsService,
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

  registerForm!: FormGroup;
  personalDataForm!: FormGroup;
  rememberMeForm!: FormGroup;

  ngOnInit() {
    const currentDate = new Date(); 
    currentDate.setFullYear(currentDate.getFullYear() - minAge) //at least 5 y.o.
    this.maxBirthdayDate = currentDate;

    this.initForm();
  }

  initForm(): void {
    this.registerForm = new FormGroup({
      name: new FormControl(null, [ Validators.required, Validators.minLength(3)] ),
      email: new FormControl(null, [ Validators.email ]),
      password: new FormControl(null, [ Validators.required, Validators.minLength(8) ]),
      passwordConfirm: new FormControl(null, [ Validators.required, Validators.minLength(8) ])
    }, { validators: this.passwordsMatchValidator });

    this.personalDataForm = this.fb.group({
      birthday: [null, [ Validators.required ]],
      gender: [null, [ Validators.required ]],
      height: [null, [ Validators.required ]],
      weight: [null, [ Validators.required ]],
      bodyFatPercentage: [null, [ Validators.required ]],
    });

    this.rememberMeForm = new FormGroup({
      rememberMe: new FormControl(false),
    });
  };

  passwordsMatchValidator(formGroup: AbstractControl): ValidationErrors | null {
    const password = formGroup.get('password')?.value;
    const passwordConfirm = formGroup.get('passwordConfirm')?.value;
   
    return (password && passwordConfirm && password === passwordConfirm) ? null : { notMatching: true };
  }
  
  getAuthResult() : Observable<AuthResult> {
    const registerModel: RegisterModel = this.registerForm.value;
    registerModel.rememberMe = this.rememberMeForm.controls['rememberMe'].value;

    return this.authService.register(registerModel);
  }

  register(): void{
    this.pipeAuthResult()
    .subscribe((authResult: AuthResult) => {
      this.authResult = authResult;
      this.addPersonalData();

      this.showSnackbar(authResult.message)
      this.tokenManager.setToken(authResult.token!);

      const redirectUrl = this.returnUrl || '/';
      this.router.navigateByUrl(redirectUrl);
    })
  }

  addPersonalData() {
    if (this.personalDataForm.valid) {
      var userDetails = <UserDetails>{};

      userDetails.gender = this.personalDataForm.controls['gender'].value;
      userDetails.dateOfBirth = this.personalDataForm.controls['birthday'].value;
      userDetails.bodyFatPercentage = this.personalDataForm.controls['bodyFatPercentage'].value;
      userDetails.height = this.personalDataForm.controls['height'].value;
      userDetails.weight = this.personalDataForm.controls['weight'].value;

      this.userDetailsService.addUserDetails(userDetails)
        .pipe(this.catchError())
        .subscribe();
    }
  }
}