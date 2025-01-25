import { Component, AfterViewInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { EquipmentDetails } from './equipment-details';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { MatSnackBar } from '@angular/material/snack-bar';
import { EquipmentService } from './equipment.service';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { StatusCodes } from 'http-status-codes';
import { ApiResult } from '../shared/models/api-result';
import { Exercise } from '../exercises/models/exercise';
import { showExerciseType } from '../shared/helpers/functions/showFunctions/showExerciseType';
import { MainComponent } from '../shared/components/base/main.component';
import { ExerciseService } from '../exercises/services/exercise.service';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';
import { environment } from 'src/environments/environment.prod';
import { showValuesStr } from '../shared/helpers/functions/showFunctions/showValuesStr';

@Component({
  selector: 'app-equipment-details',
  templateUrl: './equipment-details.component.html',
})
export class EquipmentDetailsComponent extends MainComponent implements AfterViewInit  {
  equipmentDetails!: EquipmentDetails;
  equipmentId!: number;

  constructor(private activatedRoute: ActivatedRoute, 
    private router: Router,
    private exerciseService: ExerciseService, 
    private equipmentService: EquipmentService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
      super(impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  equipmentPageType!: "yours"|"internal";
  envProduction = environment;
  
  readonly musclePanelOpenState = signal(false);

  ngAfterViewInit(): void {
    const fullPath = this.router.url;
    
    if(fullPath.startsWith('/your-equipment'))
      this.equipmentPageType = "yours";
    else
      this.equipmentPageType = "internal";

    this.loadEquipmentDetails();
    this.loadExercises();
  } 

   getMuscleNamesStr(): string {
    if(!this.equipmentDetails.muscles)
      return '';
    
    var muscleNames = this.equipmentDetails.muscles.map(muscle => muscle.name);
    const maxLength = 100;
    return showValuesStr(muscleNames, maxLength);
  }

  hasMuscles: boolean = false; 
  loadEquipmentDetails() {
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.equipmentId = idParam ? +idParam : 0;
    if (this.equipmentId) {
      // Edit mode
      (this.equipmentPageType === 'yours' ? 
        this.equipmentService.getUserEquipmentDetailsById(this.equipmentId) :
        this.equipmentService.getInternalEquipmentDetailsById(this.equipmentId)
      )
     .pipe(catchError((errorResponse: HttpErrorResponse) => {
        console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);

        if (errorResponse.status === StatusCodes.NOT_FOUND) {
            this.router.navigate(['equipments']);
        }

        this.showSnackbar(errorResponse.message);
        return throwError(() => errorResponse);
      }))
      .subscribe((result: EquipmentDetails) => {
          this.equipmentDetails = result;
          this.hasMuscles = result.muscles !== null && result.muscles.length > 0;
      });
    } 
    else {
      this.router.navigate(['/equipments']);
    }
  }

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Exercise>> {
      return this.equipmentService.getEquipmentExercises(this.equipmentId, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  showExerciseType = showExerciseType;
  
  exercises: Exercise[] = [];
  isExercises: boolean =  false;

  pageExerciseIndex: number = 0;
  pageExerciseSize: number = 10;
  totalExerciseCount!: number;

  sortExerciseColumn: string = "name";
  sortExerciseOrder: "asc" | "desc" = "asc";
  filterExerciseColumn?: string;
  filterExerciseQuery?: string;
    
  displayedExerciseColumns!: string[];

  loadExercises() {
    this.getModels(this.pageExerciseIndex, 
        this.pageExerciseSize, 
        this.sortExerciseColumn, 
        this.sortExerciseOrder, 
        this.filterExerciseColumn ?? null, 
        this.filterExerciseQuery ?? null)
      .pipe(this.catchError())
      .subscribe((apiResult: ApiResult<Exercise>) => {
        this.totalExerciseCount = apiResult.totalCount;
        this.pageExerciseIndex = apiResult.pageIndex;
        this.pageExerciseSize = apiResult.pageSize;

        this.exercises = apiResult.data;
        this.isExercises = apiResult.data.length !== 0
      });
  }
  
  onExerciseSortChange(event: { sortColumn: string; sortOrder: string }): void {
    this.sortExerciseColumn = event.sortColumn;
    this.sortExerciseOrder = event.sortOrder as 'asc' | 'desc';
    this.loadExercises();
  }

  onExercisePageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageExerciseIndex = event.pageIndex;
    this.pageExerciseSize = event.pageSize;
    this.loadExercises();
  }

  onDeleteExercise(id: any): void {
    this.deleteExercise(id);
  }

  deleteExercise(id: number) {
    var exercise = this.exercises.find(e => e.id == id)!;

    (exercise.isCreatedByUser ? 
      this.exerciseService.deleteUserExercise(id) :
      this.exerciseService.deleteInternalExercise(id)
    )
      .pipe(this.catchError())
      .subscribe(() => {
        this.modelDeletedSuccessfully("Exercise");
      })
  }

  deleteEquipment() {
    (this.equipmentPageType === 'yours' ? 
      this.equipmentService.deleteUserEquipment(this.equipmentDetails.equipment.id) :
      this.equipmentService.deleteInternalEquipment(this.equipmentDetails.equipment.id)
    )
    .pipe(this.catchError())
    .subscribe(() => {
      this.modelDeletedSuccessfully("Equipment");
      this.router.navigate(['equipments']);
    })
  }
}