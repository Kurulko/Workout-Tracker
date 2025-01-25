import { Component, EventEmitter, forwardRef, Output } from '@angular/core';
import { ExerciseService } from 'src/app/exercises/services/exercise.service';
import { BaseExerciseSelectorComponent } from '../base-exercise-selector.component';
import { Exercise } from 'src/app/exercises/models/exercise';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-multiple-exercise-selector',
  templateUrl: './multiple-exercise-selector.component.html',
  styleUrls: ['./multiple-exercise-selector.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => MultipleExerciseSelectorComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MultipleExerciseSelectorComponent),
      multi: true,
    },
  ],
})
export class MultipleExerciseSelectorComponent extends BaseExerciseSelectorComponent<Exercise[]> {
  @Output() exercisesChange = new EventEmitter<Exercise[]>();
  
  selectedExercises?: Exercise[];

  constructor(exerciseService: ExerciseService) {
    super(exerciseService);
  }

  onExercisesSelected() {
    var exercises = this.isNoneOptionSelected ? [] : this.selectedExercises;

    this.exercisesChange.emit(exercises);
    this.onChange(exercises);
    this.onTouched();
  }

  writeValue(value?: Exercise[]): void {
    this.selectedExercises = value;
  }

  isDisabledNoneOption(): boolean {
    if(!this.selectedExercises)
      return false;

    return this.selectedExercises.length > 0 && !this.isNoneOptionSelected;
  }

  isNoneOptionSelected = false;
  noneOptionSelected(){
    this.isNoneOptionSelected = !this.isNoneOptionSelected;
  }

  validate() {
    return this.validateItems(this.selectedExercises, this.isNoneOptionSelected);
  }
}
