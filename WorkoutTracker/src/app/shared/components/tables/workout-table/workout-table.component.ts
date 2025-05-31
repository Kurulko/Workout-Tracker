import { Component, Input } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { BaseTableComponent } from '../base-table.component';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';
import { showWeightTypeShort } from 'src/app/shared/helpers/functions/showFunctions/showWeightTypeShort';
import { showValuesStr } from 'src/app/shared/helpers/functions/showFunctions/showValuesStr';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { Workout } from 'src/app/workouts/models/workout';

@Component({
  selector: 'app-workout-table',
  templateUrl: './workout-table.component.html',
  styleUrls: ['./workout-table.component.css']
})
export class WorkoutTableComponent extends BaseTableComponent<Workout> {
  constructor(
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.displayedColumns = ['index', 'name', 'weight', 'created', 'started', 'exercises', 'actions'];
  }

  roundWeight = roundNumber;
  showWeightTypeShort = showWeightTypeShort;

  getExerciseNamesStr(workout: Workout): string {
    var exerciseNames = workout.exercises.map(exercise => exercise.name);
    const maxLength = 100;
    return showValuesStr(exerciseNames, maxLength);
  }

  deleteWorkout = async (id: number): Promise<void> => {
    this.onDelete(id);
  };
}
