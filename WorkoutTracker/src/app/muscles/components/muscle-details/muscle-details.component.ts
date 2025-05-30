import { Component, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MuscleDetails } from '../../models/muscle-details';
import { ImpersonationManager } from '../../../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../../../shared/helpers/managers/token-manager';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MuscleService } from '../../services/muscle.service';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { StatusCodes } from 'http-status-codes';
import { showExerciseType } from '../../../shared/helpers/functions/showFunctions/showExerciseType';
import { MainComponent } from '../../../shared/components/base/main.component';
import { showValuesStr } from '../../../shared/helpers/functions/showFunctions/showValuesStr';
import { ApiResult } from '../../../shared/models/api-result';
import { PreferencesManager } from '../../../shared/helpers/managers/preferences-manager';
import { SizeType } from '../../../shared/models/enums/size-type';
import { Exercise } from '../../../exercises/models/exercise';
import { ExerciseService } from '../../../exercises/services/exercise.service';
import { environment } from 'src/environments/environment.prod';
import { MuscleSizeService } from '../../services/muscle-size.service';
import { MuscleSize } from '../../models/muscle-size';

@Component({
  selector: 'app-muscle-details',
  templateUrl: './muscle-details.component.html',
})
export class MuscleDetailsComponent extends MainComponent implements OnInit  {
  muscleDetails!: MuscleDetails;
  muscleId!: number;

  constructor(private activatedRoute: ActivatedRoute, 
    private router: Router,
    private muscleService: MuscleService, 
    private muscleSizeService: MuscleSizeService, 
    private exerciseService: ExerciseService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  readonly muscleChildsPanelOpenState = signal(false);
  envProduction = environment;
  
  ngOnInit(): void {
    this.loadMuscleDetails();
    this.loadExercises();
    this.loadMuscleSizes();
  } 

  hasParentMuscle: boolean = false;
  hasChildMuscles: boolean = false; 
  hasMuscleSizes: boolean = false; 
  hasExercises: boolean = false; 

  loadMuscleDetails() {
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.muscleId = idParam ? +idParam : 0;
    if (this.muscleId) {
      // Edit mode
      this.muscleService.getMuscleDetailsById(this.muscleId)
     .pipe(catchError((errorResponse: HttpErrorResponse) => {
        console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);

        if (errorResponse.status === StatusCodes.NOT_FOUND) {
          this.router.navigate(['muscles']);
        }

        this.showSnackbar(errorResponse.message);
        return throwError(() => errorResponse);
      }))
      .subscribe((result: MuscleDetails) => {
        this.muscleDetails = result;

        this.hasParentMuscle = result.muscle.parentMuscleId !== null;
        this.hasChildMuscles = result.muscle.childMuscles !== null && result.muscle.childMuscles.length > 0;
        this.hasExercises = result.exercises !== null && result.exercises.length > 0;
        this.hasMuscleSizes = result.muscleSizes !== null && result.muscleSizes.length > 0;
      });
    } 
    else {
      this.router.navigate(['/muscles']);
    }
  }

  deleteMuscle() {
    this.muscleService.deleteMuscle(this.muscleId)
    .pipe(this.catchError())
    .subscribe(() => {
      this.modelDeletedSuccessfully("Muscle");
      this.router.navigate(['muscles']);
    })
  }

  showExerciseType = showExerciseType;
  
  getChildMuscleNamesStr(): string|null {
    var childMuscles = this.muscleDetails.muscle.childMuscles;

    if(!childMuscles)
      return null;

    const maxLength = 100;
    var muscleNames =childMuscles.map(childMuscle => childMuscle.name);
    return showValuesStr(muscleNames, maxLength);
  }

  muscleSizes: MuscleSize[] = [];
  isMuscleSizes: boolean =  false;

  muscleSizePageIndex: number = 0;
  muscleSizePageSize: number = 10;
  muscleSizeTotalCount!: number;

  muscleSizeSortColumn: string = "id";
  muscleSizeSortOrder: "asc" | "desc" = "asc";
  muscleSizeFilterColumn?: string;
  muscleSizeFilterQuery?: string;
    
