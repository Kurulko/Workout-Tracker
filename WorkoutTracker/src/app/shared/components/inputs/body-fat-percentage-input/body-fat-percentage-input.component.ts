import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-body-fat-percentage-input',
  templateUrl: './body-fat-percentage-input.component.html',
  styleUrls: ['./body-fat-percentage-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => BodyFatPercentageInputComponent),
      multi: true,
    },
  ],
})
export class BodyFatPercentageInputComponent extends BaseEditorComponent<number> {
  @Input() hintStr?: string;

  private _bodyFatPercentage: number|null = null;

  ngOnInit(): void {
    this._bodyFatPercentage = this.value ?? null;
    this.modelName = this.modelName ?? "Body fat percentage";
  }

  get bodyFatPercentage(): number|null {
    return this._bodyFatPercentage;
  }

  set bodyFatPercentage(value: number) {
    this._bodyFatPercentage = value;
    this.onChange(value); 
    this.onTouched();
  }

  validateBodyFatPercentageInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
  
    const regex = /^\d*\.?\d{0,1}$/;
  
    if (!regex.test(value)) {
      input.value = value.slice(0, -1);
    }
  }

  writeValue(value?: number): void {
    this._bodyFatPercentage = value ?? null;
  }
}
