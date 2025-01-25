import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UserProgress } from './user-progress';
import { MainComponent } from '../shared/components/base/main.component';
import { UserProgressService } from './user-progress.service';
import { showWeightTypeShort } from '../shared/helpers/functions/showFunctions/showWeightTypeShort';
import { roundNumber } from '../shared/helpers/functions/roundNumber';
import { showTime } from '../shared/helpers/functions/showFunctions/showTime';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';
import { showCountOfSomethingStr } from '../shared/helpers/functions/showFunctions/showCountOfSomethingStr';
import { TimeSpan } from '../shared/models/time-span';
import { showBigNumberStr } from '../shared/helpers/functions/showFunctions/showBigNumberStr';

@Component({
  selector: 'app-user-progress',
  templateUrl: './user-progress.component.html',
  styleUrls: ['./user-progress.component.css'],
})
export class UserProgressComponent extends MainComponent implements OnInit {
  userProgress!: UserProgress;
  workoutDays!: number;
  restDays!: number;

  workoutDates: Date[] = [];

  firstWorkoutDate!: Date;
  todayDate = new Date();
  totalDays!: TimeSpan;

  constructor(private userProgressService: UserProgressService, 
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
    this.loadData();
  }

  showCountOfSomethingStr = showCountOfSomethingStr;
  
  loadData() {
    this.userProgressService.calculateUserProgress()
      .pipe(this.catchError())
      .subscribe((result: UserProgress) => {
        this.userProgress = result;
        this.totalDays = <TimeSpan>{days: result.countOfDaysSinceFirstWorkout};
        
        this.workoutDates = result.workoutDates!.map(workoutDate => new Date(workoutDate));
        this.firstWorkoutDate = new Date(Math.min(...this.workoutDates.map(date => date.getTime())))

        this.workoutDays = result.totalWorkouts;
        this.restDays = result.countOfDaysSinceFirstWorkout - result.totalWorkouts;
      });
  }
}
