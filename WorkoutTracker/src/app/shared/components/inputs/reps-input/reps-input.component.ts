import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { BaseInputComponent } from '../base-input.component';

@Component({
  selector: 'app-reps-input',
  templateUrl: './reps-input.component.html',
  styleUrls: ['./reps-input.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => RepsInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => RepsInputComponent),
      multi: true,
    },
  ],
})
export class RepsInputComponent extends BaseInputComponent<number> {
  @Input() hintStr?: string;
  @Input() maxValue?: number;
  @Input() minValue: number = 1;

  ngOnInit() {
    const validators = [];

    if (this.required) {
      validators.push(Validators.required);
    }

    if (this.maxValue) {
      validators.push(Validators.max(this.maxValue));
    }

    validators.push(Validators.min(this.minValue));

    this.internalControl.setValidators(validators);

    if(!this.modelName){
      this.modelName = "Reps";
    }
  }
}
