import { Component, EventEmitter, Input, Output, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';

import { ExerciseType } from '../../../../exercises/models/exercise-type';
import { TimeSpan } from '../../../models/time-span';
import { ModelWeight } from '../../../models/model-weight';
import { ExerciseSetGroup } from '../../../models/exercise-set-group';
import { ExerciseSet } from '../../../models/exercise-set';
import { BaseEditorComponent } from '../../base-editor.component';
import { WeightType } from 'src/app/shared/models/weight-type';

@Component({
    selector: 'app-exercise-sets-edit',
    templateUrl: './exercise-sets-editor.component.html',
    providers: [
        {
            provide: NG_VALIDATORS,
            useExisting: forwardRef(() => ExerciseSetsEditorComponent),
            multi: true,
        },
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => ExerciseSetsEditorComponent),
            multi: true,
        },
    ],
})
export class ExerciseSetsEditorComponent extends BaseEditorComponent<ExerciseSetGroup> {
    @Input() weightTypeValue?: WeightType;

    private isValid: boolean = false;
    @Output() validityChange = new EventEmitter<boolean>();
  
    private exerciseSetGroup!: ExerciseSetGroup;
    
    exerciseSets!: ExerciseSet[];
    private exerciseSetsValidators!: boolean[];

    displayedColumns: string[] = ['set', 'value', 'actions'];

    ngOnInit(): void {
        if(this.value) {
            this.exerciseSetGroup = this.value;

            if(this.exerciseSetGroup.exerciseSets) {
                this.exerciseSets = this.exerciseSetGroup.exerciseSets;
                this.exerciseSetsValidators = Array(this.exerciseSets.length).fill(false); // all exercise sets are invalid before validation
                this.isValid = this.checkExerciseSetsValidation();
            }
            else {
                this.exerciseSets = []
                this.exerciseSetsValidators = [];
                this.isValid = false;
            }
            this.isValid = this.checkExerciseSetsValidation();
        }
        else {
            this.exerciseSetGroup = <ExerciseSetGroup>{};
            this.exerciseSets = []
            this.exerciseSetsValidators = [];
            this.isValid = false;
        }
        
        this.emitValidity();
    }
 
    private checkExerciseSetsValidation(): boolean {
        return this.exerciseSets && this.exerciseSets.length > 0;
    }

    private updateValue() {
      this.emitValidity();
      this.exerciseSetGroup.exerciseSets = this.exerciseSets;
      this.onChange(this.exerciseSetGroup); 
      this.onTouched();
    }

    private emitValidity(): void {
        if(this.exerciseSetsValidators && this.exerciseSetsValidators.some(v => v === false)){ 
            // if at least one exercise set is invalid
            this.validityChange.emit(false);
        }
        else {
            this.validityChange.emit(this.isValid);
        }
    }

    onExerciseSetValidityChange(isValid: boolean, index: number){
        this.exerciseSetsValidators[index] = isValid;
        this.emitValidity();
    }

    writeValue(value: ExerciseSetGroup): void {
        this.exerciseSetGroup = value;
    }

    addNewExerciseSet() {
        var exerciseSet = this.exerciseSets.length === 0 ? this.addFirstExerciseSet() : this.addLastExerciseSet();
        this.exerciseSets = [...this.exerciseSets, exerciseSet];

        this.exerciseSetsValidators.push(false); // exercise set is invalid at first

        this.isValid = true;
        this.updateValue();
    }

    private addFirstExerciseSet(): ExerciseSet {
        var exerciseSet = <ExerciseSet>{ 
            exerciseId : this.exerciseSetGroup.exerciseId, 
            exerciseType : this.exerciseSetGroup.exerciseType, 
            exerciseName : this.exerciseSetGroup.exerciseName 
        };

        if(exerciseSet.exerciseType == ExerciseType.Time || exerciseSet.exerciseType == ExerciseType.WeightAndTime)
            exerciseSet.time = <TimeSpan>{};
        if(exerciseSet.exerciseType == ExerciseType.WeightAndReps || exerciseSet.exerciseType == ExerciseType.WeightAndTime)
            exerciseSet.weight = <ModelWeight>{};

        return exerciseSet;
    }

    private addLastExerciseSet(): ExerciseSet{
        var exerciseSet = <ExerciseSet>{};

        var lastExerciseSet = this.exerciseSets[this.exerciseSets.length - 1];
        exerciseSet = {...lastExerciseSet};
        exerciseSet.weight = <ModelWeight>{...lastExerciseSet.weight};
        exerciseSet.time = <TimeSpan>{...lastExerciseSet.time};
        // exerciseSet.id = 0;

        return exerciseSet;
    }

    deleteExerciseSet = async (index: number): Promise<void> => {
        if (index !== -1) {
            this.exerciseSets.splice(index, 1);
            this.exerciseSets = [...this.exerciseSets];
            this.exerciseSetsValidators.splice(index, 1);

            if(this.exerciseSets.length === 0) {
                // if we deleted the last exercise set
                this.isValid = false;
                this.emitValidity();
            }

            this.updateValue();
        }
    };

    deleteAllExerciseSets = async (_: number): Promise<void> => {
        this.exerciseSets = [];
        this.exerciseSetsValidators = [];
        
        this.isValid = false; // there are no exercise sets
        this.emitValidity();

        this.updateValue();
    };

    onExerciseSetUpdated(updatedSet: any, index: number) {
        this.exerciseSets[index] = updatedSet;
        this.updateValue();
    }

    validate() {
        var isValid;
        if(this.exerciseSetsValidators && this.exerciseSetsValidators.some(v => v === false)){ 
            // if at least one exercise set is invalid
            isValid = false;
        }
        else {
            isValid = this.isValid;
        }

        return isValid ? null : { required: true };
    }
}