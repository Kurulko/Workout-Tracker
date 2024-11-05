import { Component, OnInit } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { catchError, map } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { StatusCodes } from 'http-status-codes';

import { MuscleSize } from './muscle-size';
import { MuscleSizeService } from './muscle-size.service';
import { ApiResult } from '../shared/models/api-result.model';
import { ModelsTableComponent } from '../shared/components/models-table.component';
import { MuscleService } from '../muscles/muscle.service';
import { Muscle } from '../muscles/muscle';

@Component({
  selector: 'app-muscle-sizes',
  templateUrl: './muscle-sizes.component.html',
  styleUrls: ['./muscle-sizes.component.css']
})
export class MuscleSizesComponent extends ModelsTableComponent<MuscleSize> implements OnInit {
  constructor(public muscleSizeService: MuscleSizeService, snackBar: MatSnackBar, private muscleService: MuscleService) {
      super(snackBar);

      this.displayedColumns = ['date', 'size', 'actions'];
      this.defaultSortColumn = "date";
      this.defaultSortOrder = "desc";

      this.loadMuscles();
  }

  sizeType: "cm" | "inches" = "cm";
  muscleId!:number;

  muscles!: Observable<Muscle[]>;

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<MuscleSize>> {
    if(this.sizeType === 'cm')
      return this.muscleSizeService.getMuscleSizesInCentimeters(this.muscleId, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
    else
      return this.muscleSizeService.getMuscleSizesInInches(this.muscleId, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  ngOnInit() {
    this.loadMuscles();
  }

  onSelectedMuscle(){
    console.log(this.muscleId)
    this.loadData();
  }
  loadMuscles(){
    this.muscles = this.muscleService.getMuscles(0, 9999, "name", "asc", null, null).pipe(map(x => x.data));
  }

  deleteMuscleSize(id: number) {
    this.muscleSizeService.deleteMuscleSize(id)
    .pipe(this.catchError())
    .subscribe(() => {
          this.loadData();
          this.modelDeletedSuccessfully("Muscle Size");
        })
  }
}
