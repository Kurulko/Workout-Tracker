import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, throwError } from 'rxjs';

import { MuscleSize } from './muscle-size';
import { EditModelComponent } from '../shared/components/edit-model.component';
import { MuscleSizeService } from './muscle-size.service';
import { MatSnackBar } from '@angular/material/snack-bar';

import { catchError, map } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { StatusCodes } from 'http-status-codes';
import { SizeType } from "./size-type";
import { Muscle } from '../muscles/muscle';
import { MuscleService } from '../muscles/muscle.service';

@Component({
  selector: 'app-muscle-size-edit',
  templateUrl: './edit-muscle-size.component.html',
})
export class MuscleSizeEditComponent extends EditModelComponent<MuscleSize>{
  muscleSize: MuscleSize = <MuscleSize>{ date : new Date() };
  sizeTypes = Object.keys(SizeType).filter(key => isNaN(Number(key)));
  maxDate: Date = new Date();

  muscles!: Observable<Muscle[]>;

  constructor(private activatedRoute: ActivatedRoute,  private router: Router,  private muscleSizeService: MuscleSizeService, 
      private muscleService: MuscleService, snackBar: MatSnackBar) {
    super(snackBar);
  }

  getObjectKeys(obj: any): string[] {
    return Object.keys(obj);
  }

  validateInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
  
    const regex = /^\d*\.?\d{0,1}$/;
  
    if (!regex.test(value)) {
      input.value = value.slice(0, -1);
    }
  }

  loadData() {
    this.loadMuscles();

    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.id = idParam ? +idParam : 0;
    if (this.id) {
      // Edit mode
      this.muscleSizeService.getMuscleSizeById(this.id)
      .pipe(catchError((errorResponse: HttpErrorResponse) => {
        console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);

        if (errorResponse.status === StatusCodes.NOT_FOUND) {
            this.router.navigate(['/muscle-sizes']);
        }

        this.showSnackbar(errorResponse.message);
        return throwError(() => errorResponse);
      }))
      .subscribe(result => {
        this.muscleSize = result;

        this.title = `Edit MuscleSize ('${this.muscleSize.id}')`;
      });
    }
    else {
      // Add mode
      this.title = "Create new MuscleSize";
    }
  }

  loadMuscles(){
    this.muscles = this.muscleService.getMuscles(0, 9999, "name", "asc", null, null).pipe(map(x => x.data));
  }


  onSubmit() {
    if (this.id) {
      // Edit mode
      this.muscleSizeService.updateMuscleSize(this.muscleSize)
      .pipe(this.catchError())
      .subscribe(_ => {
          this.modelUpdatedSuccessfully('Muscle Size')
          console.log("MuscleSize " + this.muscleSize!.id + " has been updated.");
          this.router.navigate(['/muscle-sizes']);
      });
    }
    else {
      // Add mode
      this.muscleSizeService.createMuscleSize(this.muscleSize)
      .pipe(this.catchError())
      .subscribe(result => {
          this.modelAddedSuccessfully('Muscle Size')
          console.log("MuscleSize " + result.id + " has been created.");
          this.router.navigate(['/muscle-sizes']);
      });
    }
  }
}