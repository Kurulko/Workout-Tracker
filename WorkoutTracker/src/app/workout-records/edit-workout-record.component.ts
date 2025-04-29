import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar  } from '@angular/material/snack-bar';

import { EditModelComponent } from 'src/app/shared/components/base/edit-model.component';
import { WorkoutRecord } from './workout-record';
import { WorkoutRecordService } from './workout-record.service';
import { ExerciseSetGroup } from '../shared/models/exercise-set-group';
import { ExerciseRecordGroup } from '../shared/models/exercise-record-group';
import { ExerciseSet } from '../shared/models/exercise-set';
import { TimeSpan } from '../shared/models/time-span';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { ExerciseRecord } from '../exercise-records/exercise-record';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';
import { toExerciseRecordGroups } from '../shared/helpers/functions/toExerciseRecordGroups';
import { toExerciseSetGroups } from '../shared/helpers/functions/toExerciseSetGroups';

@Component({
  selector: 'app-edit-workout-record',
  templateUrl: './edit-workout-record.component.html',
  styleUrls: ['./edit-workout-record.component.css']
})
export class EditWorkoutRecordComponent extends EditModelComponent<WorkoutRecord> implements OnInit {
  workoutRecord!: WorkoutRecord;
  exerciseSetGroups!: ExerciseSetGroup[];
  workoutId?: number;
  maxDate: Date = new Date();

  constructor(private activatedRoute: ActivatedRoute,  
    private workoutRecordService: WorkoutRecordService, 
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
    const workoutIdParam = this.activatedRoute.snapshot.paramMap.get('workoutId');
    const idParam = this.activatedRoute.snapshot.paramMap.get('id');

    if(workoutIdParam) {
      this.workoutId = +workoutIdParam;
    }

    this.id = idParam ? +idParam : 0;
    this.workoutId = workoutIdParam ? +workoutIdParam : 0;

    if (this.id) {
      // Edit mode
      this.workoutRecordService.getWorkoutRecordById(this.id)
      .pipe(this.catchLoadDataError(this.getWorkoutRecordsPath()))
      .subscribe((result: WorkoutRecord) => {
        this.workoutRecord = result;
        this.exerciseSetGroups = toExerciseSetGroups(result.exerciseRecordGroups);
        this.title = `Edit Workout Record`;
      });
    }
    else {
      // Add mode
      this.workoutRecord = <WorkoutRecord>{ date: new Date(), time: <TimeSpan>{} };
      if(this.workoutId) {
        this.workoutRecord.workoutId = this.workoutId;
      }

      this.exerciseSetGroups = [];
      this.title = "Create new Workout Record";
    }
  }

  onSubmit() {
    this.workoutRecord.exerciseRecordGroups = toExerciseRecordGroups(this.exerciseSetGroups, this.workoutRecord.date);

    if (this.id) {
      // Edit mode
      this.workoutRecordService.updateWorkoutRecord(this.workoutRecord)
        .pipe(this.catchError())
        .subscribe(_ => {
            console.log("Workout Record " + this.workoutRecord!.id + " has been updated.");
            this.router.navigate([this.getWorkoutRecordsPath()]);
        });
    }
    else {
      // Add mode
      this.workoutRecordService.createWorkoutRecord(this.workoutRecord)
        .pipe(this.catchError())
        .subscribe(_ => {
            // console.log("Workout Record " + result.id + " has been created.");
            this.router.navigate([this.getWorkoutRecordsPath()]);
        });
    }
  }

  getWorkoutRecordsPath(){
    if(this.workoutId)
      return `/workouts/${this.workoutId}/workout-records`;
    return `/workout-records`;
  }

  isExerciseSetGroupsValid!: boolean;
  onExerciseSetGroupsValidityChange(isValid: boolean): void {
    this.isExerciseSetGroupsValid = isValid;
  }
}
