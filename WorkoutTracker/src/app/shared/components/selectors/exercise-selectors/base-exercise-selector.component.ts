import { Component, Input, OnInit } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ExerciseService } from 'src/app/exercises/services/exercise.service';
import { ExerciseType } from 'src/app/exercises/models/exercise-type';
import { ModelsSelectorComponent } from '../models-selector.component';
import { Exercise } from 'src/app/exercises/models/exercise';
import { environment } from 'src/environments/environment.prod';

@Component({
  template: ''
})
export abstract class BaseExerciseSelectorComponent<T extends number|Exercise[]> extends ModelsSelectorComponent<T> implements OnInit {
  @Input()
  modelsType:"all"|"user"|"internal" = "all";

  @Input()
  exerciseType:ExerciseType|null = null;

  exercises!: Observable<Exercise[]>;

  constructor(private exerciseService: ExerciseService) {
    super();
  }

  envProduction = environment;

  loadData(){
    this.exercises = 
      (() => {
        switch (this.modelsType) {
          case "user":
            return this.exerciseService.getUserExercises(this.exerciseType, this.pageIndex, this.pageSize, this.sortColumn, this.sortOrder, this.filterQuery);
          case "internal":
            return this.exerciseService.getInternalExercises(this.exerciseType, this.pageIndex, this.pageSize, this.sortColumn, this.sortOrder, this.filterQuery);
          default:
            return this.exerciseService.getAllExercises(this.exerciseType, this.pageIndex, this.pageSize, this.sortColumn, this.sortOrder, this.filterQuery);
        }
      })().pipe(map(x => x.data));
  }
}