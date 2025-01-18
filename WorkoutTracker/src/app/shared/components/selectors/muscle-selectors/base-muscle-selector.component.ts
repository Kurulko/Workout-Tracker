import { Component, Input, OnInit } from '@angular/core';
import { map, Observable } from 'rxjs';
import { Muscle } from 'src/app/muscles/muscle';
import { MuscleService } from 'src/app/muscles/muscle.service';
import { ModelsSelectorComponent } from '../models-selector.component';
import { environment } from 'src/environments/environment.prod';

@Component({
  template: ''
})
export abstract class BaseMuscleSelectorComponent<T extends number|Muscle[]> extends ModelsSelectorComponent<T> implements OnInit {
  @Input()
  parentMuscleId:number|null = null;

  @Input()
  isMeasurable:boolean|null = null;

  muscles!: Observable<Muscle[]>;

  constructor(private muscleService: MuscleService) {
    super();
  }
  
  envProduction = environment;

  loadData(){
    this.muscles = this.muscleService
      .getMuscles(this.parentMuscleId, this.isMeasurable, this.pageIndex, this.pageSize, this.sortColumn, this.sortOrder, this.filterColumn, this.filterQuery)
      .pipe(map(x => x.data));
  }
}