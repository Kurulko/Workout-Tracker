import { ModelsService } from "../../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ApiResult } from "../../shared/models/api-result";
import { EquipmentDetails } from "../models/equipment-details";
import { Exercise } from "../../exercises/models/exercise";
import { UploadWithPhoto } from "../../shared/models/upload-with-photo";
import { Equipment } from "../models/equipment";

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

    updateInternalEquipment(equipmentWithPhoto: UploadWithPhoto<Equipment>): Observable<Object> {
        const formData = this.toFormData(equipmentWithPhoto);
        return this.webClient.put(`internal-equipment/${equipmentWithPhoto.model.id}`, formData);
    }

    createInternalEquipment(equipmentWithPhoto: UploadWithPhoto<Equipment>): Observable<Equipment> {
        const formData = this.toFormData(equipmentWithPhoto);
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

    updateUserEquipment(equipmentWithPhoto: UploadWithPhoto<Equipment>): Observable<Object> {
        const formData = this.toFormData(equipmentWithPhoto);
        return this.webClient.put(`user-equipment/${equipmentWithPhoto.model.id}`, formData);
    }

    createUserEquipment(equipmentWithPhoto: UploadWithPhoto<Equipment>): Observable<Equipment>{
        const formData = this.toFormData(equipmentWithPhoto);
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

    getEquipmentById(id: number): Observable<Equipment> {
        return this.webClient.get<Equipment>(`equipment/${id}`);
    }

    getEquipmentByName(name: string): Observable<Equipment> {
        return this.webClient.get<Equipment>(`equipment/by-name/${name}`);
    }

    getAllEquipments(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Equipment>> {
        return this.webClient.get<ApiResult<Equipment>>("all-equipments", this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    getUsedEquipments(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Equipment>> {
        return this.webClient.get<ApiResult<Equipment>>("used-equipments", this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    getEquipmentExercises(equipmentId:number, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Exercise>> {
        return this.webClient.get<ApiResult<Exercise>>(`${equipmentId}/exercises`, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    private toFormData(equipmentWithPhoto: UploadWithPhoto<Equipment>): FormData {
        let formData = new FormData();
    
        const { model: equipment, photo } = equipmentWithPhoto;

        const prefix = 'model.';

        if(equipment.id) {
            formData.append(`${prefix}id`, equipment.id.toString());
        }

        formData.append(`${prefix}name`, equipment.name);

        if (equipment.image) {
            formData.append(`${prefix}image`, equipment.image);
        }

        if (photo) {
            formData.append('photo', photo, photo.name);
        }
    
        return formData;
    }
}
