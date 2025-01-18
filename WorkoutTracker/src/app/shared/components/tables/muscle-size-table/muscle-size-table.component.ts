import { Component, Input } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { MuscleSize } from 'src/app/muscle-sizes/muscle-size';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';
import { showSizeTypeShort } from 'src/app/shared/helpers/functions/showFunctions/showSizeTypeShort';
import { SizeType } from 'src/app/shared/models/size-type';
import { BaseTableComponent } from '../base-table.component';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { environment } from 'src/environments/environment.prod';

@Component({
  selector: 'app-muscle-size-table',
  templateUrl: './muscle-size-table.component.html',
  styleUrls: ['./muscle-size-table.component.css']
})
export class MuscleSizeTableComponent extends BaseTableComponent<MuscleSize> {
  constructor(
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);

    this.sortColumn = "date";
    this.sortOrder = "desc";
    this.displayedColumns = ['index', 'date', 'size', 'muscleName', 'musclePhoto', 'actions'];
  }
  
  envProduction = environment;

  showSizeTypeShort = showSizeTypeShort;
  roundSize = roundNumber;

  deleteMuscleSize = async (id: number): Promise<void> => {
    this.onDelete(id);
  };
}