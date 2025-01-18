import { Component, Input, OnInit, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { FormControl, FormGroup, NG_VALUE_ACCESSOR, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { TimeSpan } from 'src/app/shared/models/time-span';

@Component({
  selector: 'app-time-span-input',
  templateUrl: './time-span-input.component.html',
  styleUrls: ['./time-span-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TimeSpanInputComponent),
      multi: true,
    },
  ],
})
export class TimeSpanInputComponent extends BaseEditorComponent<TimeSpan> implements OnInit {
  @Input() isWithSeconds: boolean = false;
  @Input() isWithMilliseconds: boolean = false;

  timeSpanForm!: FormGroup;
  private _timeSpan: TimeSpan = <TimeSpan>{};
  

  ngOnInit(): void {
    this._timeSpan = this.value ?? <TimeSpan>{};
    this.modelName = this.modelName ?? "Time";
    this.initForm();
  }

  initForm(): void {
    this.timeSpanForm = new FormGroup({
      hours: new FormControl(null, [Validators.required, Validators.min(0)]),
      minutes: new FormControl(null, [Validators.required, Validators.min(0), Validators.max(60)]),
      seconds: new FormControl(null, [Validators.required, Validators.min(0), Validators.max(60)]),
      milliseconds: new FormControl(null, [Validators.required, Validators.min(0), Validators.max(1000)]),
    }, { validators: this.timeSpanValidator });
  }

  timeSpanValidator = (control: AbstractControl) => {
    const hours = control.get('hours');
    const minutes = control.get('minutes');
    const seconds = control.get('seconds');
    const milliseconds = control.get('milliseconds');

    return (
      (hours!.valid || hours!.untouched) 
      && (minutes!.valid || minutes!.untouched) 
      && (!this.isWithSeconds || (seconds!.valid || seconds!.untouched)) 
      && (!this.isWithMilliseconds || (milliseconds!.valid || milliseconds!.untouched))) 
      ? null : { notValid: true };
  }

  get timeSpan(): TimeSpan {
    return this._timeSpan;
  }

  set timeSpan(value: TimeSpan) {
    this._timeSpan = value;
    this.updateTimeSpan(); 
  }

  updateTimeSpan(){
    this.onChange(this._timeSpan); 
    this.onTouched();
  }

  writeValue(value?: TimeSpan): void {
    this._timeSpan = value ?? <TimeSpan>{};
  }
}