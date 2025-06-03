import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

import { EditModelComponent } from '../../../shared/components/base/edit-model.component';
import { ExerciseService } from '../../services/exercise.service';
import { ImpersonationManager } from '../../../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../../../shared/helpers/managers/token-manager';
import { Exercise } from '../../models/exercise';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { environment } from 'src/environments/environment.prod';
import { ChildMuscle } from 'src/app/muscles/models/child-muscle';
import { Equipment } from 'src/app/equipments/models/equipment';

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
        this.exerciseAliases = result.aliases;

        if(this.exercise.image)
          this.previewUrl = this.envProduction.baseUrl + this.exercise.image;

        this.title = `Edit Exercise '${this.exercise.name}'`;
      });
    }
    else {
      // Add mode
      this.title = "Create new Exercise";
      this.exercise = <Exercise>{};
      this.exerciseAliases = [];
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

        if(this.isPhotoUploaded) {
          (this.exercisePageType === 'yours' ? 
            this.exerciseService.updateUserExercisePhoto(this.exercise!.id, this.photo) :
            this.exerciseService.updateInternalExercisePhoto(this.exercise!.id, this.photo)
          )          
            .pipe(this.catchError())
            .subscribe(_ => {
              console.log("Exercise photo has been updated.");
            });
        }

        if(this.haveMusclesChanged) {
          this.updateMuscles();
        }

        if(this.haveEquipmentsChanged) {
          this.updateEquipments();
        }

        if(this.haveExerciseAliasesChanged) {
          this.updateExerciseAliases();
        }

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

        if(this.isPhotoUploaded && this.photo) {
          (this.exercisePageType === 'yours' ? 
            this.exerciseService.updateUserExercisePhoto(result.id, this.photo) :
            this.exerciseService.updateInternalExercisePhoto(result.id, this.photo)
          )          
            .pipe(this.catchError())
            .subscribe(_ => {
              console.log("Exercise photo has been added.");
            });
        }

         if(this.haveMusclesChanged) {
          this.updateMuscles();
        }

        if(this.haveEquipmentsChanged) {
          this.updateEquipments();
        }

        if(this.haveExerciseAliasesChanged) {
          this.updateExerciseAliases();
        }

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

  exerciseAliases!: string[];
  updateExerciseAliases() {
    (this.exercisePageType === 'yours' ? 
      this.exerciseService.updateUserExerciseAliases(this.exercise.id, this.exerciseAliases) :
      this.exerciseService.updateInternalExerciseAliases(this.exercise.id, this.exerciseAliases)
    )
    .pipe(this.catchError())
    .subscribe(_ => {
      console.log("Exercise aliases have been updated.");
    });
  }

  photo: File | null = null;
  isPhotoUploaded = false;
  onPhotoUpload() {
    this.isPhotoUploaded = true;
  }

  haveMusclesChanged = false;
  onMusclesChanged() {
    this.haveMusclesChanged = true;
  }

  haveEquipmentsChanged = false;
  onEquipmentsChanged() {
    this.haveEquipmentsChanged = true;
  }

  haveExerciseAliasesChanged = false;
  onExerciseAliasesChanged(): void {
    this.haveExerciseAliasesChanged = true;
  }

  isExerciseAliasesValid!: boolean;
  onExerciseAliasesValidityChange(isValid: boolean): void {
    this.isExerciseAliasesValid = isValid;
  }
}