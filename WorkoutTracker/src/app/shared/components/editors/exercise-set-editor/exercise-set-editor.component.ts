import { Component, EventEmitter, Input, Output, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';

import { ExerciseType } from '../../../../exercises/models/exercise-type';
import { ExerciseSet } from '../../../models/exercise-set';
import { BaseEditorComponent } from '../../base-editor.component';
import { WeightType } from 'src/app/shared/models/weight-type';
import { TimeSpan } from 'src/app/shared/models/time-span';
import { ModelWeight } from 'src/app/shared/models/model-weight';

@Component({
  selector: 'app-exercise-set-editor',
  templateUrl: './exercise-set-editor.component.html',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => ExerciseSetEditorComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ExerciseSetEditorComponent),
      multi: true,
    },
  ],
})
export class ExerciseSetEditorComponent extends BaseEditorComponent<ExerciseSet> {
  @Input() weightTypeValue?: WeightType;

  private isValid: boolean = false;
  @Output() validityChange = new EventEmitter<boolean>();

  exerciseType = ExerciseType;
  private _exerciseSet!: ExerciseSet;

  ngOnInit(): void {
    if(this.value) {
      this._exerciseSet = this.value;
      this.isValid = this.checkExerciseSetValidation();
    }
    else {
      this._exerciseSet = <ExerciseSet>{};
      this.isValid = false;
    }
    
    this.emitValidity();
  }

  private checkExerciseSetValidation(): boolean {
    if(this.required) {
      switch (this.exerciseSet.exerciseType) {
        case ExerciseType.Reps:
          return this.hasRepsValue(this.exerciseSet.reps);
        case ExerciseType.Time:
          return this.hasTimeValue(this.exerciseSet.time);
        case ExerciseType.WeightAndReps:
          return this.hasRepsValue(this.exerciseSet.reps) && this.hasWeightValue(this.exerciseSet.weight);
        case ExerciseType.WeightAndTime:
          return this.hasTimeValue(this.exerciseSet.time) && this.hasWeightValue(this.exerciseSet.weight);
        default:
          throw new Error(`Unexpected exerciseType value`);
      }
    }

    return true;
  }

  private hasWeightValue(weight: ModelWeight|null){
    return this.hasValue(weight) && (this.hasValue(weight!.weight) && weight!.weight > 0) && this.hasValue(weight!.weightType);
  }

  private hasTimeValue(time: TimeSpan|null){
    return this.hasValue(time) && ((this.hasValue(time!.hours) && time!.hours > 0) || (this.hasValue(time!.minutes) && time!.minutes > 0));
  }

  private hasRepsValue(reps: number|null){
    return this.hasValue(reps) && reps! > 0;
  }

  private hasValue(value: any){
    return value !== null && value !== undefined;
  }

  private emitValidity(): void {
    this.validityChange.emit(this.isValid);
  }

  get exerciseSet(): ExerciseSet {
    return this._exerciseSet;
  }

  set exerciseSet(value: ExerciseSet) {
    this._exerciseSet = value;

    this.onExerciseSetChange();
    this.onChange(value); 
    this.onTouched();
  }

  onExerciseSetChange(){
    this.isValid = this.checkExerciseSetValidation();
    this.emitValidity();
  }

  writeValue(value: ExerciseSet): void {
    this._exerciseSet = value;
  }

  validate() {
    return this.isValid ? null : {required: true};
  }
}