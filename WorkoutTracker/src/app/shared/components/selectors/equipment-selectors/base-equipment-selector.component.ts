import { Component, Input, OnInit } from '@angular/core';
import { map, Observable } from 'rxjs';
import { EquipmentService } from 'src/app/equipments/services/equipment.service';
import { ModelsSelectorComponent } from '../models-selector.component';
import { environment } from 'src/environments/environment.prod';
import { Equipment } from 'src/app/equipments/models/equipment';

@Component({
  template: ''
})
export abstract class BaseEquipmentSelectorComponent<T extends number|Equipment[]> extends ModelsSelectorComponent<T> implements OnInit 
{
  @Input()
  modelsType:"all"|"user"|"internal" = "all";

  equipments!: Observable<Equipment[]>;

  constructor(private equipmentService: EquipmentService) {
    super();
  }
  
  envProduction = environment;

  loadData(){
    this.equipments = 
      (() => {
        switch (this.modelsType) {
          case "user":
            return this.equipmentService.getUserEquipments(this.pageIndex, this.pageSize, this.sortColumn, this.sortOrder, this.filterColumn, this.filterQuery);
          case "internal":
            return this.equipmentService.getInternalEquipments(this.pageIndex, this.pageSize, this.sortColumn, this.sortOrder, this.filterColumn, this.filterQuery);
          default:
            return this.equipmentService.getAllEquipments(this.pageIndex, this.pageSize, this.sortColumn, this.sortOrder, this.filterColumn, this.filterQuery);
        }
      })().pipe(map(x => x.data));
  }
}