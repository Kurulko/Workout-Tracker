import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { WorkoutProgress } from './workout-progress';
import { MainComponent } from '../shared/components/base/main.component';
import { WorkoutProgressService } from './workout-progress.service';
import { showWeightTypeShort } from '../shared/helpers/functions/showFunctions/showWeightTypeShort';
import { roundNumber } from '../shared/helpers/functions/roundNumber';
import { showTime } from '../shared/helpers/functions/showFunctions/showTime';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';
import { showCountOfSomethingStr } from '../shared/helpers/functions/showFunctions/showCountOfSomethingStr';
import { TimeSpan } from '../shared/models/time-span';
import { YearMonth } from '../shared/models/year-month';
import { WorkoutStatus } from './workout-status';
import { DateTimeRange } from '../shared/models/date-time-range';
import { CurrentUserProgress } from './current-user-progress';
import { differenceInDays } from 'date-fns';
import { showLastDateStr } from '../shared/helpers/functions/showFunctions/showLastDateStr';

@Component({
  selector: 'app-workout-progress',
  templateUrl: './workout-progress.component.html',
  styleUrls: ['./workout-progress.component.css'],
})
export class WorkoutProgressComponent extends MainComponent implements OnInit {
  currentUserProgress!: CurrentUserProgress;
  workoutProgress!: WorkoutProgress;

  workoutDays!: number;
  restDays!: number;

  workoutDates: Date[] = [];

  firstWorkoutDate!: Date;
  todayDate = new Date();
  totalDays!: TimeSpan;

  range: DateTimeRange|null = null;
  maxDate: Date = new Date();

  constructor(
    private workoutProgressService: WorkoutProgressService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  showWeightTypeShort = showWeightTypeShort;
  roundWeight = roundNumber;
  showTime = showTime;

  ngOnInit(): void {
    this.loadCurrentUserProgress();
    this.loadWorkoutProgress();
  }

  showCountOfSomethingStr = showCountOfSomethingStr;
  showLastDateStr = showLastDateStr;
  WorkoutStatus = WorkoutStatus;

  showDays(countOfDays: number) {
    return showCountOfSomethingStr(countOfDays, 'day', 'days');
  }
  
  loadCurrentUserProgress(){
    this.workoutProgressService.calculateCurrentUserProgress()
    .pipe(this.catchError())
    .subscribe((result: CurrentUserProgress) => {
      this.currentUserProgress = result;
      this.totalDays = <TimeSpan>{days: result.countOfWorkoutDays};
    });
  }

  loadWorkoutProgress() {
    this.workoutProgressService.calculateWorkoutProgress(this.range)
      .pipe(this.catchError())
      .subscribe((result: WorkoutProgress) => {
        this.workoutProgress = result;
        var baseInfoProgress = result.baseInfoProgress;
        
        this.workoutDates = baseInfoProgress.workoutDates!.map(workoutDate => new Date(workoutDate));

        this.workoutDays = baseInfoProgress.totalWorkouts;
        var countOfWorkoutDays = this.getCountOfDaysTillTodayByRange();
        this.restDays = countOfWorkoutDays - baseInfoProgress.totalWorkouts;
        console.log(`countOfWorkoutDays ${countOfWorkoutDays}, workoutDays: ${this.workoutDays}, restDays: ${this.restDays}`)
      });
  }

  getCountOfDaysTillTodayByRange(): number {
    var today = new Date();
    if(this.range) {
      return differenceInDays(today, this.range.firstDate) + 1;
    }
    else {
      return differenceInDays(today, this.currentUserProgress.firstWorkoutDate!) + 1;
    }
  }

  onDateRangeChange() {
    if(this.range){
      if(!this.range.firstDate) {
        this.range.firstDate = this.currentUserProgress.firstWorkoutDate!;
      }
      if(!this.range.lastDate) {
        this.range.lastDate = this.currentUserProgress.lastWorkoutDate!;
      }
    }

    this.loadWorkoutProgress();
  }
  
  monthWorkoutProgress!: WorkoutProgress;
  currentMonth!: YearMonth;

  getCurrentMonthDate() {
    return new Date(this.currentMonth.year, this.currentMonth.month)
  }

  onCurrentMonthChange(currentMonth: YearMonth) {
    this.currentMonth = currentMonth;

    this.workoutProgressService.calculateWorkoutProgressForMonth(currentMonth.year, currentMonth.month + 1)
      .pipe(this.catchError())
      .subscribe((result: WorkoutProgress) => {
        this.monthWorkoutProgress = result;
      });
  }
}
