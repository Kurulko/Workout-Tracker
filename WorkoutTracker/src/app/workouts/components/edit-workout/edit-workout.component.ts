import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar  } from '@angular/material/snack-bar';

import { EditModelComponent } from 'src/app/shared/components/base/edit-model.component';
import { ExerciseSetGroup } from '../../../shared/models/exercises/exercise-set-group';
import { ImpersonationManager } from '../../../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../../../shared/helpers/managers/token-manager';
import { PreferencesManager } from '../../../shared/helpers/managers/preferences-manager';
import { TimeSpan } from '../../../shared/models/time-span';
import { MatDialog } from '@angular/material/dialog';
import { WorkoutRecordService } from '../../services/workout-record.service';
import { toExerciseRecordGroups } from '../../../shared/helpers/functions/toExerciseRecordGroups';
import { Workout } from '../../models/workout';
import { WorkoutRecord } from '../../models/workout-record';
import { WorkoutService } from '../../services/workout.service';

@Component({
  selector: 'app-workout-edit',
  templateUrl: './edit-workout.component.html',
})
export class EditWorkoutComponent extends EditModelComponent<Workout> implements OnInit {
  workout!: Workout;
  exerciseSetGroups: ExerciseSetGroup[] = [];

  readonly workoutsPath = "/workouts";

  @ViewChild('completedTemplate') completedTemplate!: TemplateRef<TimeSpan>;

  constructor(private activatedRoute: ActivatedRoute,  
    private workoutService: WorkoutService, 
    private workoutRecordService: WorkoutRecordService, 
    private dialog: MatDialog, 
    router: Router, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(router, impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.id = idParam ? +idParam : 0;
    if (this.id) {
      // Edit mode
      this.workoutService.getWorkoutById(this.id)
        .pipe(this.catchLoadDataError(this.workoutsPath))
        .subscribe((result: Workout) => {
          this.workout = result;
          this.exerciseSetGroups = result.exerciseSetGroups;

          this.title = `Edit Workout '${this.workout.name}'`;
        });
    }
    else {
      // Add mode
      this.title = "Create new Workout";
      this.workout = <Workout>{};
    }
  }

  onSubmit(isComplete: boolean = false) {
    this.workout.exerciseSetGroups = this.exerciseSetGroups;
    if (this.id) {
      // Edit mode
      this.workoutService.updateWorkout(this.workout)
        .pipe(this.catchError())
        .subscribe(_ => {
          console.log("Workout " + this.workout!.id + " has been updated.");

          this.workoutService.updateWorkoutExerciseSetGroups(this.workout.id, this.exerciseSetGroups)
            .pipe(this.catchError())
            .subscribe(_ => {
              if(isComplete) {
                this.openWorkoutCompleteDialog()
              } else {
                this.router.navigate([this.workoutsPath]);
              }
            });
        });
    }
    else {
      // Add mode
      this.workoutService.createWorkout(this.workout)
        .pipe(this.catchError())
        .subscribe(result => {
          console.log("Workout " + result.id + " has been created.");
          this.workout = result;

          this.workoutService.addExerciseSetGroupsToWorkout(result.id, this.exerciseSetGroups)
            .pipe(this.catchError())
            .subscribe(_ => {
              if(isComplete) {
                this.openWorkoutCompleteDialog()
              } else {
                this.router.navigate([this.workoutsPath]);
              }
            });
        });
    }
  }

  isExerciseSetGroupsValid!: boolean;
  onExerciseSetGroupsValidityChange(isValid: boolean): void {
    this.isExerciseSetGroupsValid = isValid;
  }
  
  workoutTime!: TimeSpan;
  workoutDate!: Date;

  maxCompleteDate: Date = new Date();
  
  onComplete() {
    this.openWorkoutCompleteDialog()
    this.isCompleteCurrentWorkout = false;
  }

  isCompleteCurrentWorkout = true;
  openWorkoutCompleteDialog(){
    this.workoutTime = new TimeSpan();
    this.workoutDate = new Date();

    this.dialog.open(this.completedTemplate, { width: '300px' });
  }

  completeCurrentWorkout() {
    this.workoutService.completeWorkout(this.workout.id, this.workoutDate, this.workoutTime)
      .pipe(this.catchError())
      .subscribe(() => {
        this.operationDoneSuccessfully("Workout", "completed");
        this.router.navigate([this.workoutsPath]);
      })
  }

  completeWorkoutRecord() {
    var workoutRecord = <WorkoutRecord>{ date: this.workoutDate, time: this.workoutTime };
    workoutRecord.exerciseRecordGroups = toExerciseRecordGroups(this.workout.exerciseSetGroups, this.workoutDate);
    workoutRecord.workoutId = this.workout.id;
    
    this.workoutRecordService.createWorkoutRecord(workoutRecord)
      .pipe(this.catchError())
      .subscribe(() => {
        this.operationDoneSuccessfully("Workout", "completed");
        this.router.navigate([this.workoutsPath]);
      })
  }

}