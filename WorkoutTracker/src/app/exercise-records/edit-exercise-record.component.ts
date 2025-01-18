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
    private exerciseRecordService: ExerciseRecordService, 
    router: Router,  
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(router, impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  exerciseType = ExerciseType;

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
      // Add mode
      this.title = "Create new Exercise Record";
      this.exerciseRecord = <ExerciseRecord>{ date : new Date() };
    }
  }

  onSubmit() {
    if (this.id) {
      // Edit mode
      this.exerciseRecordService.updateExerciseRecord(this.exerciseRecord)
      .pipe(this.catchError())
      .subscribe(_ => {
          this.modelUpdatedSuccessfully('Exercise Record')
          console.log("ExerciseRecord " + this.exerciseRecord!.id + " has been updated.");
          this.router.navigate([this.exerciseRecordsPath]);
      });
    }
    else {
      // Add mode
      this.exerciseRecordService.createExerciseRecord(this.exerciseRecord)
      .pipe(this.catchError())
      .subscribe(result => {
          this.modelAddedSuccessfully('Exercise Record')
          console.log("ExerciseRecord " + result.id + " has been created.");
          this.router.navigate([this.exerciseRecordsPath]);
      });
    }
  }
}