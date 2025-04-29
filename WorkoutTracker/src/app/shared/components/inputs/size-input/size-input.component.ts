import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { showSizeType } from 'src/app/shared/helpers/functions/showFunctions/showSizeType';
import { showSizeTypeShort } from 'src/app/shared/helpers/functions/showFunctions/showSizeTypeShort';
import { SizeType } from 'src/app/shared/models/size-type';
import { BaseInputComponent } from '../base-input.component';

@Component({
  selector: 'app-size-input',
  templateUrl: './size-input.component.html',
  styleUrls: ['./size-input.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => SizeInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SizeInputComponent),
      multi: true,
    },
  ],
})
export class SizeInputComponent extends BaseInputComponent<number> {
  @Input() minValue: number = 1;
  @Input() maxValue: number| null = null;
  @Input() sizeType?: SizeType;
  @Input() isShortSizeType: boolean = true;
  @Input() hintStr?: string;

  ngOnInit() {
    const validators = [];

    if (this.required) {
      validators.push(Validators.required);
    }

    if (this.maxValue) {
      validators.push(Validators.max(this.maxValue));
    }

    validators.push(Validators.min(this.minValue));
    this.internalControl.setValidators(validators);

    if(!this.modelName){
      this.modelName = "Size";
    }
  }

  showSizeType = showSizeType;
  showSizeTypeShort = showSizeTypeShort;

  validateSizeInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
  
    const regex = /^\d*\.?\d{0,2}$/;
  
    if (!regex.test(value)) {
      input.value = value.slice(0, -1);
    }
  }
}
