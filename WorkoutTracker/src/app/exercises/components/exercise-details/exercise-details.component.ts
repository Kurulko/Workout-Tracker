import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ExerciseService } from '../../services/exercise.service';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { StatusCodes } from 'http-status-codes';
import { showText } from 'src/app/shared/helpers/functions/showFunctions/showText';
import { showExerciseType } from 'src/app/shared/helpers/functions/showFunctions/showExerciseType';
import { showValuesStr } from 'src/app/shared/helpers/functions/showFunctions/showValuesStr';
import { ExerciseDetails } from '../../models/exercise-details';
import { showCountOfSomethingStr } from 'src/app/shared/helpers/functions/showFunctions/showCountOfSomethingStr';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';
import { showExerciseValue, showReps, showWeight } from 'src/app/shared/helpers/functions/showFunctions/showExerciseValue';
import { showTime } from 'src/app/shared/helpers/functions/showFunctions/showTime';
import { ExerciseType } from '../../models/exercise-type';
import { ExerciseRecordService } from '../../../exercise-records/exercise-record.service';
import { ApiResult } from 'src/app/shared/models/api-result';
import { MainComponent } from 'src/app/shared/components/base/main.component';
import { ExerciseRecord } from 'src/app/exercise-records/exercise-record';
import { Workout } from 'src/app/workouts/workout';
import { WorkoutService } from 'src/app/workouts/workout.service';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { environment } from 'src/environments/environment.prod';

@Component({
  selector: 'app-exercise-details',
  templateUrl: './exercise-details.component.html',
  styleUrls: ['./exercise-details.component.css']
})
export class ExerciseDetailsComponent extends MainComponent implements OnInit {
  exerciseDetails!: ExerciseDetails;
  exerciseId!: number;

  constructor(private activatedRoute: ActivatedRoute, 
    private router: Router, 
    private exerciseService: ExerciseService, 
    private exerciseRecordService: ExerciseRecordService, 
    private workoutService: WorkoutService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
      super(impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  exercisePageType!: "yours"|"internal";

  ngOnInit(): void {
    const fullPath = this.router.url;
    
    if(fullPath.startsWith('/your-exercise'))
      this.exercisePageType = "yours";
    else
      this.exercisePageType = "internal";

    this.loadExerciseDetails();
    this.loadExerciseRecords();
    this.loadWorkouts();
  } 

  loadExerciseDetails() {
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.exerciseId = idParam ? +idParam : 0;
    if (this.exerciseId) {
      // Edit mode
      (this.exercisePageType === 'yours' ? 
        this.exerciseService.getUserExerciseDetailsById(this.exerciseId) :
        this.exerciseService.getInternalExerciseDetailsById(this.exerciseId)
      )
     .pipe(catchError((errorResponse: HttpErrorResponse) => {
        console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);

        if (errorResponse.status === StatusCodes.NOT_FOUND) {
          this.router.navigate(['exercises']);
        }

        this.showSnackbar(errorResponse.message);
        return throwError(() => errorResponse);
      }))
      .subscribe((result: ExerciseDetails) => {
        this.exerciseDetails = result;
      });
    } 
    else {
      this.router.navigate(['/exercises']);
    }
  }

  readonly musclePanelOpenState = signal(false);
  readonly equipmentPanelOpenState = signal(false);

  showExerciseType = showExerciseType;
  showCountOfSomethingStr = showCountOfSomethingStr;
  roundWeight = roundNumber;
  showExerciseValue = showExerciseValue;
  envProduction = environment;

  showExerciseSumValue(): string {
    const type = this.exerciseDetails.exercise.type;

    if(type == ExerciseType.Reps && this.exerciseDetails.sumOfReps != null)
      return showReps(this.exerciseDetails.sumOfReps);
    else if(type == ExerciseType.Time && this.exerciseDetails.sumOfTime != null)
      return showTime(this.exerciseDetails.sumOfTime);
    else if(type == ExerciseType.WeightAndReps && this.exerciseDetails.sumOfWeight != null)
      return showWeight(this.exerciseDetails.sumOfWeight);
    else if(type == ExerciseType.WeightAndTime && this.exerciseDetails.sumOfWeight != null && this.exerciseDetails.sumOfTime != null)
      return showWeight(this.exerciseDetails.sumOfWeight) + ' and ' + showTime(this.exerciseDetails.sumOfTime);
    else 
      throw new Error('Incorrect sum type');
  }

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

  showExerciseDescription(): string|null{
    const maxLength = 100;
    const description = this.exerciseDetails.exercise.description;

    if(description)
      return showText(description, maxLength); 
      
    return null;
  }

  getMuscleNamesStr(): string {
    var muscleNames = this.exerciseDetails.exercise.muscles.map(muscle => muscle.name);
    const maxLength = 100;
    return showValuesStr(muscleNames, maxLength);
  }

  getEquipmentNamesStr(): string {
    var equipmentNames = this.exerciseDetails.exercise.equipments.map(equipment => equipment.name);
    const maxLength = 100;
    return showValuesStr(equipmentNames, maxLength);
  }

  deleteExercise() {
    (this.exercisePageType === 'yours' ? 
      this.exerciseService.deleteUserExercise(this.exerciseDetails.exercise.id) :
      this.exerciseService.deleteInternalExercise(this.exerciseDetails.exercise.id)
    )
      .pipe(this.catchError())
      .subscribe(() => {
        this.modelDeletedSuccessfully("Exercise");
        this.router.navigate(['exercises']);
      })
  }

  exerciseRecords: ExerciseRecord[] = [];
  isExerciseRecords: boolean =  false;

  pageExerciseRecordIndex: number = 0;
  pageExerciseRecordSize: number = 5;
  totalExerciseRecordCount!: number;

  sortExerciseRecordColumn: string = "id";
  sortExerciseRecordOrder: "asc" | "desc" = "asc";
  filterExerciseRecordColumn?: string;
  filterExerciseRecordQuery?: string;
    
  displayedExerciseRecordColumns!: string[];

  loadExerciseRecords() {
    this.exerciseRecordService.getExerciseRecords(
        this.exerciseId, 
        null,  
        null,
        this.pageExerciseRecordIndex, 
        this.pageExerciseRecordSize, 
        this.sortExerciseRecordColumn, 
        this.sortExerciseRecordOrder, 
        this.filterExerciseRecordColumn ?? null, 
        this.filterExerciseRecordQuery ?? null)
      .pipe(this.catchError())
      .subscribe((apiResult: ApiResult<ExerciseRecord>) => {
        this.totalExerciseRecordCount = apiResult.totalCount;
        this.pageExerciseRecordIndex = apiResult.pageIndex;
        this.pageExerciseRecordSize = apiResult.pageSize;

        this.exerciseRecords = apiResult.data;
        this.isExerciseRecords = apiResult.data.length !== 0
      });
  }
 
  onExerciseRecordSortChange(event: { sortColumn: string; sortOrder: string }): void {
    this.sortExerciseRecordColumn = event.sortColumn;
    this.sortExerciseRecordOrder = event.sortOrder as 'asc' | 'desc';
    this.loadExerciseRecords();
  }

  onExerciseRecordPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageExerciseRecordIndex = event.pageIndex;
    this.pageExerciseRecordSize = event.pageSize;
    this.loadExerciseRecords();
  }

