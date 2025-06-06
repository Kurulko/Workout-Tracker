import { Component, EventEmitter, Output, Input, OnInit, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';
import { map, Observable } from 'rxjs';
import { ModelsSelectorComponent } from '../models-selector.component';
import { Workout } from 'src/app/workouts/models/workout';
import { WorkoutService } from 'src/app/workouts/services/workout.service';

@Component({
  selector: 'app-workout-selector',
  templateUrl: './workout-selector.component.html',
  styleUrls: ['./workout-selector.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => WorkoutSelectorComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => WorkoutSelectorComponent),
      multi: true,
    },
  ],
})
export class WorkoutSelectorComponent extends ModelsSelectorComponent<number> implements OnInit {
  @Input()
  exerciseId:number|null = null;

  @Output() workoutIdChange = new EventEmitter<number>();
  
  selectedWorkoutId?: number;
  workouts!: Observable<Workout[]>;

  constructor(private workoutService: WorkoutService) {
    super();
  }

  loadData(){
    this.workouts = this.workoutService.getWorkouts(this.exerciseId, this.pageIndex, this.pageSize, this.sortColumn, this.sortOrder, this.filterColumn, this.filterQuery).pipe(map(x => x.data));
  }

  onWorkoutIdSelected() {
    this.workoutIdChange.emit(this.selectedWorkoutId);
    this.onChange(this.selectedWorkoutId);
    this.onTouched();
  }

  writeValue(value?: number): void {
    this.selectedWorkoutId = value;
  }

  validate() {
    return this.validateItemId(this.selectedWorkoutId)
  }
}
