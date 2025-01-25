import { Component, Input } from '@angular/core';
import { BaseEditorComponent } from '../base-editor.component';

@Component({
  template: '',
})
export abstract class BaseSelectorComponent<T> extends BaseEditorComponent<T> {
  @Input() noneOptionStr?: string;
  @Input() errorMessage?: string;

  protected validateEnum(selectedEnumItem: any) {
    if(!this.required)
      return null;

    return selectedEnumItem != undefined && selectedEnumItem != null ? null : { required: true };
  }
}