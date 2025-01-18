import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { ModelWeight } from 'src/app/shared/models/model-weight';
import { WeightType } from 'src/app/shared/models/weight-type';

@Component({
  selector: 'app-model-weight-input',
  templateUrl: './model-weight-input.component.html',
  styleUrls: ['./model-weight-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ModelWeightInputComponent),
      multi: true,
    },
  ],
})
export class ModelWeightInputComponent extends BaseEditorComponent<ModelWeight> {
  @Input() weightValue?: number;
  @Input() weightMinValue: number = 0;
  @Input() weightMaxValue: number|null = null;
  @Input() weightIsShortWeightType: boolean = true;
  @Input() weightHintStr?: string;
  @Input() weightLabel?: string;
  @Input() weightErrorMessage?: string;
  @Input() weightWidth?: string = "65%";

  @Input() weightTypeValue?: WeightType;
  @Input() weightTypeIsShortForm: boolean = true;
  @Input() weightTypeLabel?: string;
  @Input() weightTypeErrorMessage?: string;
  @Input() weightTypeWidth?: string = "35%";

  private _modelWeight: ModelWeight = <ModelWeight>{};
  
  get modelWeight(): ModelWeight {
    return this._modelWeight;
  }

  set modelWeight(value: ModelWeight) {
    this._modelWeight = value;
    this.updateModelWeight(); 
  }

  updateModelWeight() {
    this.onChange(this._modelWeight); 
    this.onTouched();
  }

  writeValue(value?: ModelWeight): void {
    var modelWeight = value ?? <ModelWeight>{};

    if(this.weightTypeValue !== undefined)
      modelWeight.weightType = this.weightTypeValue;

    if(this.weightValue !== undefined)
      modelWeight.weight = this.weightValue;

    this._modelWeight = modelWeight;
  }
}