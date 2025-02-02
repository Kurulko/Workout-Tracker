import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-readonly-calendar',
  templateUrl: './readonly-calendar.component.html',
  styleUrls: ['./readonly-calendar.component.css']
})
export class ReadonlyCalendarComponent {
  @Input()
  width?: string;
  
  @Input()
  minDate: Date|null = null; 

  @Input()
  maxDate: Date|null = null; 

  @Input()
  selectedDates!: Date[]; 

  dateFilter = (_: Date | null): boolean => {
    return false; // Disable all dates
  };
}
