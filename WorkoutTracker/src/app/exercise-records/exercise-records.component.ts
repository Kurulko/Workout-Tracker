import { Component, OnInit } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';

import { ApiResult } from '../shared/models/api-result';
import { ModelsTableComponent } from '../shared/components/base/models-table.component';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { ExerciseRecord } from './exercise-record';
import { ExerciseRecordService } from './exercise-record.service';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';
import { ExerciseType } from '../exercises/models/exercise-type';
import { ExerciseService } from '../exercises/services/exercise.service';

@Component({
  selector: 'app-exercise-records',
  templateUrl: './exercise-records.component.html',
  styleUrls: ['./exercise-records.component.css']
})
export class ExerciseRecordsComponent extends ModelsTableComponent<ExerciseRecord> implements OnInit {
  constructor( 
    private exerciseRecordService: ExerciseRecordService, 
    private exerciseService: ExerciseService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);

    this.sortColumn = "date";
    this.sortOrder = "desc";
  }
  
  date: Date|null = null;
  maxDate: Date = new Date();

  exerciseId: number|null = null;
  exerciseType: ExerciseType|null = null;

  ngOnInit() {
    this.loadData();
  }

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<ExerciseRecord>> {
      return this.exerciseRecordService.getExerciseRecords(this.exerciseId, this.exerciseType, this.date, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  clearDate(){
    this.date = null;
    this.loadData();
  }

  deleteItem(id: number) {
    this.exerciseRecordService.deleteExerciseRecord(id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.loadData();
        this.modelDeletedSuccessfully("Exercise Record");
      })
  };

  onExerciseSelected() {
    if(this.exerciseId){
      this.exerciseService.getExerciseById(this.exerciseId)
        .pipe(this.catchError())
        .subscribe(exercise => {
          this.exerciseType = exercise.type;
        });
    }
    this.loadData();
  }

  onExerciseTypeSelected() {
    this.exerciseId = null;
    this.loadData();
  }
}