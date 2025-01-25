import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

import { EditModelComponent } from '../shared/components/base/edit-model.component';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { ExerciseRecord } from './exercise-record';
import { ExerciseRecordService } from './exercise-record.service';
import { ExerciseType } from '../exercises/models/exercise-type';
import { ExerciseService } from '../exercises/services/exercise.service';

@Component({
  selector: 'app-edit-exercise-record',
  templateUrl: './edit-exercise-record.component.html',
  styleUrls: ['./edit-exercise-record.component.css']
})
export class EditExerciseRecordComponent extends EditModelComponent<ExerciseRecord> implements OnInit {
  exerciseRecord!: ExerciseRecord;
  maxDate: Date = new Date();

  readonly exerciseRecordsPath = '/exercise-records';

  constructor( 
    private activatedRoute: ActivatedRoute,  
    private exerciseService: ExerciseService, 
    private exerciseRecordService: ExerciseRecordService, 
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
      this.exerciseRecordService.getExerciseRecordById(this.id)
      .pipe(this.catchLoadDataError(this.exerciseRecordsPath))
      .subscribe(result => {
        this.exerciseRecord = result;
        this.title = `Edit Exercise Record`;
      });
    }
    else {
      this.router.navigate([this.exerciseRecordsPath]);
    }
  }

  onSubmit() {
    this.prepareExerciseRecordBeforeSumbiting(this.exerciseRecord);
    this.exerciseRecordService.updateExerciseRecord(this.exerciseRecord)
    .pipe(this.catchError())
    .subscribe(_ => {
        this.modelUpdatedSuccessfully('Exercise Record')
        console.log("ExerciseRecord " + this.exerciseRecord!.id + " has been updated.");
        this.router.navigate([this.exerciseRecordsPath]);
    });
  }

  isExerciseSetValid: boolean = false;
  onExerciseSetValidityChange(isValid: boolean){
    this.isExerciseSetValid = isValid;
  }


  prepareExerciseRecordBeforeSumbiting(exerciseRecord: ExerciseRecord) {
    switch (exerciseRecord.exerciseType) {
      case ExerciseType.Reps:
        exerciseRecord.time = null;
        exerciseRecord.weight = null;
        break;
      case ExerciseType.Time:
        exerciseRecord.reps = null;
        exerciseRecord.weight = null;
        break;
      case ExerciseType.WeightAndReps:
        exerciseRecord.time = null;
        break;
      case ExerciseType.WeightAndTime:
        exerciseRecord.reps = null;
        break;
      default:
        throw new Error(`Unexpected exerciseType value`);
    }
  }

  onExerciseSelected() {
    this.exerciseService.getExerciseById(this.exerciseRecord.exerciseId)
      .pipe(this.catchError())
      .subscribe(exercise => {
        this.exerciseRecord.exerciseType = exercise.type;
      });
  }
}