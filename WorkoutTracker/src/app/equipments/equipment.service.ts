import { ModelsService } from "../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Equipment } from "./equipment";
import { ApiResult } from "../shared/models/api-result";
import { EquipmentDetails } from "./equipment-details";
import { Exercise } from "../exercises/models/exercise";

@Injectable({
    providedIn: 'root'
})
export class EquipmentService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'equipments');
    }
    
    getInternalEquipmentById(id: number): Observable<Equipment> {
        return this.webClient.get<Equipment>(`internal-equipment/${id}`);
    }

    getInternalEquipmentByName(name: string): Observable<Equipment> {
        return this.webClient.get<Equipment>(`internal-equipment/by-name/${name}`);
    }

    getInternalEquipmentDetailsById(id: number): Observable<EquipmentDetails> {
        return this.webClient.get<EquipmentDetails>(`internal-equipment/${id}/details`);
    }

    getInternalEquipmentDetailsByName(name: string): Observable<EquipmentDetails> {
        return this.webClient.get<EquipmentDetails>(`internal-equipment/by-name/${name}/details`);
    }

    getInternalEquipments(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Equipment>> {
        return this.webClient.get<ApiResult<Equipment>>("internal-equipments", this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateInternalEquipment(equipment:Equipment): Observable<Object> {
        const formData = this.toFormData(equipment);
        return this.webClient.put(`internal-equipment/${equipment.id}`, formData);
    }

    createInternalEquipment(equipment:Equipment): Observable<Equipment> {
        const formData = this.toFormData(equipment);
        return this.webClient.post<Equipment>("internal-equipment", formData);
    }

    deleteInternalEquipment(id: number): Observable<Object> {
        return this.webClient.delete(`internal-equipment/${id}`);
    }

    internalEquipmentExists(id: number): Observable<boolean> {
        return this.webClient.get<boolean>(`internal-equipment-exists/${id}`);
    }

    internalEquipmentExistsByName(name: string): Observable<boolean> {
        return this.webClient.get<boolean>(`internal-equipment-exists-by-name/${name}`);
    }


    getUserEquipmentById(id: number): Observable<Equipment> {
        return this.webClient.get<Equipment>(`user-equipment/${id}`);
    }

    getUserEquipmentByName(name: string): Observable<Equipment> {
        return this.webClient.get<Equipment>(`user-equipment/by-name/${name}`);
    }

    getUserEquipmentDetailsById(id: number): Observable<EquipmentDetails> {
        return this.webClient.get<EquipmentDetails>(`user-equipment/${id}/details`);
    }

    getUserEquipmentDetailsByName(name: string): Observable<EquipmentDetails> {
        return this.webClient.get<EquipmentDetails>(`user-equipment/by-name/${name}/details`);
    }
    
    getUserEquipments(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Equipment>> {
        return this.webClient.get<ApiResult<Equipment>>("user-equipments", this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateUserEquipment(equipment:Equipment): Observable<Object> {
        const formData = this.toFormData(equipment);
        return this.webClient.put(`user-equipment/${equipment.id}`, formData);
    }

    createUserEquipment(equipment:Equipment): Observable<Equipment>{
        const formData = this.toFormData(equipment);
        return this.webClient.post<Equipment>("user-equipment", formData);
    }

    deleteUserEquipment(id: number): Observable<Object> {
        return this.webClient.delete(`user-equipment/${id}`);
    }

    userEquipmentExists(id: number): Observable<boolean> {
        return this.webClient.get<boolean>(`user-equipment-exists/${id}`);
    }

    userEquipmentExistsByName(name: string): Observable<boolean> {
        return this.webClient.get<boolean>(`user-equipment-exists-by-name/${name}`);
    }

    getAllEquipments(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Equipment>> {
        return this.webClient.get<ApiResult<Equipment>>("all-equipments", this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    getEquipmentExercises(equipmentId:number, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Exercise>> {
        return this.webClient.get<ApiResult<Exercise>>(`${equipmentId}/exercises`, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    private toFormData(equipment: Equipment): FormData {
        let formData = new FormData();
    
        if(equipment.id) {
            formData.append('id', equipment.id.toString());
        }

        formData.append('name', equipment.name);

        if (equipment.image) {
            formData.append('image', equipment.image);
        }

        if (equipment.imageFile) {
            formData.append('imageFile', equipment.imageFile, equipment.imageFile.name);
        }
    
        return formData;
    }
}
