import { Component, Input } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { BaseTableComponent } from '../base-table.component';
import { Muscle } from 'src/app/muscles/muscle';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { environment } from 'src/environments/environment.prod';

@Component({
  selector: 'app-muscle-table',
  templateUrl: './muscle-table.component.html',
  styleUrls: ['./muscle-table.component.css']
})
export class MuscleTableComponent extends BaseTableComponent<Muscle> {
  constructor(
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.displayedColumns = ['index', 'name', 'photo', 'parentMuscleName', 'childMuscles', 'actions'];
  }
  
  envProduction = environment;

  getChildrenMuscleNames(muscle: Muscle): string|null {
    if(!muscle.childMuscles || muscle.childMuscles.length == 0)
      return null;

    var muscleNames = muscle.childMuscles?.map(child => child.name);
    const maxNameLength = 60;

    var result = muscleNames[0];
    if(result.length > maxNameLength)
      return result.slice(0, maxNameLength).concat("...");

    for(let i  = 1; i < muscleNames.length; i++){
      var muscleName = muscleNames[i];
      if(result.length + muscleName.length > maxNameLength){
        return result.concat(", ...");
      }

      result += `, ${muscleName}`;
    }
    
    return result;
  }

  deleteMuscle = async (id: number): Promise<void> => {
    this.onDelete(id);
  };
}
