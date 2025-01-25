import { Component, Input, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { BaseInputComponent } from '../base-input.component';

@Component({
  selector: 'app-name-input',
  templateUrl: './name-input.component.html',
  styleUrls: ['./name-input.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => NameInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => NameInputComponent),
      multi: true,
    },
  ],
})
export class NameInputComponent extends BaseInputComponent<string> {
  @Input() pattern: string|RegExp = '';
  @Input() minLength: number = 3;
  @Input() maxLength?: number;
  @Input() hintStr?: string;

  ngOnInit() {
    const validators = [];

    if (this.required) {
      validators.push(Validators.required);
    }

    if (this.pattern) {
      validators.push(Validators.pattern(this.pattern));
    }

    if (this.maxLength) {
      validators.push(Validators.maxLength(this.maxLength));
    }

    validators.push(Validators.minLength(this.minLength));
    this.internalControl.setValidators(validators);

    if(!this.modelName){
      this.modelName = "Name";
    }
  }
}

