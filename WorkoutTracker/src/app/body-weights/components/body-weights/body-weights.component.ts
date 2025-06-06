import { Component, OnInit } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';

import { BodyWeightService } from '../../services/body-weight.service';
import { ApiResult } from '../../../shared/models/api-result';
import { ModelsTableComponent } from '../../../shared/components/base/models-table.component';
import { PreferencesManager } from '../../../shared/helpers/managers/preferences-manager';
import { WeightType } from '../../../shared/models/enums/weight-type';
import { ImpersonationManager } from '../../../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../../../shared/helpers/managers/token-manager';
import { DateTimeRange } from '../../../shared/models/date-time-range';
import { BodyWeight } from '../../models/body-weight';

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
  
  range: DateTimeRange|null = null;
  maxDate: Date = new Date();

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<BodyWeight>> {
    if(this.weightType === 'kg')
      return this.bodyWeightService.getBodyWeightsInKilograms(this.range, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
    else
      return this.bodyWeightService.getBodyWeightsInPounds(this.range, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  ngOnInit() {
    if(this.preferableWeightType != undefined)
      this.weightType = (this.preferableWeightType == WeightType.Kilogram) ? "kg" : "lbs";

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
