import { Component, EventEmitter, Input, Output, ViewChild, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';

import { ExerciseSetGroup } from '../../../models/exercise-set-group';
import { ModelWeight } from '../../../models/model-weight';
import { ExerciseService } from 'src/app/exercises/services/exercise.service';
import { BaseEditorComponent } from '../../base-editor.component';
import { WeightType } from 'src/app/shared/models/weight-type';
import { MatAccordion } from '@angular/material/expansion';
import { showCountOfSomethingStr } from 'src/app/shared/helpers/functions/showFunctions/showCountOfSomethingStr';
import { Exercise } from 'src/app/exercises/models/exercise';

@Component({
  selector: 'app-exercise-set-groups-editor',
  templateUrl: './exercise-set-groups-editor.component.html',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => ExerciseSetGroupsEditorComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ExerciseSetGroupsEditorComponent),
      multi: true,
    },
  ],
})
export class ExerciseSetGroupsEditorComponent extends BaseEditorComponent<ExerciseSetGroup[]>{
  @Input() weightTypeValue?: WeightType;
  @ViewChild(MatAccordion) accordion!: MatAccordion;
  
  private isValid: boolean = false;
  @Output() validityChange = new EventEmitter<boolean>();
  
  exerciseSetGroups!: ExerciseSetGroup[];
  private exerciseSetGroupsValidators!: boolean[];

  constructor(private exerciseService: ExerciseService) {
    super();
  }

  private checkExerciseSetGroupsValidation(): boolean {
    return this.exerciseSetGroups && this.exerciseSetGroups.length > 0;
  }

  private updateValue() {
    this.onChange(this.exerciseSetGroups); 
    this.onTouched();
  }

  private emitValidity(): void {
    if(this.exerciseSetGroupsValidators && this.exerciseSetGroupsValidators.some(v => v === false)){ 
      // if at least one exercise set group is invalid
      this.validityChange.emit(false);
    }
    else {
      this.validityChange.emit(this.isValid);
    }
  }

  onExerciseSetGroupValidityChange(isValid: boolean, index: number){
    this.exerciseSetGroupsValidators[index] = isValid;
    this.emitValidity();
  }

  writeValue(value: ExerciseSetGroup[]): void {
    if(value) {
      this.exerciseSetGroups = value;
      this.exerciseSetGroupsValidators = Array(this.exerciseSetGroups.length).fill(false); // all exercise set groups are invalid before validation
      this.isValid = this.checkExerciseSetGroupsValidation();
    }
    else {
      this.exerciseSetGroups = [];
      this.exerciseSetGroupsValidators = [];
      this.isValid = false;
    }

    this.emitValidity();
  }

  onExerciseSetGroupUpdated(updatedGroup: any, index: number) {
    this.exerciseSetGroups[index] = updatedGroup;
    this.updateValue();
  }

  deleteExerciseSetGroup = async (index: number): Promise<void> => {
    if (index !== -1) {
      this.exerciseSetGroups.splice(index, 1);
      this.exerciseSetGroupsValidators.splice(index, 1);

      if(this.exerciseSetGroups.length === 0) {
        // if we deleted the last exercise set group
        this.isValid = false;
        this.emitValidity();
      }

      this.updateValue();
    }
  };

  deleteAllExerciseSetGroups = async (_: number): Promise<void> => {
    this.exerciseSetGroups = [];
    this.exerciseSetGroupsValidators = [];

    this.isValid = false; // there are no exercise set groups
    this.emitValidity();

    this.updateValue();
  };

  isAddingNewExercise: boolean = false;
  addNewExercise(){
    this.isAddingNewExercise = true;
  }

  onExerciseIdSelected(exerciseId: number) {
    this.exerciseService.getExerciseById(exerciseId)
      // .pipe(this.catchError())
      .subscribe((exercise) => {
        this.exerciseSetGroups.push(this.getDefaultExerciseSetGroupByExercise(exercise));
        this.exerciseSetGroupsValidators.push(false); // exercise set group is invalid at first

        this.isValid = true; // there is at least one exercise set group
        this.emitValidity();

        this.isAddingNewExercise = false;
    })
  }

  private getDefaultExerciseSetGroupByExercise(exercise: Exercise): ExerciseSetGroup {
    return <ExerciseSetGroup>{ 
      id: 0,
      exerciseId: exercise.id, 
      exerciseName: exercise.name,
      exerciseType: exercise.type,
      exerciseSets: [],
      sets: 0,
      weight: <ModelWeight>{},
    };
  }

  showCountOfSetsStr(countOfSets: number) {
    return showCountOfSomethingStr(countOfSets, 'set', 'sets');
  }

  showCountOfExercisesStr(countOfExercises: number) {
    return showCountOfSomethingStr(countOfExercises, 'exercise', 'exercises');
  }
  
  expandAll(): void {
    this.accordion.openAll();
  }

  collapseAll(): void {
    this.accordion.closeAll();
  }

  validate() {
    var isValid;
    if(this.exerciseSetGroupsValidators && this.exerciseSetGroupsValidators.some(v => v === false)) { 
      // if at least one exercise set is invalid
      isValid = false;
    }
    else {
      isValid = this.isValid;
    }

    return isValid ? null : { required: true };
  }
}
