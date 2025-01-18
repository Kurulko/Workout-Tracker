import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-reps-input',
  templateUrl: './reps-input.component.html',
  styleUrls: ['./reps-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => RepsInputComponent),
      multi: true,
    },
  ],
})
export class RepsInputComponent extends BaseEditorComponent<number> {
  @Input() hintStr?: string;

  private _reps: number|null = null;

  ngOnInit(): void {
    this._reps = this.value ?? null;
    this.modelName = this.modelName ?? "Reps";
  }

  get reps(): number|null {
    return this._reps;
  }

  set reps(value: number) {
    this._reps = value;
    this.onChange(value); 
    this.onTouched();
  }

  writeValue(value?: number): void {
    this._reps = value ?? null;
  }
}
