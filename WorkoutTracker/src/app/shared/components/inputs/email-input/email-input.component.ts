import { Component, Input, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { BaseInputComponent } from '../base-input.component';

@Component({
  selector: 'app-email-input',
  templateUrl: './email-input.component.html',
  styleUrls: ['./email-input.component.css'],
  providers: [
     {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => EmailInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => EmailInputComponent),
      multi: true,
    },
  ],
})
export class EmailInputComponent extends BaseInputComponent<string> {
  @Input() hintStr?: string;

  ngOnInit() {
    const validators = [];

    if (this.required) {
      validators.push(Validators.required);
    }

    validators.push(Validators.email);

    this.internalControl.setValidators(validators);

    if(!this.modelName) {
      this.modelName = "Email";
    }
  }
}
