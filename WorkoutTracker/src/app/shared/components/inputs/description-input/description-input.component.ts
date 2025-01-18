import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-description-input',
  templateUrl: './description-input.component.html',
  styleUrls: ['./description-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DescriptionInputComponent),
      multi: true,
    },
  ],
})
export class DescriptionInputComponent extends BaseEditorComponent<string> {
  @Input() hintStr?: string;
  @Input() minlength: number = 10;

  private _description: string|null = null;

  ngOnInit(): void {
    this._description = this.value ?? null;
    this.modelName = this.modelName ?? "Description";
  }

  get description(): string|null {
    return this._description;
  }

  set description(value: string) {
    this._description = value;
    this.onChange(value); 
    this.onTouched();
  }

  writeValue(value?: string): void {
    this._description = value ?? null;
  }
}
