import { Component, OnInit } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';

import { MuscleSize } from './muscle-size';
import { MuscleSizeService } from './muscle-size.service';
import { ApiResult } from '../shared/models/api-result';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';
import { SizeType } from '../shared/models/size-type';
import { roundNumber } from '../shared/helpers/functions/roundNumber';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { ModelsTableComponent } from '../shared/components/base/models-table.component';
import { MuscleService } from '../muscles/muscle.service';
import { Muscle } from '../muscles/muscle';
import { environment } from 'src/environments/environment.prod';
import { DateTimeRange } from '../shared/models/date-time-range';

@Component({
  selector: 'app-muscle-sizes',
  templateUrl: './muscle-sizes.component.html',
  styleUrls: ['./muscle-sizes.component.css']
})
export class MuscleSizesComponent extends ModelsTableComponent<MuscleSize> implements OnInit {
  constructor(
    private muscleSizeService: MuscleSizeService, 
    private muscleService: MuscleService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.sortColumn = "date";
    this.sortOrder = "desc";
  }

  sizeType: "cm" | "inches" = "cm";
  muscleId: number|null = null;
  range: DateTimeRange|null = null;
  maxDate: Date = new Date();

  envProduction = environment;
  
  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<MuscleSize>> {
    if(this.sizeType === 'cm')
      return this.muscleSizeService.getMuscleSizesInCentimeters(this.muscleId, this.range, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
    else
      return this.muscleSizeService.getMuscleSizesInInches(this.muscleId, this.range, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  ngOnInit() {
    if(this.preferencesManager.hasPreferableSizeType())
      this.sizeType = this.preferencesManager.getPreferableSizeType() === SizeType.Centimeter ? "cm" : "inches";
    
    this.loadData();
  }
  
  roundSize = roundNumber;

  muscle: Muscle|null = null;
  onMuscleIdSelected() {
    if(this.muscleId) {
      this.muscleService.getMuscleById(this.muscleId!)
        .pipe(this.catchError())
        .subscribe(result => {
          this.muscle = result;
        });
    }
    else {
      this.muscle = null;
    }
    this.loadData();
  }

  deleteItem(id: number): void {
    this.muscleSizeService.deleteMuscleSize(id)
      .pipe(this.catchError())
      .subscribe(() => {
        this.loadData();
        this.modelDeletedSuccessfully("Muscle Size");
      })
  };
}
