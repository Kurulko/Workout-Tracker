import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-name-input',
  templateUrl: './name-input.component.html',
  styleUrls: ['./name-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => NameInputComponent),
      multi: true,
    },
  ],
})
export class NameInputComponent extends BaseEditorComponent<string> {
  @Input() pattern: string|RegExp = '';
  @Input() minlength: number = 3;
  @Input() hintStr?: string;

  private _name: string|null = null;
  
  ngOnInit(): void {
    this._name = this.value ?? null;
    this.modelName = this.modelName ?? "Name";
  }

  get name(): string|null {
    return this._name;
  }

  set name(value: string) {
    this._name = value;
    this.onChange(value); 
    this.onTouched();
  }

  writeValue(value?: string): void {
    this._name = value ?? null;
  }
}