  onDeleteExerciseRecord(id: any): void {
    this.deleteExerciseRecord(id);
  }

  deleteExerciseRecord(id: number): void {
    this.exerciseRecordService.deleteExerciseRecord(id)
    .pipe(this.catchError())
    .subscribe(() => {
      this.loadExerciseRecords();
      this.modelDeletedSuccessfully("Exercise Record");
    })
  };


  workouts: Workout[] = [];
  isWorkouts: boolean =  false;

  pageWorkoutIndex: number = 0;
  pageWorkoutSize: number = 10;
  totalWorkoutCount!: number;

  sortWorkoutColumn: string = "id";
  sortWorkoutOrder: "asc" | "desc" = "asc";
  filterWorkoutColumn?: string;
  filterWorkoutQuery?: string;
    
  displayeWorkoutColumns!: string[];

  loadWorkouts() {
    this.workoutService.getWorkouts(
        this.exerciseId, 
        this.pageWorkoutIndex, 
        this.pageWorkoutSize, 
        this.sortWorkoutColumn, 
        this.sortWorkoutOrder, 
        this.filterWorkoutColumn ?? null, 
        this.filterWorkoutQuery ?? null)
      .pipe(this.catchError())
      .subscribe((apiResult: ApiResult<Workout>) => {
        this.totalWorkoutCount = apiResult.totalCount;
        this.pageWorkoutIndex = apiResult.pageIndex;
        this.pageWorkoutSize = apiResult.pageSize;

        this.workouts = apiResult.data;
        this.isWorkouts = apiResult.data.length !== 0
      });
  }
 
  onWorkoutSortChange(event: { sortColumn: string; sortOrder: string }): void {
    this.sortWorkoutColumn = event.sortColumn;
    this.sortWorkoutOrder = event.sortOrder as 'asc' | 'desc';
    this.loadWorkouts();
  }

  onWorkoutPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageWorkoutIndex = event.pageIndex;
    this.pageWorkoutSize = event.pageSize;
    this.loadWorkouts();
  }

  onDeleteWorkout(id: any): void {
    this.deleteWorkout(id);
  }

  deleteWorkout(id: number): void {
    this.workoutService.deleteWorkout(id)
    .pipe(this.catchError())
    .subscribe(() => {
      this.loadWorkouts();
      this.modelDeletedSuccessfully("Workout");
    })
  };
}