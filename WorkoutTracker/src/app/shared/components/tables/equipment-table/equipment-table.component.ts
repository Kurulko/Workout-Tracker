import { Component } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { BaseTableComponent } from '../base-table.component';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { environment } from 'src/environments/environment.prod';
import { Equipment } from 'src/app/equipments/models/equipment';

@Component({
  selector: 'app-equipment-table',
  templateUrl: './equipment-table.component.html',
  styleUrls: ['./equipment-table.component.css']
})
export class EquipmentTableComponent  extends BaseTableComponent<Equipment> {
  constructor(
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.displayedColumns = ['index', 'photo', 'name', 'actions'];
  }

  envProduction = environment;
  
  deleteEquipment = async (id: number): Promise<void> => {
    this.onDelete(id);
  };
}
