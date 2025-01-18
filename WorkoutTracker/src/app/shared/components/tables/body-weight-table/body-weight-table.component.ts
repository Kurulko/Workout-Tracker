import { Component } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';

import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { BaseTableComponent } from '../base-table.component';
import { BodyWeight } from 'src/app/body-weights/body-weight';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';
import { showWeightTypeShort } from 'src/app/shared/helpers/functions/showFunctions/showWeightTypeShort';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-body-weight-table',
  templateUrl: './body-weight-table.component.html',
  styleUrls: ['./body-weight-table.component.css']
})
export class BodyWeightTableComponent extends BaseTableComponent<BodyWeight> {
  constructor(
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.displayedColumns = ['index', 'date', 'weight', 'actions'];
  }

  roundWeight = roundNumber;
  showWeightTypeShort = showWeightTypeShort;
  
  deleteBodyWeight = async (id: number): Promise<void> => {
    this.onDelete(id);
  };
}