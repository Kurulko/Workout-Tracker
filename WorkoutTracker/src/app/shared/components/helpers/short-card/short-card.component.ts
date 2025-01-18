import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-short-card',
  templateUrl: './short-card.component.html',
  styleUrls: ['./short-card.component.css']
})
export class ShortCardComponent {
  @Input()
  title?: string;

  @Input()
  subtitle?: string;

  @Input()
  width?: string;
}
