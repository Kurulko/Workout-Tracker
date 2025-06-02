import { Component } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';

import { Exercise } from '../../models/exercise';
import { ExerciseService } from '../../services/exercise.service';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { ExerciseType } from '../../models/exercise-type';
import { ApiResult } from 'src/app/shared/models/api-result';
import { ModelsTableComponent } from 'src/app/shared/components/base/models-table.component';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-exercises',
  templateUrl: './exercises.component.html',
  styleUrls: ['./exercises.component.css']
})
export class ExercisesComponent extends ModelsTableComponent<Exercise> {
  constructor(public exerciseService: ExerciseService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.sortColumn = 'name';
  }

  ngOnInit(): void {
    this.loadData();
  }

  exercisePageType: "all"|"yours"|"internal"|"used" = "all";
  onExerciseTabChange(event: any): void {
    const index = event.index;

    if (index === 0) 
      this.exercisePageType = 'all';
    else if (index === 1) 
      this.exercisePageType = 'used';
    else if (index === 2) 
      this.exercisePageType = 'yours';
    else if (index === 3) 
      this.exercisePageType = 'internal';

    this.loadData();
  }

  exerciseType:ExerciseType|null = null;
  getModels(pageIndex: number, pageSize: number, sortColumn: string, sortOrder: string, _: string|null, filterQuery: string|null): Observable<ApiResult<Exercise>> {
    switch (this.exercisePageType) {
      case 'all':
        return this.exerciseService.getAllExercises(this.exerciseType, pageIndex, pageSize, sortColumn, sortOrder, filterQuery);
      case 'yours':
        return this.exerciseService.getUserExercises(this.exerciseType, pageIndex, pageSize, sortColumn, sortOrder, filterQuery);
      case 'internal':
        return this.exerciseService.getInternalExercises(this.exerciseType, pageIndex, pageSize, sortColumn, sortOrder, filterQuery);
      case 'used':
        return this.exerciseService.getUsedExercises(this.exerciseType, pageIndex, pageSize, sortColumn, sortOrder, filterQuery);
      default:
        throw new Error(`Unexpected exercise type page value`);
      }
  }

  deleteItem(id: number): void {
    var exercise = this.data.find(e => e.id == id);

    (exercise!.isCreatedByUser ? 
      this.exerciseService.deleteUserExercise(id) :
      this.exerciseService.deleteInternalExercise(id)
    )
    .pipe(this.catchError())
    .subscribe(() => {
      this.loadData();
      this.modelDeletedSuccessfully("Exercise");
    })
  };
}
