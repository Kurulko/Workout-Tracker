import { Component } from '@angular/core';
import { WeightType } from 'src/app/shared/models/enums/weight-type';
import { SizeType } from 'src/app/shared/models/enums/size-type';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { MainComponent } from 'src/app/shared/components/base/main.component';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css']
})
export class AccountComponent extends MainComponent {
  constructor(
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  changePreferableWeightType(weightType?: WeightType) {
    this.preferableWeightType = weightType;
    if(this.preferableWeightType !== undefined)
      this.preferencesManager.setPreferableWeightType(this.preferableWeightType);
    else
      this.preferencesManager.clearPreferableWeightType();
  }

  changePreferableSizeType(sizeType?: SizeType) {
    this.preferableSizeType = sizeType;
    if(this.preferableSizeType !== undefined)
      this.preferencesManager.setPreferableSizeType(this.preferableSizeType);
    else
      this.preferencesManager.clearPreferableSizeType();
  }
}
