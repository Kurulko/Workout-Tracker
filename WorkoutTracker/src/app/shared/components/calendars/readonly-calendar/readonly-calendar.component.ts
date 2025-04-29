import { Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { MatCalendar } from '@angular/material/datepicker';
import { YearMonth } from 'src/app/shared/models/year-month';

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

  private currentMonth!:  YearMonth; 
  @Output() currentMonthChange = new EventEmitter<YearMonth>();

  dateFilter = (date: Date | null): boolean => {
    if(date) {
      this.changeCurrentMonth(date);
    }

    return false; // Disable all dates
  };

  private changeCurrentMonth(date: Date): void {
    var month = <YearMonth>{year: date.getFullYear(), month: date.getMonth()}
    if(!this.currentMonth || !this.isSameMonth(this.currentMonth, month)) {
      this.currentMonth = month;
      this.emitCurrentMonth();
    }
  }

  private isSameMonth(month1: YearMonth, month2: YearMonth): boolean {
    return month1.year === month2.year && month1.month === month2.month;
  }

  private emitCurrentMonth(): void {
    this.currentMonthChange.emit(this.currentMonth);
  }
}
