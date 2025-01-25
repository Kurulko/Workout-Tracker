import { Component, Input, forwardRef } from '@angular/core';
import { FormControl, NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { BaseEditorComponent } from '../../base-editor.component';
import { BaseInputComponent } from '../base-input.component';

@Component({
  selector: 'app-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.css'],
  providers: [
     {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => DateInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DateInputComponent),
      multi: true,
    },
  ],
})
export class DateInputComponent extends BaseInputComponent<Date> {
  @Input() minDate?: Date;
  @Input() maxDate?: Date;
  @Input() hintStr?: string;

  ngOnInit() {
    const validators = [];

    if (this.required) {
      validators.push(Validators.required);
    }

    this.internalControl.setValidators(validators);

    if(!this.modelName)
      this.modelName = "Date";
  }
}