import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { differenceInDays } from 'date-fns';

import { MainComponent } from 'src/app/shared/components/base/main.component';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';
import { showCountOfSomethingStr } from 'src/app/shared/helpers/functions/showFunctions/showCountOfSomethingStr';
import { showTime } from 'src/app/shared/helpers/functions/showFunctions/showExerciseValue';
import { showLastDateStr } from 'src/app/shared/helpers/functions/showFunctions/showLastDateStr';
import { showWeightTypeShort } from 'src/app/shared/helpers/functions/showFunctions/showWeightTypeShort';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { DateTimeRange } from 'src/app/shared/models/date-time-range';
import { TimeSpan } from 'src/app/shared/models/time-span';
import { YearMonth } from 'src/app/shared/models/year-month';
import { CurrentUserProgress } from '../models/current-user-progress';
import { WorkoutProgress } from '../models/workout-progress';
import { WorkoutStatus } from '../models/workout-status';
import { WorkoutProgressService } from '../services/workout-progress.service';


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
