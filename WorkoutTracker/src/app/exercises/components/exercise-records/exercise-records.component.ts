import { Component, OnInit } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';

import { ModelsTableComponent } from 'src/app/shared/components/base/models-table.component';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { ApiResult } from 'src/app/shared/models/api-result';
import { DateTimeRange } from 'src/app/shared/models/date-time-range';
import { ExerciseRecord } from '../../models/exercise-record';
import { ExerciseType } from '../../models/exercise-type';
import { ExerciseRecordService } from '../../services/exercise-record.service';
import { ExerciseService } from '../../services/exercise.service';

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
  
  range: DateTimeRange|null = null;
  maxDate: Date = new Date();

  exerciseId: number|null = null;
  exerciseType: ExerciseType|null = null;

  ngOnInit() {
    this.loadData();
  }

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<ExerciseRecord>> {
      return this.exerciseRecordService.getExerciseRecords(this.exerciseId, this.exerciseType, this.range, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
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