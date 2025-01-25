import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { StatusCodes } from 'http-status-codes';
import { MatSnackBar } from '@angular/material/snack-bar';

import { WorkoutRecord } from './workout-record';
import { WorkoutRecordService } from './workout-record.service';
import { MainComponent } from '../shared/components/base/main.component';
import { showWeightTypeShort } from '../shared/helpers/functions/showFunctions/showWeightTypeShort';
import { showExerciseValue } from '../shared/helpers/functions/showFunctions/showExerciseValue';
import { roundNumber } from '../shared/helpers/functions/roundNumber';
import { showTime } from '../shared/helpers/functions/showFunctions/showTime';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { showCountOfSomethingStr } from '../shared/helpers/functions/showFunctions/showCountOfSomethingStr';
import { ExerciseRecordService } from '../exercise-records/exercise-record.service';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-workout-record-details',
  templateUrl: './workout-record-details.component.html',
  styleUrls: ['./workout-record-details.component.css']
})
export class WorkoutRecordDetailsComponent extends MainComponent implements OnInit {
  workoutRecord!: WorkoutRecord;

  constructor(private activatedRoute: ActivatedRoute, 
    private router: Router, 
    private workoutRecordService: WorkoutRecordService, 
    private exerciseRecordService: ExerciseRecordService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  showWeightTypeShort = showWeightTypeShort;
  showExerciseValue = showExerciseValue;
  showCountOfSomethingStr = showCountOfSomethingStr;
  roundWeight = roundNumber;
  showTime = showTime;
  
  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    const workoutIdParam = this.activatedRoute.snapshot.paramMap.get('workoutId');
    const idParam = this.activatedRoute.snapshot.paramMap.get('id');

    if (idParam && workoutIdParam) {
      const id = +idParam, workoutId = +workoutIdParam;

      this.workoutRecordService.getWorkoutRecordById(id)
        .pipe(catchError((errorResponse: HttpErrorResponse) => {
          console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);

          if (errorResponse.status === StatusCodes.NOT_FOUND) {
              this.router.navigate(['workouts', workoutId , 'workout-records']);
          }

          this.showSnackbar(errorResponse.message);
          return throwError(() => errorResponse);
        }))
        .subscribe((result: WorkoutRecord) => {

          if(result.workoutId != workoutId)
            this.router.navigate(['/workouts']);
          else
            this.workoutRecord = result;
        });
    }
    else {
      this.router.navigate(['/workouts']);
    }
  }

  deleteWorkoutRecord() {
    this.workoutRecordService.deleteWorkoutRecord(this.workoutRecord.id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.modelDeletedSuccessfully("Workout Record");
        this.router.navigate(['workouts', this.workoutRecord.workoutId , 'workout-records']);
      })
  }

  deleteExerciseRecord = async (id: number): Promise<void> => {
    this.exerciseRecordService.deleteExerciseRecord(id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.loadData();
        this.modelDeletedSuccessfully("Exercise Record");
      })
  };
}