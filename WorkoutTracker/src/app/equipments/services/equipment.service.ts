import { ModelsService } from "../../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ApiResult } from "../../shared/models/api-result";
import { EquipmentDetails } from "../models/equipment-details";
import { Exercise } from "../../exercises/models/exercise";
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

    updateInternalEquipment(equipment: Equipment): Observable<Object> {
        return this.webClient.put(`internal-equipment/${equipment.id}`, equipment);
    }

    createInternalEquipment(equipment: Equipment): Observable<Equipment> {
        return this.webClient.post<Equipment>("internal-equipment", equipment);
    }

    deleteInternalEquipment(id: number): Observable<Object> {
        return this.webClient.delete(`internal-equipment/${id}`);
    }

    updateInternalEquipmentPhoto(id: number, photo: File | null): Observable<Object> {
        const formData = new FormData();

        if (photo) {
            formData.append('fileUpload', photo);
        }

        return this.webClient.put(`internal-equipment-photo/${id}`, formData);
    }

    deleteInternalEquipmentPhoto(id: number): Observable<Object>{
        return this.webClient.delete(`internal-equipment-photo/${id}`);
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

    updateUserEquipment(equipment: Equipment): Observable<Object> {
        return this.webClient.put(`user-equipment/${equipment.id}`, equipment);
    }

    createUserEquipment(equipment: Equipment): Observable<Equipment>{
        return this.webClient.post<Equipment>("user-equipment", equipment);
    }

    deleteUserEquipment(id: number): Observable<Object> {
        return this.webClient.delete(`user-equipment/${id}`);
    }

    updateUserEquipmentPhoto(id: number, photo: File | null): Observable<Object> {
        const formData = new FormData();

        if (photo) {
            formData.append('fileUpload', photo);
        }

        return this.webClient.put(`user-equipment-photo/${id}`, formData);
    }

    deleteUserEquipmentPhoto(id: number): Observable<Object>{
        return this.webClient.delete(`user-equipment-photo/${id}`);
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
}
