import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

import { EditModelComponent } from '../../../shared/components/base/edit-model.component';
import { ExerciseService } from '../../services/exercise.service';
import { ImpersonationManager } from '../../../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../../../shared/helpers/managers/token-manager';
import { Exercise } from '../../models/exercise';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { Equipment } from 'src/app/equipments/equipment';
import { environment } from 'src/environments/environment.prod';
import { ChildMuscle } from 'src/app/muscles/child-muscle';

@Component({
  selector: 'app-exercise-edit',
  templateUrl: './edit-exercise.component.html',
})
export class ExerciseEditComponent extends EditModelComponent<Exercise> implements OnInit {
  exercise!: Exercise;
  previewUrl: string|null = null;

  readonly exercisesPath = '/exercises';

  constructor(private activatedRoute: ActivatedRoute,  
    private exerciseService: ExerciseService, 
    router: Router,  
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(router, impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  exercisePageType!: "yours"|"internal";
  envProduction = environment;

  ngOnInit(): void {
    const fullPath = this.router.url;
    
    if(fullPath.startsWith('/your-exercise'))
      this.exercisePageType = "yours";
    else
      this.exercisePageType = "internal";

    this.loadData();
  }

  loadData() {
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.id = idParam ? +idParam : 0;
    if (this.id) {
      // Edit mode
      (this.exercisePageType === 'yours' ? 
        this.exerciseService.getUserExerciseById(this.id) :
        this.exerciseService.getInternalExerciseById(this.id)
      )
      .pipe(this.catchLoadDataError(this.exercisesPath))
      .subscribe(result => {
        this.exercise = result;
        this.muscles = result.muscles;
        this.equipments = result.equipments;

        if(this.exercise.image)
          this.previewUrl = this.envProduction.baseUrl + 'resources/' + this.exercise.image;

        this.title = `Edit Exercise '${this.exercise.name}'`;
      });
    }
    else {
      // Add mode
      this.title = "Create new Exercise";
      this.exercise = <Exercise>{};
    }
  }

  onSubmit() {
    if (this.id) {
      // Edit mode
      (this.exercisePageType === 'yours' ? 
        this.exerciseService.updateUserExercise(this.exercise) :
        this.exerciseService.updateInternalExercise(this.exercise)
      )
      .pipe(this.catchError())
      .subscribe(_ => {
        console.log("Exercise " + this.exercise!.id + " has been updated.");

        this.updateMuscles();
        this.updateEquipments();

        this.router.navigate([this.exercisesPath]);
      });
    }
    else {
      // Add mode
      (this.exercisePageType === 'yours' ? 
        this.exerciseService.createUserExercise(this.exercise) :
        this.exerciseService.createInternalExercise(this.exercise)
      )
      .pipe(this.catchError())
      .subscribe(result => {
        this.exercise = result;
        console.log("Exercise " + result.id + " has been created.");

        this.updateMuscles();
        this.updateEquipments();

        this.router.navigate([this.exercisesPath]);
      });
    }

  }

  muscles!: ChildMuscle[]; 
  updateMuscles() {
    var muscleIds = this.muscles.map(m => m.id);
    (this.exercisePageType === 'yours' ? 
      this.exerciseService.updateUserExerciseMuscles(this.exercise.id, muscleIds) :
      this.exerciseService.updateInternalExerciseMuscles(this.exercise.id, muscleIds)
    )
    .pipe(this.catchError())
    .subscribe(_ => {
        console.log("Exercise's muscles have been updated.");
    });
  }

  equipments!: Equipment[]; 
  updateEquipments() {
    var equipmentIds = this.equipments.map(m => m.id);
    (this.exercisePageType === 'yours' ? 
      this.exerciseService.updateUserExerciseEquipments(this.exercise.id, equipmentIds) :
      this.exerciseService.updateInternalExerciseEquipments(this.exercise.id, equipmentIds)
    )
    .pipe(this.catchError())
    .subscribe(_ => {
      console.log("Exercise's equipments have been updated.");
    });
  }

  onPhotoUpload() {
    if(!this.exercise.imageFile){
      this.exercise.image = null;
    }
  }
}