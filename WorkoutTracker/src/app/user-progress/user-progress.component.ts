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

  loadData() {
    this.userProgressService.calculateUserProgress()
      .pipe(this.catchError())
      .subscribe((result: UserProgress) => {
        this.userProgress = result;

        this.workoutDates = result.workoutDates!.map(workoutDate => new Date(workoutDate));
        this.workoutDays = result.totalWorkouts;
        this.restDays = result.countOfDaysSinceFirstWorkout - result.totalWorkouts;
      });
  }
}
