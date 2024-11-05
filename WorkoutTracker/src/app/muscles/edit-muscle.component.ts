import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatestWith, of, Observable, throwError } from 'rxjs';
import { map } from 'rxjs/operators';

import { Muscle } from './muscle';
import { EditModelComponent } from '../shared/components/edit-model.component';
import { MuscleService } from './muscle.service';
import { MatSnackBar } from '@angular/material/snack-bar';

import { catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { StatusCodes } from 'http-status-codes';

@Component({
  selector: 'app-muscle-edit',
  templateUrl: './edit-muscle.component.html',
})
export class MuscleEditComponent extends EditModelComponent<Muscle>{
  muscle: Muscle = <Muscle>{};

  childMuscles!: Observable<Muscle[]>;
  parentMuscles!: Observable<Muscle[]>;

  constructor(private activatedRoute: ActivatedRoute,  private router: Router,  private muscleService: MuscleService, snackBar: MatSnackBar) {
    super(snackBar);
  }

  loadData() {
    this.loadMuscles();

    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.id = idParam ? +idParam : 0;
    if (this.id) {
      // Edit mode
      this.muscleService.getMuscleById(this.id)
      .pipe(catchError((errorResponse: HttpErrorResponse) => {
        console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);

        if (errorResponse.status === StatusCodes.NOT_FOUND) {
            this.router.navigate(['/muscles']);
        }

        this.showSnackbar(errorResponse.message);
        return throwError(() => errorResponse);
      }))
      .subscribe(result => {
        this.muscle = result;

        if(this.muscle.childMuscles)
          this.childMuscles = this.childMuscles.pipe(
            combineLatestWith(of(this.muscle.childMuscles)),
            map(([muscles, additionalMuscles]) => [...muscles, ...additionalMuscles]));

        this.title = `Edit Muscle '${this.muscle.name}'`;
      });
    }
    else {
      // Add mode
      this.title = "Create new Muscle";
    }
  }

  loadMuscles(){
    this.parentMuscles = this.muscleService.getMuscles(0, 9999, "name", "asc", null, null).pipe(map(x => x.data));
    this.childMuscles = this.parentMuscles
      .pipe(map(muscles => muscles.filter(muscle => muscle.parentMuscle === null && muscle.id !== this.muscle.parentMuscleId)));
  }

  onSubmit() {
    if(this.muscle.childMuscles){
      this.muscle.childMuscles = this.muscle.childMuscles.map(childMuscle => {
        childMuscle.parentMuscleId = this.muscle.id;
        childMuscle.childMuscles = null;
        return childMuscle;
      })
    }

    if (this.id) {
      // Edit mode
      this.muscleService.updateMuscle(this.muscle)
      .pipe(this.catchError())
      .subscribe(_ => {
          console.log("Muscle " + this.muscle!.id + " has been updated.");
          this.router.navigate(['/muscles']);
      });
    }
    else {
      // Add mode
      this.muscleService.createMuscle(this.muscle)
      .pipe(this.catchError())
      .subscribe(result => {
          console.log("Muscle " + result.id + " has been created.");
          this.router.navigate(['/muscles']);
      });
    }
  }

  getChildrenMuscleNames(): string|undefined {
    return this.muscle.childMuscles?.map(child => child.name).join(', ');
  }

  selectedParentMuscle(){
    this.childMuscles = this.parentMuscles
      .pipe(map(muscles => muscles.filter(muscle => muscle.parentMuscle === null && muscle.id !== this.muscle.parentMuscleId)));
  }
}