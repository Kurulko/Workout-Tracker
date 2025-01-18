import { Component, Input } from '@angular/core';
import { AuthResult } from '../../models/auth-result.model';

@Component({
  selector: 'app-show-auth-result-errors',
  templateUrl: './show-auth-result-errors.component.html',
  styleUrls: ['./show-auth-result-errors.component.css']
})
export class ShowAuthResultErrorsComponent {
  @Input()
  authResult?: AuthResult;
}
