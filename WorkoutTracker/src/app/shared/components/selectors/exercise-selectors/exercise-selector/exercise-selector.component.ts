import { Component, EventEmitter, Output, Input, OnInit, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { ExerciseService } from 'src/app/exercises/services/exercise.service';
import { BaseExerciseSelectorComponent } from '../base-exercise-selector.component';

@Component({
  selector: 'app-exercise-selector',
  templateUrl: './exercise-selector.component.html',
  styleUrls: ['./exercise-selector.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ExerciseSelectorComponent),
      multi: true,
    },
  ],
})
export class ExerciseSelectorComponent extends BaseExerciseSelectorComponent<number> {
  @Output() exerciseIdChange = new EventEmitter<number>();
  selectedExerciseId?: number;

  constructor(exerciseService: ExerciseService) {
    super(exerciseService);
  }

  onExerciseIdSelected() {
    this.exerciseIdChange.emit(this.selectedExerciseId);
    this.onChange(this.selectedExerciseId);
    this.onTouched();
  }

  writeValue(value?: number): void {
    this.selectedExerciseId = value;
  }
}
