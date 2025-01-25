import { Component, Input, signal, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { BaseInputComponent } from '../base-input.component';

@Component({
  selector: 'app-password-input',
  templateUrl: './password-input.component.html',
  styleUrls: ['./password-input.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => PasswordInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PasswordInputComponent),
      multi: true,
    },
  ],
})
export class PasswordInputComponent extends BaseInputComponent<string> {
  @Input() pattern: string|RegExp = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_\-])[A-Za-z\d@$!%*?&_\-]{6,}$/;
  @Input() minLength: number = 8;
  @Input() maxLength?: number;
  @Input() hintStr?: string;

  ngOnInit() {
    const validators = [];

    if (this.required) {
      validators.push(Validators.required);
    }

    if (this.maxLength) {
      validators.push(Validators.maxLength(this.maxLength));
    }

    validators.push(Validators.pattern(this.pattern));
    validators.push(Validators.minLength(this.minLength));

    this.internalControl.setValidators(validators);

    if(!this.modelName){
      this.modelName = "Password";
    }
  }

  hidePassword = signal(true);
  togglePasswordVisibility(event: MouseEvent): void {
    this.hidePassword.set(!this.hidePassword());
    event.stopPropagation();
  }
}