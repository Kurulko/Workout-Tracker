import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { showSizeType } from 'src/app/shared/helpers/functions/showFunctions/showSizeType';
import { showSizeTypeShort } from 'src/app/shared/helpers/functions/showFunctions/showSizeTypeShort';
import { SizeType } from 'src/app/shared/models/size-type';

@Component({
  selector: 'app-size-input',
  templateUrl: './size-input.component.html',
  styleUrls: ['./size-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SizeInputComponent),
      multi: true,
    },
  ],
})
export class SizeInputComponent  extends BaseEditorComponent<number> {
  @Input() minValue: number = 0;
  @Input() maxValue: number| null = null;
  @Input() sizeType?: SizeType;
  @Input() isShortSizeType: boolean = true;
  @Input() hintStr?: string;

  private _size: number|null = null;

  showSizeType = showSizeType;
  showSizeTypeShort = showSizeTypeShort;

  ngOnInit(): void {
    this._size = this.value ?? null;
    this.modelName = this.modelName ?? "Size";
  }

  get size(): number|null {
    return this._size;
  }

  set size(value: number) {
    this._size = value;
    this.onChange(value); 
    this.onTouched();
  }

  validateSizeInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
  
    const regex = /^\d*\.?\d{0,1}$/;
  
    if (!regex.test(value)) {
      input.value = value.slice(0, -1);
    }
  }

  writeValue(value?: number): void {
    this._size = value ?? null;
  }
}
