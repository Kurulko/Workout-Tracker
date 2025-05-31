import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

import { EditModelComponent } from '../../../shared/components/base/edit-model.component';
import { MuscleService } from '../../services/muscle.service';
import { ImpersonationManager } from '../../../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../../../shared/helpers/managers/token-manager';
import { PreferencesManager } from '../../../shared/helpers/managers/preferences-manager';
import { environment } from 'src/environments/environment.prod';
import { UploadWithPhoto } from '../../../shared/models/upload-with-photo';
import { ChildMuscle } from '../../models/child-muscle';
import { Muscle } from '../../models/muscle';

@Component({
  selector: 'app-muscle-edit',
  templateUrl: './edit-muscle.component.html',
})
export class MuscleEditComponent extends EditModelComponent<Muscle> implements OnInit {
  muscle!: Muscle;
  previewUrl: string|null = null;

  childMuscles: ChildMuscle[]|null = null; 

  allMuscles!: Muscle[];
  accessibleChildMuscles!: ChildMuscle[];

  readonly musclesPath = "/muscles";

  constructor(private activatedRoute: ActivatedRoute,  
    private muscleService: MuscleService, 
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

  envProduction = environment;
  
  loadData() {
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.id = idParam ? +idParam : 0;
    if (this.id) {
      // Edit mode
      this.muscleService.getMuscleById(this.id)
      .pipe(this.catchLoadDataError(this.musclesPath))
      .subscribe(result => {
        this.muscle = result;
        this.childMuscles = result.childMuscles;

        if(this.muscle.image)
          this.previewUrl = this.envProduction.baseUrl + this.muscle.image;

        this.title = `Edit Muscle '${this.muscle.name}'`;
        this.loadAllMuscles();
      });
    }
    else {
      // Add mode
      this.title = "Create new Muscle";
      this.muscle = <Muscle>{};
      this.loadAllMuscles();
    }
  }

  loadAllMuscles() {
    this.muscleService.getMuscles(null, null, 0, 9999, "name", "asc", null, null)
      .pipe(this.catchError())
      .subscribe(result => {
        this.allMuscles = result.data;
        this.updateAccessibleChildMuscles();
      });
  }

  updateAccessibleChildMuscles() {
    if(this.muscle.parentMuscleId) {
      this.accessibleChildMuscles = this.allMuscles
        .filter(d => (d.parentMuscleId == null || d.parentMuscleId == this.muscle.id) && d.id !== this.muscle.parentMuscleId && d.id !== this.muscle.id);
    }
    else {
      this.accessibleChildMuscles = this.allMuscles.filter(d => d.id !== this.muscle.id);
    }

    if(this.childMuscles && !this.isNoneOptionSelected) {
      this.childMuscles = this.childMuscles.filter(cm => this.accessibleChildMuscles.map(acm => acm.id).includes(cm.id))
    }
  }

  onSubmit() {   
    if (this.id) {
      // Edit mode
      this.muscleService.updateMuscle(this.muscle)
      .pipe(this.catchError())
      .subscribe(_ => {
        console.log("Muscle " + this.muscle!.id + " has been updated.");

        if(this.isPhotoUploaded) {
          this.muscleService.updateMusclePhoto(this.muscle!.id, this.photo)
            .pipe(this.catchError())
            .subscribe(_ => {
              console.log("Muscle photo has been updated.");
            });
        }

        this.updateChildMuscles();
        this.router.navigate([this.musclesPath]);
      });
    }
    else {
      // Add mode
      this.muscleService.createMuscle(this.muscle)
      .pipe(this.catchError())
      .subscribe(result => {
        this.muscle = result;
        console.log("Muscle " + result.id + " has been created.");

        if(this.isPhotoUploaded && this.photo) {
          this.muscleService.updateMusclePhoto(result.id, this.photo)
            .pipe(this.catchError())
            .subscribe(_ => {
              console.log("Muscle photo has been added.");
            });
        }

        this.updateChildMuscles();
        this.router.navigate([this.musclesPath]);
      });
    }
  }

  photo: File | null = null;
  isPhotoUploaded = false;
  onPhotoUpload() {
    this.isPhotoUploaded = true;
  }
  
  updateChildMuscles() {
    if(!this.isNoneOptionSelected && this.childMuscles) {
      var childMuscleIds = this.childMuscles.map(m => m.id);
      this.muscleService.updateMuscleChildren(this.muscle.id, childMuscleIds)
      .pipe(this.catchError())
      .subscribe(_ => {
        console.log("Child muscles have been updated.");
      });
    }
  }

  isDisabledNoneOption(): boolean {
    if(!this.childMuscles)
      return false;

    return this.childMuscles.length > 0 && !this.isNoneOptionSelected;
  }

  isNoneOptionSelected = false;
  noneOptionSelected() {
    this.isNoneOptionSelected = !this.isNoneOptionSelected;
  }

  compareMusclesById(muscle1: Muscle, muscle2: Muscle): boolean {
    return muscle1 && muscle2 ? muscle1.id === muscle2.id : muscle1 === muscle2;
  }
}