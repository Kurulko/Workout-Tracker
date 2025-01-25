import { Component, Input, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { BaseInputComponent } from '../base-input.component';

@Component({
  selector: 'app-description-input',
  templateUrl: './description-input.component.html',
  styleUrls: ['./description-input.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => DescriptionInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DescriptionInputComponent),
      multi: true,
    },
  ],
})
export class DescriptionInputComponent extends BaseInputComponent<string> {
  @Input() hintStr?: string;
  @Input() minLength: number = 10;
  @Input() maxLength?: number;

  ngOnInit() {
    const validators = [];

    if (this.required) {
      validators.push(Validators.required);
    }

    if (this.maxLength) {
      validators.push(Validators.maxLength(this.maxLength));
    }

    validators.push(Validators.minLength(this.minLength));
    this.internalControl.setValidators(validators);

    if(!this.modelName){
      this.modelName = "Description";
    }
  }
}
