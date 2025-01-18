import { Component, Input } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { Exercise } from 'src/app/exercises/models/exercise';
import { showExerciseType } from 'src/app/shared/helpers/functions/showFunctions/showExerciseType';
import { showText } from 'src/app/shared/helpers/functions/showFunctions/showText';
import { showValuesStr } from 'src/app/shared/helpers/functions/showFunctions/showValuesStr';
import { BaseTableComponent } from '../base-table.component';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { environment } from 'src/environments/environment.prod';

@Component({
  selector: 'app-exercise-table',
  templateUrl: './exercise-table.component.html',
  styleUrls: ['./exercise-table.component.css']
})
export class ExerciseTableComponent extends BaseTableComponent<Exercise> {
  constructor(
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.displayedColumns = ['index', 'name', 'photo', 'description', 'type', 'muscles', 'equipments', 'actions'];
  }

  showExerciseType = showExerciseType;
  envProduction = environment;

  showDescription(exercise: Exercise): string|null {
    var description = exercise.description;
    
    if(description){
      const maxDescriptionLength = 50;
      return showText(description, maxDescriptionLength);
    }
    
    return description;
  }

  getMuscleNamesStr(exercise: Exercise): string {
    var muscleNames = exercise.muscles.map(muscle => muscle.name);
    const maxLength = 50;
    return showValuesStr(muscleNames, maxLength);
  }

  getEquipmentNamesStr(exercise: Exercise): string {
    var equipmentNames = exercise.equipments.map(equipment => equipment.name);
    const maxLength = 50;
    return showValuesStr(equipmentNames, maxLength);
  }

  deleteExercise = async (id: number): Promise<void> => {
    this.onDelete(id);
  };
}