import { Component, Input, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { BaseEditorComponent } from '../../base-editor.component';

@Component({
  selector: 'app-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DateInputComponent),
      multi: true,
    },
  ],
})
export class DateInputComponent extends BaseEditorComponent<Date> {
  @Input() maxDate?: Date;
  @Input() hintStr?: string;

  private _date: Date|null = null;

  ngOnInit(): void {
    this._date = this.value ?? null;
    this.modelName = this.modelName ?? "Date";
  }

  get date(): Date|null {
    return this._date;
  }

  set date(value: Date) {
    this._date = value;
    this.onChange(this._date); 
    this.onTouched();
  }

  writeValue(value?: Date): void {
    this._date = value ?? null;
  }
}