import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { throwError } from 'rxjs';

import { Equipment } from './equipment';
import { EditModelComponent } from '../shared/components/edit-model.component';
import { EquipmentService } from './equipment.service';
import { MatSnackBar } from '@angular/material/snack-bar';

import { catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { StatusCodes } from 'http-status-codes';

@Component({
  selector: 'app-equipment-edit',
  templateUrl: './edit-equipment.component.html',
})
export class EquipmentEditComponent extends EditModelComponent<Equipment> {
  equipment: Equipment = <Equipment>{};

  constructor(private activatedRoute: ActivatedRoute,  private router: Router,  private equipmentService: EquipmentService, snackBar: MatSnackBar) {
    super(snackBar);
  }

  loadData() {
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.id = idParam ? +idParam : 0;
    if (this.id) {
      // Edit mode
      this.equipmentService.getEquipmentById(this.id)
      .pipe(catchError((errorResponse: HttpErrorResponse) => {
        console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);

        if (errorResponse.status === StatusCodes.NOT_FOUND) {
            this.router.navigate(['/equipments']);
        }

        this.showSnackbar(errorResponse.message);
        return throwError(() => errorResponse);
      }))
      .subscribe(result => {
        this.equipment = result;
        this.title = `Edit Equipment '${this.equipment.name}'`;
      });
    }
    else {
      // Add mode
      this.title = "Create new Equipment";
    }
  }

  onSubmit() {
    if (this.id) {
      // Edit mode
      this.equipmentService.updateEquipment(this.equipment)
      .pipe(this.catchError())
      .subscribe(_ => {
          console.log("Equipment " + this.equipment!.id + " has been updated.");
          this.router.navigate(['/equipments']);
      });
    }
    else {
      // Add mode
      this.equipmentService.createEquipment(this.equipment)
      .pipe(this.catchError())
      .subscribe(result => {
          console.log("Equipment " + result.id + " has been created.");
          this.router.navigate(['/equipments']);
      });
    }
  }
}