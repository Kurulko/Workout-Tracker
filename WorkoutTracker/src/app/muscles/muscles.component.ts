import { Component, OnInit } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';

import { Muscle } from './muscle';
import { MuscleService } from './muscle.service';
import { ApiResult } from '../shared/models/api-result';
import { ModelsTableComponent } from '../shared/components/base/models-table.component';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-muscles',
  templateUrl: './muscles.component.html',
  styleUrls: ['./muscles.component.css']
})
export class MusclesComponent extends ModelsTableComponent<Muscle> implements OnInit {
  parentMuscleId: number|null = null;

  constructor(public muscleService: MuscleService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.filterColumn = "name";
  }

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Muscle>> {
    return this.muscleService.getMuscles(this.parentMuscleId, null, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  ngOnInit() {
    this.loadData();
  }

  deleteItem(id: number): void {
    this.muscleService.deleteMuscle(id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.loadData();
        this.modelDeletedSuccessfully("Muscle");
      })
  };
}
