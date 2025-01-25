import { Component, Input, OnInit, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { BaseInputComponent } from '../base-input.component';

@Component({
  selector: 'app-body-fat-percentage-input',
  templateUrl: './body-fat-percentage-input.component.html',
  styleUrls: ['./body-fat-percentage-input.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => BodyFatPercentageInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => BodyFatPercentageInputComponent),
      multi: true,
    },
  ],
})
export class BodyFatPercentageInputComponent extends BaseInputComponent<number> implements OnInit {
  @Input() hintStr?: string;
  
  readonly minValue: number = 1;
  readonly maxValue: number = 100;

  ngOnInit() {
    const validators = [];

    if (this.required) {
      validators.push(Validators.required);
    }

    validators.push(Validators.min(this.minValue), Validators.max(this.maxValue));

    this.internalControl.setValidators(validators);

    if(!this.modelName){
      this.modelName = "Body fat percentage";
    }
  }

  validateBodyFatPercentageInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
  
    const regex = /^\d*\.?\d{0,1}$/;
  
    if (!regex.test(value)) {
      input.value = value.slice(0, -1);
    }
  }
}
// export class BodyFatPercentageInputComponent extends BaseEditorComponent<number> implements Validator {
//   @Input() hintStr?: string;
//   private _bodyFatPercentage?: number;

//   internalControl = new FormControl(null);

//   ngOnInit(): void {
//     this._bodyFatPercentage = this.value;
//     this.modelName = this.modelName ?? "Body fat percentage";
//       const validators = [];
//         if (this.required) {
//           validators.push(Validators.required);
//         }
//         validators.push(Validators.min(1), Validators.max(100));
    
//         this.internalControl.setValidators(validators);
//     }
   
//   get bodyFatPercentage(): number|undefined {
//     return this._bodyFatPercentage;
//   }

//   set bodyFatPercentage(value: number) {
//     this._bodyFatPercentage = value;
//     this.onChange(value); 
//     this.onTouched();
//   }

//   validateBodyFatPercentageInput(event: Event): void {
//     const input = event.target as HTMLInputElement;
//     const value = input.value;
  
//     const regex = /^\d*\.?\d{0,1}$/;
  
//     if (!regex.test(value)) {
//       input.value = value.slice(0, -1);
//     }
//   }

//   validate() {
//     return this.internalControl.valid ? null : this.internalControl.errors;
//   }
//   writeValue(value?: number): void {
//     this._bodyFatPercentage = value;
//   }
// }
