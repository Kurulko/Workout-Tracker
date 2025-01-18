import { Component, OnInit } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';

import { BodyWeight } from './body-weight';
import { BodyWeightService } from './body-weight.service';
import { ApiResult } from '../shared/models/api-result';
import { ModelsTableComponent } from '../shared/components/base/models-table.component';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';
import { WeightType } from '../shared/models/weight-type';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';

@Component({
  selector: 'app-body-weights',
  templateUrl: './body-weights.component.html',
  styleUrls: ['./body-weights.component.css']
})
export class BodyWeightsComponent extends ModelsTableComponent<BodyWeight> implements OnInit {
  constructor(
    public bodyWeightService: BodyWeightService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);

    this.displayedColumns = ['date', 'weight', 'actions'];
    this.sortColumn = "date";
    this.sortOrder = "desc";
  }

  weightType: "kg" | "lbs" = "kg";
  
  date: Date|null = null;
  maxDate: Date = new Date();

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<BodyWeight>> {
    if(this.weightType === 'kg')
      return this.bodyWeightService.getBodyWeightsInKilograms(this.date, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
    else
      return this.bodyWeightService.getBodyWeightsInPounds(this.date, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  ngOnInit() {
    if(this.preferableWeightType != undefined)
      this.weightType = (this.preferableWeightType == WeightType.Kilogram) ? "kg" : "lbs";

    this.loadData();
  }
  
  clearDate(){
    this.date = null;
    this.loadData();
  }

  deleteItem(id: number) {
    this.bodyWeightService.deleteBodyWeight(id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.loadData();
        this.modelDeletedSuccessfully("Body Weight");
      })
  };
}
