import { Component, OnInit } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';

import { Equipment } from './equipment';
import { EquipmentService } from './equipment.service';
import { ApiResult } from '../shared/models/api-result.model';
import { ModelsTableComponent } from '../shared/components/models-table.component';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-equipments',
  templateUrl: './equipments.component.html',
  styleUrls: ['./equipments.component.css']
})
export class EquipmentsComponent extends ModelsTableComponent<Equipment> implements OnInit {
  constructor(private router: Router, public equipmentService: EquipmentService, snackBar: MatSnackBar) {
      super(snackBar);
      this.displayedColumns = ['name', 'actions'];
  }

  equipmentPageType!: "all"|"yours"|"internal";

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Equipment>> {
    switch (this.equipmentPageType) {
      case 'all':
        return this.equipmentService.getAllEquipments(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
      case 'yours':
        return this.equipmentService.getUserEquipments(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
      default:
        return this.equipmentService.getInternalEquipments(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
    }
  }

  ngOnInit() {
    this.loadData();

    const fullPath = this.router.url;
    switch (fullPath) {
      case '/all-equipments':
        this.equipmentPageType = "all";
        break;
      case '/your-equipments':
        this.equipmentPageType = "yours";
        break;
      default:
        this.equipmentPageType = "internal";
        break;
    }
  }

  deleteEquipment(id: number, isOwnedByUser: boolean) {
    (isOwnedByUser ? 
      this.equipmentService.deleteUserEquipment(id) :
      this.equipmentService.deleteInternalEquipment(id)
    )
    .pipe(this.catchError())
    .subscribe(() => {
          this.loadData();
          this.modelDeletedSuccessfully("Equipment");
        })
  }
}
