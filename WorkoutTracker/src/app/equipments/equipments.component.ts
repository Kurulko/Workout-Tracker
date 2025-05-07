import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';

import { Equipment } from './equipment';
import { EquipmentService } from './equipment.service';
import { ApiResult } from '../shared/models/api-result';
import { ModelsTableComponent } from '../shared/components/base/models-table.component';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';
import { environment } from 'src/environments/environment.prod';
import { UploadWithPhoto } from '../shared/models/upload-with-photo';

@Component({
  selector: 'app-equipments',
  templateUrl: './equipments.component.html',
  styleUrls: ['./equipments.component.css']
})
export class EquipmentsComponent extends ModelsTableComponent<Equipment> implements OnInit {
  constructor(public equipmentService: EquipmentService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.displayedColumns = ['index', 'name', 'photo', 'actions'];
    this.filterColumn = "name";
    this.sortColumn = 'name';
  }

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  equipmentPageType: "all"|"yours"|"internal"|"used" = "all";
  envProduction = environment;

  onEquipmentTabChange(event: any): void {
    const index = event.index;

    if (index === 0) 
      this.equipmentPageType = 'all';
    else if (index === 1) 
      this.equipmentPageType = 'used';
    else if (index === 2) 
      this.equipmentPageType = 'yours';
    else if (index === 3) 
      this.equipmentPageType = 'internal';

    this.loadData();
  }

  getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Equipment>> {
    switch (this.equipmentPageType) {
      case 'all':
        return this.equipmentService.getAllEquipments(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
      case 'yours':
        return this.equipmentService.getUserEquipments(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
      case 'internal':
        return this.equipmentService.getInternalEquipments(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
      case 'used':
        return this.equipmentService.getUsedEquipments(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
      default:
        throw new Error(`Unexpected equipment type page value`);
    }
  }

  getData(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;

    this.loadEquipments();
  }

  loadEquipments(){
    if(this.sort){
      this.sortColumn = this.sort.active;
      this.sortOrder = this.sort.direction as 'asc'|'desc';
    }

    this.loadData();
  }

  ngOnInit() {
    this.loadEquipments();
  }

  private editingEquipmentId: number | null = null;
  editingEquipment: Equipment | null = null;
  editingEquipmentPhoto: File | null = null;

  isEditingEquipment(id: number): boolean {
    return this.editingEquipmentId === id;
  }

  startEditingEquipment(equipment: Equipment): void {
    this.editingEquipmentId = equipment.id;
    this.editingEquipment = {...equipment};
  }

  cancelEditingEquipment(): void {
    this.editingEquipmentId = null;
    this.editingEquipment = null;
  }

  isEditingNameValid: boolean = true;
  onNameChange(nameEditing: any): void {
    this.isEditingNameValid = nameEditing.valid; 
  }

  isEditingPhotoValid: boolean = true;
  onPhotoChange(photoEditing: any): void {
    if(!this.editingEquipmentPhoto){
      this.editingEquipment!.image = null;
    }

    this.isEditingPhotoValid = photoEditing.valid; 
  }

  saveEquipment(): void {
    var equipmentWithPhoto = <UploadWithPhoto<Equipment>>{model: this.editingEquipment, photo: this.editingEquipmentPhoto};
    
    (this.equipmentPageType === 'yours' ? 
      this.equipmentService.updateUserEquipment(equipmentWithPhoto) :
      this.equipmentService.updateInternalEquipment(equipmentWithPhoto)
    )
    .pipe(this.catchError())
    .subscribe(_ => {
      console.log("Equipment " + this.editingEquipment!.id + " has been updated.");
      this.cancelEditingEquipment();
      this.loadData();
    });
  }

 
  isAddingEquipment: boolean = false;
  addingEquipmentName: string | null = null;
  addingEquipmentPhoto: File | null = null;

  startAddingEquipment(): void {
    this.isAddingEquipment = true;
  }

  cancelAddingEquipment(): void {
    this.isAddingEquipment = false;
    this.addingEquipmentName = null;
    this.addingEquipmentPhoto = null;
  }

  addEquipment(): void {
    var equipment = <Equipment>{ name: this.addingEquipmentName };
    var equipmentWithPhoto = <UploadWithPhoto<Equipment>>{model: equipment, photo: this.addingEquipmentPhoto};

    (this.equipmentPageType === 'yours' ? 
      this.equipmentService.createUserEquipment(equipmentWithPhoto) :
      this.equipmentService.createInternalEquipment(equipmentWithPhoto)
    )
    .pipe(this.catchError())
    .subscribe(result => {
        console.log("Equipment " + result.id + " has been created.");
        this.cancelAddingEquipment();
        this.loadData();
    });
  }

  
  deleteItem = async (id: number): Promise<void> => {
    var equipment = this.data.find(e => e.id == id);

    (equipment!.isOwnedByUser ? 
      this.equipmentService.deleteUserEquipment(id) :
      this.equipmentService.deleteInternalEquipment(id)
    )
    .pipe(this.catchError())
    .subscribe(() => {
      this.loadData();
      this.modelDeletedSuccessfully("Equipment");
    })
  };

  getPreviewUrl(equipment: Equipment): string|null {
    if(!equipment.image)
      return null;

    return this.envProduction.baseUrl + 'resources/' + equipment.image;
  }
}
