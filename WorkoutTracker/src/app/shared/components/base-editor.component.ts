import { Component, Input } from '@angular/core';
import { AbstractControl, ControlValueAccessor, ValidationErrors, Validator } from '@angular/forms';

@Component({
  template: '',
})
export abstract class BaseEditorComponent<T> implements ControlValueAccessor, Validator {
  @Input() value?: T;
  @Input() modelName?: string;
  @Input() label?: string;
  @Input() required: boolean = false;
  @Input() width?: string;

  protected onChange: (value?: T) => void = () => {};
  protected onTouched: () => void = () => {};
  
  abstract writeValue(value?: T): void;
  abstract validate(control: AbstractControl): ValidationErrors | null;

  registerOnChange(fn: (value?: T) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
}