import { Component, Input, OnInit, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { FormControl, FormGroup, NG_VALUE_ACCESSOR, Validators, AbstractControl, ValidationErrors, NG_VALIDATORS, FormBuilder } from '@angular/forms';
import { TimeSpan } from 'src/app/shared/models/time-span';
import { showCountOfSomethingStr } from 'src/app/shared/helpers/functions/showFunctions/showCountOfSomethingStr';

@Component({
  selector: 'app-time-span-input',
  templateUrl: './time-span-input.component.html',
  styleUrls: ['./time-span-input.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => TimeSpanInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TimeSpanInputComponent),
      multi: true,
    },
  ],
})
export class TimeSpanInputComponent extends BaseEditorComponent<TimeSpan> implements OnInit {
  @Input() min: number = 0;
  @Input() maxHours: number = 24;
  @Input() isWithSeconds: boolean = false;
  @Input() isWithMilliseconds: boolean = false;

  timeSpanForm!: FormGroup;

  constructor(private fb: FormBuilder){
    super();
  }
  
  showCountOfSomethingStr = showCountOfSomethingStr;

  ngOnInit(){
    this.initForm();

    if(!this.modelName) {
      this.modelName = "Time";
    }
  }

  initForm() {
    var requiredValidation = this.required ? Validators.required : null;
    this.timeSpanForm = this.fb.group({
      hours: [null, [Validators.min(this.min), Validators.max(this.maxHours), requiredValidation].filter(v => v !== null)],
      minutes: [null, [Validators.min(this.min), Validators.max(59), requiredValidation].filter(v => v !== null)],
      seconds: [null, [Validators.min(this.min), Validators.max(59), requiredValidation].filter(v => v !== null)],
      milliseconds: [null, [Validators.min(this.min), Validators.max(999), requiredValidation].filter(v => v !== null)]
    }, { validators: this.timeSpanValidator });
  }

  timeSpanValidator = (control: AbstractControl) => {
    const { hours, minutes, seconds, milliseconds } = control.value;

    const isAnyValueAboveZero = 
      (hours ?? 0) > 0 ||
      (minutes ?? 0) > 0 ||
      (this.isWithSeconds && (seconds ?? 0) > 0) ||
      (this.isWithMilliseconds && (milliseconds ?? 0) > 0);

    const areAllFieldsValid =
      control.get('hours')!.valid &&
      control.get('minutes')!.valid &&
      (!this.isWithSeconds || control.get('seconds')!.valid) &&
      (!this.isWithMilliseconds || control.get('milliseconds')!.valid);

    return isAnyValueAboveZero && areAllFieldsValid ? null : { notValid: true };
  }

  override writeValue(value?: TimeSpan): void {
    if (value) {
      this.timeSpanForm.patchValue(value);
    } else {
      this.timeSpanForm.reset();
    }
  }

  override registerOnChange(fn: any): void {
    this.onChange = fn;
    this.timeSpanForm.valueChanges.subscribe(fn);
  }

  validate() {
    return this.timeSpanForm.valid ? null : this.timeSpanForm.errors;
  }
}