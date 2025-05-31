import { Component, EventEmitter, Output, Input, OnInit, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MuscleService } from 'src/app/muscles/services/muscle.service';
import { BaseMuscleSelectorComponent } from '../base-muscle-selector.component';
import { Muscle } from 'src/app/muscles/models/muscle';

@Component({
  selector: 'app-multiple-muscle-selector',
  templateUrl: './multiple-muscle-selector.component.html',
  styleUrls: ['./multiple-muscle-selector.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => MultipleMuscleSelectorComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MultipleMuscleSelectorComponent),
      multi: true,
    },
  ],
})
export class MultipleMuscleSelectorComponent extends BaseMuscleSelectorComponent<Muscle[]> {
  @Output() musclesChange = new EventEmitter<Muscle[]>();
  
  selectedMuscles?: Muscle[];

  constructor(muscleService: MuscleService) {
    super(muscleService);
  }

  onMusclesSelected() {
    var muscles = this.isNoneOptionSelected ? [] : this.selectedMuscles;
    this.musclesChange.emit(muscles);
    this.onChange(muscles);
    this.onTouched();
  }

  writeValue(value?: any): void {
    this.selectedMuscles = value;
  }

  isDisabledNoneOption(): boolean {
    if(!this.selectedMuscles)
      return false;

    return this.selectedMuscles.length > 0 && !this.isNoneOptionSelected;
  }

  isNoneOptionSelected = false;
  noneOptionSelected(){
    this.isNoneOptionSelected = !this.isNoneOptionSelected;
  }  

  validate() {
    return this.validateItems(this.selectedMuscles, this.isNoneOptionSelected);
  }
}
