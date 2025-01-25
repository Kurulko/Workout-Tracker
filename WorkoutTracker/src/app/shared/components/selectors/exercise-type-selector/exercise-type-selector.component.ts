import { Component, EventEmitter, Output, OnInit, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';
import { BaseSelectorComponent } from '../base-selector.component';
import { getEnumElements } from 'src/app/shared/helpers/functions/getFunctions/getEnumElements';
import { showExerciseType } from 'src/app/shared/helpers/functions/showFunctions/showExerciseType';
import { ExerciseType } from 'src/app/exercises/models/exercise-type';

@Component({
  selector: 'app-exercise-type-selector',
  templateUrl: './exercise-type-selector.component.html',
  styleUrls: ['./exercise-type-selector.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => ExerciseTypeSelectorComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ExerciseTypeSelectorComponent),
      multi: true,
    },
  ],
})
export class ExerciseTypeSelectorComponent extends BaseSelectorComponent<ExerciseType|null> implements OnInit {
  @Output() exerciseTypeChange = new EventEmitter<ExerciseType|null>();
  
  exerciseTypes = getEnumElements(ExerciseType);
  
  selectedExerciseType: ExerciseType|null = null;

  showExerciseType = showExerciseType;
 
  ngOnInit(): void {
    this.selectedExerciseType = this.value ?? null;
  }

  onExerciseTypeSelected() {
    this.selectedExerciseType = this.selectedExerciseType ?? null;
    this.exerciseTypeChange.emit(this.selectedExerciseType);
    this.onChange(this.selectedExerciseType);
    this.onTouched();
  }

  writeValue(value?: ExerciseType): void {
    this.selectedExerciseType = value ?? null;
  }

  validate() {
    return this.validateEnum(this.selectedExerciseType);
  }
}