  loadMuscleSizes() {
    (this.preferableSizeType == SizeType.Centimeter ?
     this.muscleSizeService.getMuscleSizesInCentimeters(
        this.muscleId,  
        null,
        this.muscleSizePageIndex, 
        this.muscleSizePageSize, 
        this.muscleSizeSortColumn, 
        this.muscleSizeSortOrder, 
        this.muscleSizeFilterColumn ?? null, 
        this.muscleSizeFilterQuery ?? null) :
      this.muscleSizeService.getMuscleSizesInInches(
        this.muscleId,  
        null,
        this.muscleSizePageIndex, 
        this.muscleSizePageSize, 
        this.muscleSizeSortColumn, 
        this.muscleSizeSortOrder, 
        this.muscleSizeFilterColumn ?? null, 
        this.muscleSizeFilterQuery ?? null) 
    )
      .pipe(this.catchError())
      .subscribe((apiResult: ApiResult<MuscleSize>) => {
        this.muscleSizeTotalCount = apiResult.totalCount;
        this.muscleSizePageIndex = apiResult.pageIndex;
        this.muscleSizePageSize = apiResult.pageSize;

        this.muscleSizes = apiResult.data;
        this.isMuscleSizes = apiResult.data.length !== 0
      });
  }
  
  onMuscleSizeSortChange(event: { sortColumn: string; sortOrder: string }): void {
    this.muscleSizeSortColumn = event.sortColumn;
    this.muscleSizeSortOrder = event.sortOrder as 'asc' | 'desc';
    this.loadMuscleSizes();
  }

  onMuscleSizePageChange(event: { pageIndex: number; pageSize: number }): void {
    this.muscleSizePageIndex = event.pageIndex;
    this.muscleSizePageSize = event.pageSize;
    this.loadMuscleSizes();
  }

  onDeleteMuscleSize(id: any): void {
    this.deleteMuscleSize(id);
  }

  deleteMuscleSize(id: number): void {
    this.muscleSizeService.deleteMuscleSize(id)
    .pipe(this.catchError())
    .subscribe(() => {
      this.loadMuscleSizes();
      this.modelDeletedSuccessfully("Muscle Size");
    })
  };


  exercises: Exercise[] = [];
  isExercises: boolean =  false;

  exercisePageIndex: number = 0;
  exercisePageSize: number = 10;
  exerciseTotalCount!: number;

  exerciseSortColumn: string = "id";
  exerciseSortOrder: "asc" | "desc" = "asc";
  exerciseFilterColumn?: string;
  exerciseFilterQuery?: string;
    
  loadExercises() {
     this.muscleService.getMuscleExercises(
        this.muscleId,  
        this.exercisePageIndex, 
        this.exercisePageSize, 
        this.exerciseSortColumn, 
        this.exerciseSortOrder, 
        this.exerciseFilterColumn ?? null, 
        this.exerciseFilterQuery ?? null)
      .pipe(this.catchError())
      .subscribe((apiResult: ApiResult<Exercise>) => {
        this.exerciseTotalCount = apiResult.totalCount;
        this.exercisePageIndex = apiResult.pageIndex;
        this.exercisePageSize = apiResult.pageSize;

        this.exercises = apiResult.data;
        this.isExercises = apiResult.data.length !== 0
      });
  }
  
  onExerciseSortChange(event: { sortColumn: string; sortOrder: string }): void {
    this.exerciseSortColumn = event.sortColumn;
    this.exerciseSortOrder = event.sortOrder as 'asc' | 'desc';
    this.loadExercises();
  }

  onExercisePageChange(event: { pageIndex: number; pageSize: number }): void {
    this.exercisePageIndex = event.pageIndex;
    this.exercisePageSize = event.pageSize;
    this.loadExercises();
  }

  onDeleteExercise(id: any): void {
    this.deleteExercise(id);
  }

  deleteExercise(id: number): void {
    var exercise = this.exercises.find(e => e.id == id);

    (exercise!.isCreatedByUser ? 
      this.exerciseService.deleteUserExercise(id) :
      this.exerciseService.deleteInternalExercise(id)
    )
    .pipe(this.catchError())
    .subscribe(() => {
      this.loadExercises();
      this.modelDeletedSuccessfully("Exercise");
    })
  };
} 