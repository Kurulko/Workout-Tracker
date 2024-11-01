import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatestWith, of, Observable, throwError } from 'rxjs';
import { map } from 'rxjs/operators';

import { BodyWeight } from './body-weight';
import { EditModelComponent } from '../shared/components/edit-model.component';
import { BodyWeightService } from './body-weight.service';
import { MatSnackBar } from '@angular/material/snack-bar';

import { catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { StatusCodes } from 'http-status-codes';
import { WeightType } from "./weight-type";

@Component({
  selector: 'app-body-weight-edit',
  templateUrl: './edit-body-weight.component.html',
})
export class BodyWeightEditComponent extends EditModelComponent<BodyWeight>{
  bodyWeight: BodyWeight = <BodyWeight>{};
  weightTypes = Object.keys(WeightType).filter(key => isNaN(Number(key)));
  maxDate: Date = new Date();

  constructor(private activatedRoute: ActivatedRoute,  private router: Router,  private bodyWeightService: BodyWeightService, snackBar: MatSnackBar) {
    super(snackBar);
  }

  loadData() {
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.id = idParam ? +idParam : 0;
    if (this.id) {
      // Edit mode
      this.bodyWeightService.getBodyWeightById(this.id)
      .pipe(catchError((errorResponse: HttpErrorResponse) => {
        console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);

        if (errorResponse.status === StatusCodes.NOT_FOUND) {
            this.router.navigate(['/body-weights']);
        }

        this.errorMessage = errorResponse.message;
        this.showSnackbar(this.errorMessage);
        return throwError(() => errorResponse);
      }))
      .subscribe(result => {
        this.bodyWeight = result;

        this.title = `Edit BodyWeight ('${this.bodyWeight.id}')`;
      });
    }
    else {
      // Add mode
      this.title = "Create new BodyWeight";
    }
  }


  onSubmit() {
    if (this.id) {
      // Edit mode
      this.bodyWeightService.updateBodyWeight(this.bodyWeight)
      .pipe(this.catchError())
      .subscribe(_ => {
          console.log("BodyWeight " + this.bodyWeight!.id + " has been updated.");
          this.router.navigate(['/body-weights']);
      });
    }
    else {
      // Add mode
      this.bodyWeightService.createBodyWeight(this.bodyWeight)
      .pipe(this.catchError())
      .subscribe(result => {
          console.log("BodyWeight " + result.id + " has been created.");
          this.router.navigate(['/body-weights']);
      });
    }
  }
}