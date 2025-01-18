import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';

import { WorkoutRecord } from './workout-record';
import { WorkoutRecordService } from './workout-record.service';
import { ApiResult } from '../shared/models/api-result';
import { ModelsTableComponent } from '../shared/components/base/models-table.component';
import { showWeightTypeShort } from '../shared/helpers/functions/showFunctions/showWeightTypeShort';
import { showTime } from '../shared/helpers/functions/showFunctions/showTime';
import { getSomeExercises } from '../shared/helpers/functions/getFunctions/getSomeExercises';
import { roundNumber } from '../shared/helpers/functions/roundNumber';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { showCountOfSomethingLeftStr } from '../shared/helpers/functions/showFunctions/showCountOfSomethingLeftStr';
import { showCountOfSomethingStr } from '../shared/helpers/functions/showFunctions/showCountOfSomethingStr';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-workout-records',
  templateUrl: './workout-records.component.html',
  styleUrls: ['./workout-records.component.css']
})
export class WorkoutRecordsComponent extends ModelsTableComponent<WorkoutRecord> implements OnInit {
  constructor(
    private activatedRoute: ActivatedRoute, 
    public workoutRecordService:  WorkoutRecordService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);

    this.pageSize = 5;
    this.sortColumn = "date";
    this.sortOrder = "desc";
  }

  workoutId?: number;

  date: Date|null = null;
  maxDate: Date = new Date();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  
  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<WorkoutRecord>> {
    return this.workoutRecordService.getWorkoutRecords(this.workoutId ?? null, this.date, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  showWeightTypeShort = showWeightTypeShort;
  showTime = showTime;
  roundWeight = roundNumber;

  countOfExercisesShown: number = 3;
  
  getSomeExercises = getSomeExercises;
  showCountOfExercisesStr = showCountOfSomethingStr;
  showCountOfExercisesLeftStr = showCountOfSomethingLeftStr;

  ngOnInit() {
    var idParam = this.activatedRoute.snapshot.paramMap.get('workoutId');
    if (idParam) {
      this.workoutId = +idParam;
    }

    this.loadData();
  }

  clearDate(){
    this.date = null;
    this.loadData();
  }

  dataSource!: MatTableDataSource<WorkoutRecord>;

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

        this.dataSource = new MatTableDataSource<WorkoutRecord>(result.data);
      });
  }

  deleteItem = async (id: number): Promise<void> => {
    this.workoutRecordService.deleteWorkoutRecord(id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.loadData();
        this.modelDeletedSuccessfully("Workout Record");
      })
  };
}