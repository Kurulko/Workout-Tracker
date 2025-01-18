import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { showWeightType } from 'src/app/shared/helpers/functions/showFunctions/showWeightType';
import { showWeightTypeShort } from 'src/app/shared/helpers/functions/showFunctions/showWeightTypeShort';
import { WeightType } from 'src/app/shared/models/weight-type';

@Component({
  selector: 'app-weight-input',
  templateUrl: './weight-input.component.html',
  styleUrls: ['./weight-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => WeightInputComponent),
      multi: true,
    },
  ],
})
export class WeightInputComponent extends BaseEditorComponent<number> {
  @Input() minValue: number = 0;
  @Input() maxValue: number| null = null;

  @Input() weightType?: WeightType;
  @Input() isShortWeightType: boolean = true;
  @Input() hintStr?: string;

  private _weight: number|null = null;

  showWeightType = showWeightType;
  showWeightTypeShort = showWeightTypeShort;

  ngOnInit(): void {
    this._weight = this.value ?? null;
    this.modelName = this.modelName ?? "Weight";
  }

  get weight(): number|null {
    return this._weight;
  }

  set weight(value: number) {
    this._weight = value;
    this.onChange(value); 
    this.onTouched();
  }

  validateWeightInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
  
    const regex = /^\d*\.?\d{0,1}$/;
  
    if (!regex.test(value)) {
      input.value = value.slice(0, -1);
    }
  }

  writeValue(value?: number): void {
    this._weight = value ?? null;
  }
}
