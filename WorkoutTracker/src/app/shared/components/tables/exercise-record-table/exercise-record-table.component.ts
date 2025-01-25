import { Component } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { BaseTableComponent } from '../base-table.component';
import { showCountOfSomethingStr } from 'src/app/shared/helpers/functions/showFunctions/showCountOfSomethingStr';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';
import { showExerciseType } from 'src/app/shared/helpers/functions/showFunctions/showExerciseType';
import { showExerciseValue, showReps, showTime, showWeight } from 'src/app/shared/helpers/functions/showFunctions/showExerciseValue';
import { ExerciseType } from 'src/app/exercises/models/exercise-type';
import { ExerciseRecord } from 'src/app/exercise-records/exercise-record';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { environment } from 'src/environments/environment.prod';

@Component({
  selector: 'app-exercise-record-table',
  templateUrl: './exercise-record-table.component.html',
  styleUrls: ['./exercise-record-table.component.css']
})
export class ExerciseRecordTableComponent extends BaseTableComponent<ExerciseRecord> {
  constructor(
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.displayedColumns = ['index', 'date', 'exerciseName', 'exerciseType', 'exercisePhoto', 'sum', 'value', 'actions'];
  }

  envProduction = environment;
  
  showExerciseType = showExerciseType;
  showCountOfSomethingStr = showCountOfSomethingStr;
  roundWeight = roundNumber;
  showExerciseValue = showExerciseValue;

  showExerciseRecordSumValue(exerciseRecord: ExerciseRecord): string {
    const type = exerciseRecord.exerciseType;

    if(type == ExerciseType.Reps && exerciseRecord.totalReps != null)
      return showReps(exerciseRecord.totalReps);
    else if(type == ExerciseType.Time && exerciseRecord.totalTime != null)
      return showTime(exerciseRecord.totalTime);
    else if(type == ExerciseType.WeightAndReps && exerciseRecord.totalWeight != null)
      return showWeight(exerciseRecord.totalWeight);
    else if(type == ExerciseType.WeightAndTime && exerciseRecord.totalWeight != null && exerciseRecord.totalTime != null)
      return showWeight(exerciseRecord.totalWeight) + ' and ' + showTime(exerciseRecord.totalTime);
    else 
      throw new Error('Incorrect sum type');
  }

  deleteExerciseRecord = async (id: number): Promise<void> => {
    this.onDelete(id);
  };
}
