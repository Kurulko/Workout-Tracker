import { Component, forwardRef, Input } from '@angular/core';
import { BaseInputComponent } from '../base-input.component';
import { DateTimeRange } from 'src/app/shared/models/date-time-range';
import { ControlValueAccessor, FormBuilder, FormGroup, NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { BaseEditorComponent } from '../../base-editor.component';

@Component({
  selector: 'app-date-range-input',
  templateUrl: './date-range-input.component.html',
  styleUrls: ['./date-range-input.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => DateRangeInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DateRangeInputComponent),
      multi: true,
    },
  ],
})
export class DateRangeInputComponent extends BaseEditorComponent<DateTimeRange> {
  @Input() minDate?: Date;
  @Input() maxDate?: Date;
  @Input() hintStr?: string;

  rangeForm!: FormGroup;
  range!: DateTimeRange;

  constructor(private fb: FormBuilder){
    super();
  }

  ngOnInit() {
    this.initForm();
    this.range = <DateTimeRange>{};

    if(!this.modelName)
      this.modelName = "Date Range";
  }

  initForm() {
    var requiredValidation = this.required ? Validators.required : null;
    this.rangeForm = this.fb.group({
      firstDate: [null, [requiredValidation].filter(v => v !== null)],
      lastDate: [null, [requiredValidation].filter(v => v !== null)],
    });
  }

  writeValue(value?: DateTimeRange): void {
    if (value) {
      this.rangeForm.patchValue(value);
    } else {
      this.rangeForm.reset();
    }
  }

  override registerOnChange(fn: any): void {
    this.onChange = fn;
    this.rangeForm.valueChanges.subscribe(fn);
  }

  validate() {
    return this.rangeForm.valid ? null : this.rangeForm.errors;
  }

  clearRange() {
    this.rangeForm.reset();
    this.range = <DateTimeRange>{};
    this.onChange(undefined);
  }
}
