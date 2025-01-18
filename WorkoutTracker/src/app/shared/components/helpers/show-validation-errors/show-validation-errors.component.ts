import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-show-validation-errors',
  templateUrl: './show-validation-errors.component.html',
  styleUrls: ['./show-validation-errors.component.css']
})
export class ShowValidationErrorsComponent {
  @Input()
  validationErrors: { [key: string]: string[] } = {};

  getObjectKeys(obj: any): string[] {
    return Object.keys(obj);
  }
}
