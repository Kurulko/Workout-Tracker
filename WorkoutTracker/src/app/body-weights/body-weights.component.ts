import { Component, OnInit } from '@angular/core';
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
export class BodyWeightsComponent extends ModelsTableComponent<BodyWeight> implements OnInit {
  constructor(public bodyWeightService: BodyWeightService, snackBar: MatSnackBar) {
      super(snackBar);

      this.displayedColumns = ['date', 'weight', 'actions'];
      this.defaultSortColumn = "date";
      this.defaultSortOrder = "desc";
  }

  weightType: "kg" | "lbs" = "kg";

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<BodyWeight>> {
    if(this.weightType === 'kg')
      return this.bodyWeightService.getBodyWeightsInKilograms(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
    else
      return this.bodyWeightService.getBodyWeightsInPounds(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  onToggleChange(): void {
    this.loadData();
  }
  
  ngOnInit() {
    this.loadData();
  }

  deleteBodyWeight(id: number) {
    this.bodyWeightService.deleteBodyWeight(id)
    .pipe(this.catchError())
    .subscribe(() => {
          this.loadData();
          this.modelDeletedSuccessfully("Body Weight");
        })
  }
}
