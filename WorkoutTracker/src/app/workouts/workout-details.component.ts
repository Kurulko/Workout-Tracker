import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { StatusCodes } from 'http-status-codes';
import { MatSnackBar } from '@angular/material/snack-bar';

import { MainComponent } from '../shared/components/base/main.component';
import { WorkoutService } from './workout.service';
import { showWeightTypeShort } from '../shared/helpers/functions/showFunctions/showWeightTypeShort';
import { showExerciseValue } from '../shared/helpers/functions/showFunctions/showExerciseValue';
import { roundNumber } from '../shared/helpers/functions/roundNumber';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { showCountOfSomethingStr } from '../shared/helpers/functions/showFunctions/showCountOfSomethingStr';
import { showValuesStr } from '../shared/helpers/functions/showFunctions/showValuesStr';
import { WorkoutDetails } from './workout-details';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-workout-details',
  templateUrl: './workout-details.component.html',
  styleUrls: ['./workout-details.component.css']
})
export class WorkoutDetailsComponent  extends MainComponent implements OnInit {
  workoutDetails!: WorkoutDetails;

  constructor(private activatedRoute: ActivatedRoute, 
    private router: Router, 
    private workoutService: WorkoutService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  readonly musclePanelOpenState = signal(false);
  readonly equipmentPanelOpenState = signal(false);
  
  showWeightTypeShort = showWeightTypeShort;
  showCountOfSomethingStr = showCountOfSomethingStr;
  showExerciseValue = showExerciseValue;
  roundWeight = roundNumber;

  showEquipmentsStr(maxLength: number){
    var equipmentNames = this.workoutDetails.equipments.map(e => e.name);
    return showValuesStr(equipmentNames, maxLength)
  }

  showMusclesStr(maxLength: number){
    var muscleNames = this.workoutDetails.muscles.map(e => e.name);
    return showValuesStr(muscleNames, maxLength)
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    const idParam = this.activatedRoute.snapshot.paramMap.get('id');

    if (idParam ) {
      const id = +idParam;

      this.workoutService.getWorkoutDetailsById(id)
        .pipe(catchError((errorResponse: HttpErrorResponse) => {
          console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);

          if (errorResponse.status === StatusCodes.NOT_FOUND) {
              this.router.navigate(['workouts']);
          }

          this.showSnackbar(errorResponse.message);
          return throwError(() => errorResponse);
        }))
        .subscribe((result: WorkoutDetails) => {
            this.workoutDetails = result;
        });
    }
    else {
      this.router.navigate(['/workouts']);
    }
  }

  deleteWorkoutRecord() {
    this.workoutService.deleteWorkout(this.workoutDetails.workout.id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.modelDeletedSuccessfully(`'${this.workoutDetails.workout.name}'`);
        this.router.navigate(['workouts']);
      })
  }
}