import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { EditModelComponent } from 'src/app/shared/components/base/edit-model.component';
import { UserDetailsService } from '../../services/user-details.service';
import { UserDetails } from '../../models/user-details';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { minAge } from 'src/settings';

@Component({
  selector: 'app-user-details',
  templateUrl: './user-details.component.html',
  styleUrls: ['./user-details.component.css']
})
export class UserDetailsComponent extends EditModelComponent<UserDetails> implements OnInit {
  userDetailsForm!: FormGroup;
  maxBirthdayDate!: Date;

  readonly accountPath: string = '/account';

  constructor(
    private fb: FormBuilder, 
    private userDetailsService: UserDetailsService, 
    router: Router, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(router, impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  ngOnInit(): void {
    const currentDate = new Date(); 
    currentDate.setFullYear(currentDate.getFullYear() - minAge) 
    this.maxBirthdayDate = currentDate;

    this.initForm();
    this.loadData();
  }

  initForm(): void {
    this.userDetailsForm = this.fb.group({
      birthday: [null, [ Validators.required ]],
      gender: [null, [ Validators.required ]],
      height: [null, [ Validators.required ]],
      weight: [null, [ Validators.required ]],
      bodyFatPercentage: [null, [ Validators.required ]],
    });
  };

  isLoading: boolean = true;
  loadData() {
    this.userDetailsService.getCurrentUserDetails()
      .pipe(this.catchError())
      .subscribe(result => {
        var userDetails = result ?? <UserDetails>{};
        const mappedUserDetails = {
          gender: userDetails.gender,
          bodyFatPercentage: userDetails.bodyFatPercentage,
          birthday: userDetails.dateOfBirth,
          weight: userDetails.weight,
          height: userDetails.height,
        };
        this.userDetailsForm.patchValue(mappedUserDetails);
        this.isLoading = false;
      });
  }

  onSubmit() {
    if (this.userDetailsForm.valid) {
      var userDetails = <UserDetails>{};

      userDetails.gender = this.userDetailsForm.controls['gender'].value;
      userDetails.dateOfBirth = this.userDetailsForm.controls['birthday'].value;
      userDetails.bodyFatPercentage = this.userDetailsForm.controls['bodyFatPercentage'].value;
      userDetails.height = this.userDetailsForm.controls['height'].value;
      userDetails.weight = this.userDetailsForm.controls['weight'].value;
      
      this.userDetailsService.updateUserDetails(userDetails) 
        .pipe(this.catchError())
        .subscribe(_ => {
          this.router.navigate([this.accountPath]);
        });
    }
  }
}