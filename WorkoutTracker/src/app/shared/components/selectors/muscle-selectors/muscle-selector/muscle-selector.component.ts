import { Component, EventEmitter, Output, Input, OnInit, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { MuscleService } from 'src/app/muscles/muscle.service';
import { BaseMuscleSelectorComponent } from '../base-muscle-selector.component';

@Component({
  selector: 'app-muscle-selector',
  templateUrl: './muscle-selector.component.html',
  styleUrls: ['./muscle-selector.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MuscleSelectorComponent),
      multi: true,
    },
  ],
})
export class MuscleSelectorComponent extends BaseMuscleSelectorComponent<number> {
  @Output() muscleIdChange = new EventEmitter<number>();
  selectedMuscleId?: number;

  constructor(muscleService: MuscleService) {
    super(muscleService);
  }

  onMuscleIdSelected() {
    this.muscleIdChange.emit(this.selectedMuscleId);
    this.onChange(this.selectedMuscleId);
    this.onTouched();
  }

  writeValue(value?: number): void {
    this.selectedMuscleId = value;
  }
}
