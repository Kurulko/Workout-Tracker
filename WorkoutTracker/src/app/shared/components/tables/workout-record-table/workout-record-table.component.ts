import { Component, Input } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { showValuesStr } from 'src/app/shared/helpers/functions/showFunctions/showValuesStr';
import { BaseTableComponent } from '../base-table.component';
import { WorkoutRecord } from 'src/app/workout-records/workout-record';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';
import { showTime } from 'src/app/shared/helpers/functions/showFunctions/showTime';
import { showWeightType } from 'src/app/shared/helpers/functions/showFunctions/showWeightType';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-workout-record-table',
  templateUrl: './workout-record-table.component.html',
  styleUrls: ['./workout-record-table.component.css']
})
export class WorkoutRecordTableComponent  extends BaseTableComponent<WorkoutRecord> {
  constructor(
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.displayedColumns = ['index', 'time', 'weight', 'exercises', 'actions'];
  }

  showWeightTypeShort = showWeightType;
  showTime = showTime;
  roundWeight = roundNumber;

  getExerciseNamesStr(workoutRecord: WorkoutRecord): string {
    var exerciseNames = workoutRecord.exercises.map(exercise => exercise.name);
    const maxLength = 100;
    return showValuesStr(exerciseNames, maxLength);
  }

  deleteWorkoutRecord = async (id: number): Promise<void> => {
    this.onDelete(id);
  };
}
