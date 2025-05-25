import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';

import { ModelsTableComponent } from 'src/app/shared/components/base/models-table.component';
import { ApiResult } from 'src/app/shared/models/api-result';
import { Workout } from './workout';
import { WorkoutService } from './workout.service';
import { TimeSpan } from '../shared/models/time-span';
import { getSomeExercises } from '../shared/helpers/functions/getFunctions/getSomeExercises';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { showCountOfSomethingStr } from '../shared/helpers/functions/showFunctions/showCountOfSomethingStr';
import { showCountOfSomethingLeftStr } from '../shared/helpers/functions/showFunctions/showCountOfSomethingLeftStr';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-workouts',
  templateUrl: './workouts.component.html',
  styleUrls: ['./workouts.component.css']
})
export class WorkoutsComponent extends ModelsTableComponent<Workout> implements OnInit {
  constructor(
    public workoutService: WorkoutService, 
    private dialog: MatDialog, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);

    this.pageSize = 10;
    this.filterColumn = "name";
    this.sortColumn = "isPinned";
    this.sortOrder = "desc";
  }

  currentWorkoutId!: number;
  currentWorkoutTime!: TimeSpan;
  currentWorkoutDate!: Date;

  maxCompleteDate: Date = new Date();

  @ViewChild('completedTemplate') completedTemplate!: TemplateRef<TimeSpan>;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Workout>> {
        return this.workoutService.getWorkouts(null, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  countOfExercisesShown: number = 3;
  
  getSomeExercises = getSomeExercises;
  showCountOfExercisesStr = showCountOfSomethingStr;
  showCountOfExercisesLeftStr = showCountOfSomethingLeftStr;

  ngOnInit() {
    this.loadData();
  }

  deleteItem = async (id: number): Promise<void> => {
    this.workoutService.deleteWorkout(id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.loadData();
        this.modelDeletedSuccessfully("Workout");
      })
  };

  pinWorkout(id: number){
    this.workoutService.pinWorkout(id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.loadData();
        this.operationDoneSuccessfully("Workout", "pinned");
      })
  }

  unpinWorkout(id: number){
    this.workoutService.unpinWorkout(id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.loadData();
        this.operationDoneSuccessfully("Workout", "unpinned");
      })
  }

  openWorkoutCompleteDialog(id: number){
    this.currentWorkoutTime = new TimeSpan();
    this.currentWorkoutDate = new Date();
    this.currentWorkoutId = id;

    this.dialog.open(this.completedTemplate, { width: '300px' });
  }

  completeCurrentWorkout() {
    this.workoutService.completeWorkout(this.currentWorkoutId, this.currentWorkoutDate, this.currentWorkoutTime)
      .pipe(this.catchError())
      .subscribe(() => {
        this.operationDoneSuccessfully("Workout", "completed");
      })
  }

  getData(event: PageEvent) {
    var sortColumn = (this.sort) ? this.sort.active : this.sortColumn;
    var sortOrder = (this.sort) ? this.sort.direction: this.sortOrder;
    var filterColumn = (this.filterQuery) ? this.filterColumn : null;
    var filterQuery = (this.filterQuery) ? this.filterQuery : null;

    this.getModels(event.pageIndex, event.pageSize, sortColumn, sortOrder, filterColumn ?? null, filterQuery)
      .subscribe(result => {
        this.paginator.length = result.totalCount;
        this.paginator.pageIndex = result.pageIndex;
        this.paginator.pageSize = result.pageSize;

        this.data = result.data;
      });
  }
}
