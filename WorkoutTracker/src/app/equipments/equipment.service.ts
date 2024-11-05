import { ModelsService } from "../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { TokenManager } from "../shared/helpers/token-manager";
import { Observable } from "rxjs";
import { Equipment } from "./equipment";
import { ApiResult } from "../shared/models/api-result.model";

@Injectable({
    providedIn: 'root'
})
export class EquipmentService extends ModelsService {
    constructor(httpClient: HttpClient, tokenManager: TokenManager) {
        super(httpClient, tokenManager, 'equipments');
    }
    
    getInternalEquipmentById(id: number): Observable<Equipment> {
        return this.webClient.get<Equipment>(`internal-equipment/${id}`);
    }

    getInternalEquipmentByName(name: string): Observable<Equipment> {
        return this.webClient.get<Equipment>(`internal-equipment/by-name/${name}`);
    }

    getInternalEquipments(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Equipment>> {
        return this.webClient.get<ApiResult<Equipment>>("internal-equipments", this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateInternalEquipment(equipment:Equipment): Observable<Object> {
        return this.webClient.put(`internal-equipment/${equipment.id}`, equipment);
    }

    createInternalEquipment(equipment:Equipment): Observable<Equipment>{
        return this.webClient.post<Equipment>("internal-equipment", equipment);
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

    getUserEquipments(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Equipment>> {
        return this.webClient.get<ApiResult<Equipment>>("user-equipments", this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateUserEquipment(equipment:Equipment): Observable<Object> {
        return this.webClient.put(`user-equipment/${equipment.id}`, equipment);
    }

    createUserEquipment(equipment:Equipment): Observable<Equipment>{
        return this.webClient.post<Equipment>("user-equipment", equipment);
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
}
