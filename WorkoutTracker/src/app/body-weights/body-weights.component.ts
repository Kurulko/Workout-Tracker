import { Component } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { StatusCodes } from 'http-status-codes';

import { BodyWeight } from './body-weight';
import { BodyWeightService } from './body-weight.service';
import { ApiResult } from '../shared/models/api-result.model';
import { ModelsTableComponent } from '../shared/components/models-table.component';

@Component({
  selector: 'app-body-weights',
  templateUrl: './body-weights.component.html',
  styleUrls: ['./body-weights.component.css']
})
export class BodyWeightsComponent extends ModelsTableComponent<BodyWeight> {
  constructor(public bodyWeightService: BodyWeightService, snackBar: MatSnackBar) {
      super(snackBar);
      this.displayedColumns = ['id', 'date', 'weight', 'weightType', 'actions'];
  }

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<BodyWeight>> {
    return this.bodyWeightService.getBodyWeights(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  deleteBodyWeight(id: number) {
    this.bodyWeightService.deleteBodyWeight(id)
    .pipe(this.catchError())
    .subscribe(() => {
          this.loadData();
          this.modelDeletedSuccessfully("BodyWeight");
        })
  }
}
