import { Component, Input, signal, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-password-input',
  templateUrl: './password-input.component.html',
  styleUrls: ['./password-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PasswordInputComponent),
      multi: true,
    },
  ],
})
export class PasswordInputComponent extends BaseEditorComponent<string> {
  @Input() pattern: string|RegExp = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_\-])[A-Za-z\d@$!%*?&_\-]{6,}$/;
  @Input() minlength: number = 8;
  @Input() hintStr?: string;

  private _password: string|null = null;

  ngOnInit(): void {
    this._password = this.value ?? null;
    this.modelName = this.modelName ?? "Password";
  }

  get password(): string|null {
    return this._password;
  }

  set password(value: string) {
    this._password = value;
    this.onChange(value); 
    this.onTouched();
  }

  hidePassword = signal(true);
  togglePasswordVisibility(event: MouseEvent): void {
    this.hidePassword.set(!this.hidePassword());
    event.stopPropagation();
  }

  writeValue(value?: string): void {
    this._password = value ?? null;
  }
}