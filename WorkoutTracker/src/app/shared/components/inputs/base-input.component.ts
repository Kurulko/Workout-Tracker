import { Component } from '@angular/core';
import { BaseEditorComponent } from '../base-editor.component';
import { FormControl } from '@angular/forms';

@Component({
  template: '',
})
export abstract class BaseInputComponent<T> extends BaseEditorComponent<T> {
    internalControl = new FormControl();
    
    writeValue(value: any): void {
        this.internalControl.setValue(value);
    }

    override registerOnChange(fn: any): void {
        this.internalControl.valueChanges.subscribe(fn);
    }

    validate() {
        return this.internalControl.valid ? null : this.internalControl.errors;
    }
}