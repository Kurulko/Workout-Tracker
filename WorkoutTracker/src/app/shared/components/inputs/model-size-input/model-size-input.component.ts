import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { ModelSize } from 'src/app/shared/models/model-size';
import { SizeType } from 'src/app/shared/models/size-type';

@Component({
  selector: 'app-model-size-input',
  templateUrl: './model-size-input.component.html',
  styleUrls: ['./model-size-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ModelSizeInputComponent),
      multi: true,
    },
  ],
})
export class ModelSizeInputComponent extends BaseEditorComponent<ModelSize> {
  @Input() sizeValue?: number;
  @Input() sizeMinValue: number = 0;
  @Input() sizeMaxValue: number|null = null;
  @Input() sizeIsShortSizeType: boolean = true;
  @Input() sizeHintStr?: string;
  @Input() sizeLabel?: string;
  @Input() sizeErrorMessage?: string;
  @Input() sizeWidth?: string = "65%";

  @Input() sizeTypeValue?: SizeType;
  @Input() sizeTypeIsShortForm: boolean = true;
  @Input() sizeTypeLabel?: string;
  @Input() sizeTypeErrorMessage?: string;
  @Input() sizeTypeWidth?: string = "35%";

  private _modelSize: ModelSize = <ModelSize>{};
  
  get modelSize(): ModelSize {
    return this._modelSize;
  }

  set modelSize(value: ModelSize) {
    this._modelSize = value;
    this.updateModelSize(); 
  }

  updateModelSize() {
    this.onChange(this._modelSize); 
    this.onTouched();
  }

  writeValue(value?: ModelSize): void {
    var modelSize = value ?? <ModelSize>{};
    
    if(this.sizeTypeValue !== undefined)
      modelSize.sizeType = this.sizeTypeValue;

    if(this.sizeValue !== undefined)
      modelSize.size = this.sizeValue;

    this._modelSize = modelSize;
  }
}
