import { Component, OnInit } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';

import { Muscle } from './muscle';
import { MuscleService } from './muscle.service';
import { ApiResult } from '../shared/models/api-result.model';
import { ModelsTableComponent } from '../shared/components/models-table.component';

@Component({
  selector: 'app-muscles',
  templateUrl: './muscles.component.html',
  styleUrls: ['./muscles.component.css']
})
export class MusclesComponent extends ModelsTableComponent<Muscle> implements OnInit {
  constructor(public muscleService: MuscleService, snackBar: MatSnackBar) {
      super(snackBar);
      this.displayedColumns = ['name', 'parentMuscle', 'childMuscles', 'actions'];
  }

  getChildrenMuscleNames(muscle: Muscle): string|undefined {
    return muscle.childMuscles?.map(child => child.name).join(', ');
  }

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Muscle>> {
    return this.muscleService.getMuscles(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
  }

  ngOnInit() {
    this.loadData();
  }

  deleteMuscle(id: number) {
    this.muscleService.deleteMuscle(id)
    .pipe(this.catchError())
    .subscribe(() => {
          this.loadData();
          this.modelDeletedSuccessfully("Muscle");
        })
  }
}
