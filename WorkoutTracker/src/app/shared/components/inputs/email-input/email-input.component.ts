import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-email-input',
  templateUrl: './email-input.component.html',
  styleUrls: ['./email-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => EmailInputComponent),
      multi: true,
    },
  ],
})
export class EmailInputComponent extends BaseEditorComponent<string> {
  @Input() hintStr?: string;

  private _email: string|null = null;
  
  ngOnInit(): void {
    this._email = this.value ?? null;
    this.modelName = this.modelName ?? "Emal";
  }

  get email(): string|null {
    return this._email;
  }

  set email(value: string) {
    this._email = value;
    this.onChange(value); 
    this.onTouched();
  }

  writeValue(value?: string): void {
    this._email = value ?? null;
  }
}
