import { Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  template: '',
   providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => BaseEditorComponent),
      multi: true,
    },
  ],
})
export abstract class BaseEditorComponent<T> implements ControlValueAccessor {
  @Input() value?: T;
  @Input() modelName?: string;
  @Input() label?: string;
  @Input() required: boolean = false;
  @Input() errorMessage?: string;
  @Input() width?: string;

  protected onChange: (value?: T) => void = () => {};
  protected onTouched: () => void = () => {};
  
  abstract writeValue(value?: T): void;

  registerOnChange(fn: (value?: T) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  } 
}