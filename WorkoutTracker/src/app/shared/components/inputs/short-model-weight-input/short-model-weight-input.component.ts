import { Component, Input, OnInit, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { FormBuilder, FormGroup, NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { ModelWeight } from 'src/app/shared/models/model-weight';
import { WeightType } from 'src/app/shared/models/enums/weight-type';

@Component({
  selector: 'app-short-model-weight-input',
  templateUrl: './short-model-weight-input.component.html',
  styleUrls: ['./short-model-weight-input.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => ShortModelWeightInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ShortModelWeightInputComponent),
      multi: true,
    },
  ],
})
export class ShortModelWeightInputComponent extends BaseEditorComponent<ModelWeight> implements OnInit {
  @Input() weightValue?: number;
  @Input() weightMinValue: number = 1;
  @Input() weightMaxValue: number|null = null;
  @Input() weightHintStr?: string;
  @Input() weightLabel?: string;

  @Input() weightTypeValue?: WeightType;
  @Input() weightTypeLabel?: string;

  modelWeightForm!: FormGroup;

  constructor(private fb: FormBuilder){
    super();
  }
  
  ngOnInit(){
    this.initForm();
  }

  initForm() {
    this.modelWeightForm = this.fb.group({
      weight: [null, this.getWeightValidators()],
      weightType: [null, this.getWeightTypeValidators()],
    });
  }

  private getWeightValidators() {
    const validators = [Validators.min(this.weightMinValue)];

    if (this.required) {
      validators.push(Validators.required);
    }

    if (this.weightMaxValue) {
      validators.push(Validators.max(this.weightMaxValue));
    }

    return validators;
  }
  
  private getWeightTypeValidators() {
    return this.required ? [Validators.required] : [];
  }

  override writeValue(value?: ModelWeight): void {
    if (value || this.weightValue || this.weightTypeValue != undefined) {
      var modelWeight = value ?? <ModelWeight>{};

      if(this.weightTypeValue !== undefined && modelWeight.weightType === undefined)
        modelWeight.weightType = this.weightTypeValue;

      if(this.weightValue !== undefined && modelWeight.weight === undefined)
        modelWeight.weight = this.weightValue;

      this.modelWeightForm.patchValue(modelWeight);
    } else {
      this.modelWeightForm.reset();
    }
  }

  override registerOnChange(fn: any): void {
    this.modelWeightForm.valueChanges.subscribe(fn);
  }

  validate() {
    if(this.modelWeightForm.valid)
      return null;

    const weightErrors = this.modelWeightForm.get('weight')?.errors || {};
    const weightTypeErrors = this.modelWeightForm.get('weightType')?.errors || {};
  
    const combinedErrors = { ...weightErrors, ...weightTypeErrors };
  
    return Object.keys(combinedErrors).length > 0 ? combinedErrors : null;
  }
}
