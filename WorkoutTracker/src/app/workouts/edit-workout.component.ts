import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar  } from '@angular/material/snack-bar';

import { EditModelComponent } from 'src/app/shared/components/base/edit-model.component';
import { Workout } from './workout';
import { WorkoutService } from './workout.service';
import { ExerciseSetGroup } from '../shared/models/exercise-set-group';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';

@Component({
  selector: 'app-workout-edit',
  templateUrl: './edit-workout.component.html',
})
export class EditWorkoutComponent extends EditModelComponent<Workout> implements OnInit {
  workout!: Workout;
  exerciseSetGroups: ExerciseSetGroup[] = [];

  readonly workoutsPath = "/workouts";

  constructor(private activatedRoute: ActivatedRoute,  
    private workoutService: WorkoutService, 
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

  onSubmit() {
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
              this.router.navigate([this.workoutsPath]);
            });
        });
    }
    else {
      // Add mode
      this.workoutService.createWorkout(this.workout)
        .pipe(this.catchError())
        .subscribe(result => {
          console.log("Workout " + result.id + " has been created.");

          this.workoutService.addExerciseSetGroupsToWorkout(result.id, this.exerciseSetGroups)
            .pipe(this.catchError())
            .subscribe(_ => {
              this.router.navigate([this.workoutsPath]);
            });
        });
    }
  }

  isExerciseSetGroupsValid: boolean = true;
  onExerciseSetGroupsValidityChange(isValid: boolean): void {
    this.isExerciseSetGroupsValid = isValid;
  }
}