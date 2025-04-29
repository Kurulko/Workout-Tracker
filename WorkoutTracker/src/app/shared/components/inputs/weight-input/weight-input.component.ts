import { Component, Input, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { showWeightType } from 'src/app/shared/helpers/functions/showFunctions/showWeightType';
import { showWeightTypeShort } from 'src/app/shared/helpers/functions/showFunctions/showWeightTypeShort';
import { WeightType } from 'src/app/shared/models/weight-type';
import { BaseInputComponent } from '../base-input.component';

@Component({
  selector: 'app-weight-input',
  templateUrl: './weight-input.component.html',
  styleUrls: ['./weight-input.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => WeightInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => WeightInputComponent),
      multi: true,
    },
  ],
})
export class WeightInputComponent extends BaseInputComponent<number> {
  @Input() minValue: number = 1;
  @Input() maxValue: number| null = null;

  @Input() weightType?: WeightType;
  @Input() isShortWeightType: boolean = true;
  @Input() hintStr?: string;
  
  showWeightType = showWeightType;
  showWeightTypeShort = showWeightTypeShort;

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

    if(!this.modelName) {
      this.modelName = "Weight";
    }
  }

  validateWeightInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
  
    const regex = /^\d*\.?\d{0,2}$/;
  
    if (!regex.test(value)) {
      input.value = value.slice(0, -1);
    }
  }
}
