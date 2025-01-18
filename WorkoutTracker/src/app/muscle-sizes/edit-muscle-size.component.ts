import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { MuscleSize } from './muscle-size';
import { EditModelComponent } from '../shared/components/base/edit-model.component';
import { MuscleSizeService } from './muscle-size.service';
import { MatSnackBar } from '@angular/material/snack-bar';

import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';
import { ModelSize } from '../shared/models/model-size';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';

@Component({
  selector: 'app-muscle-size-edit',
  templateUrl: './edit-muscle-size.component.html',
})
export class MuscleSizeEditComponent extends EditModelComponent<MuscleSize> implements OnInit {
  muscleSize!: MuscleSize;
  maxDate: Date = new Date();

  readonly muscleSizesPath = "/muscle-sizes";

  constructor(
    private activatedRoute: ActivatedRoute,  
    private muscleSizeService: MuscleSizeService, 
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
      this.muscleSizeService.getMuscleSizeById(this.id)
      .pipe(this.catchLoadDataError(this.muscleSizesPath))
      .subscribe(result => {
        this.muscleSize = result;
        this.title = `Edit Muscle Size`;
      });
    }
    else {
      // Add mode
      this.title = "Create new Muscle Size";
      this.muscleSize = <MuscleSize>{ date : new Date(), size: <ModelSize>{} };
      
      if(this.preferencesManager.hasPreferableSizeType())
        this.muscleSize.size.sizeType = this.preferencesManager.getPreferableSizeType()!;
    }
  }

  onSubmit() {
    if (this.id) {
      // Edit mode
      this.muscleSizeService.updateMuscleSize(this.muscleSize)
      .pipe(this.catchError())
      .subscribe(_ => {
        this.modelUpdatedSuccessfully('Muscle Size')
        console.log("MuscleSize " + this.muscleSize!.id + " has been updated.");
        this.router.navigate([this.muscleSizesPath]);
      });
    }
    else {
      // Add mode
      this.muscleSizeService.createMuscleSize(this.muscleSize)
      .pipe(this.catchError())
      .subscribe(result => {
        this.modelAddedSuccessfully('Muscle Size')
        console.log("MuscleSize " + result.id + " has been created.");
        this.router.navigate([this.muscleSizesPath]);
      });
    }
  }
}