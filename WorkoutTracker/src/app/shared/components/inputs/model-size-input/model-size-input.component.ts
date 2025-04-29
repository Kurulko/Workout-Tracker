import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { FormBuilder, FormGroup, NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { ModelSize } from 'src/app/shared/models/model-size';
import { SizeType } from 'src/app/shared/models/size-type';

@Component({
  selector: 'app-model-size-input',
  templateUrl: './model-size-input.component.html',
  styleUrls: ['./model-size-input.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => ModelSizeInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ModelSizeInputComponent),
      multi: true,
    },
  ],
})
export class ModelSizeInputComponent extends BaseEditorComponent<ModelSize> {
  @Input() sizeValue?: number;
  @Input() sizeMinValue: number = 1;
  @Input() sizeMaxValue: number|null = null;
  @Input() sizeIsShortSizeType: boolean = true;
  @Input() sizeHintStr?: string;
  @Input() sizeLabel?: string;
  @Input() sizeWidth?: string = "50%";

  @Input() sizeTypeValue?: SizeType;
  @Input() sizeTypeIsShortForm: boolean = true;
  @Input() sizeTypeLabel?: string;
  @Input() sizeTypeWidth?: string = "35%";

  modelSizeForm!: FormGroup;
  
  constructor(private fb: FormBuilder) {
    super();
  }
  
  ngOnInit(){
    this.initForm();
  }

  initForm() {
    this.modelSizeForm = this.fb.group({
      size: [null, this.getSizeValidators()],
      sizeType: [null, this.getSizeTypeValidators()],
    });
  }

  private getSizeValidators() {
    const validators = [Validators.min(this.sizeMinValue)];

    if (this.required) {
      validators.push(Validators.required);
    }

    if (this.sizeMaxValue) {
      validators.push(Validators.max(this.sizeMaxValue));
    }

    return validators;
  }
  
  private getSizeTypeValidators() {
    return this.required ? [Validators.required] : [];
  }

  override writeValue(value?: ModelSize): void {
    if (value || this.sizeValue || this.sizeTypeValue != undefined) {
      var modelSize = value ?? <ModelSize>{};

      if(this.sizeTypeValue !== undefined && modelSize.sizeType === undefined)
        modelSize.sizeType = this.sizeTypeValue;

      if(this.sizeValue !== undefined && modelSize.size === undefined)
        modelSize.size = this.sizeValue;

      this.modelSizeForm.patchValue(modelSize);
    } else {
      this.modelSizeForm.reset();
    }
  }

  override registerOnChange(fn: any): void {
    this.modelSizeForm.valueChanges.subscribe(fn);
  }

  validate() {
    if(this.modelSizeForm.valid)
      return null;

    const sizeErrors = this.modelSizeForm.get('size')?.errors || {};
    const sizeTypeErrors = this.modelSizeForm.get('sizeType')?.errors || {};
  
    const combinedErrors = { ...sizeErrors, ...sizeTypeErrors };
  
    return Object.keys(combinedErrors).length > 0 ? combinedErrors : null;
  }
}
