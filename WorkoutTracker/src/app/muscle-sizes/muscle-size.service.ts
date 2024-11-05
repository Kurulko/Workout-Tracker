import { ModelsService } from "../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { TokenManager } from "../shared/helpers/token-manager";
import { Observable } from "rxjs";
import { MuscleSize } from "./muscle-size";
import { ApiResult } from "../shared/models/api-result.model";

@Injectable({
    providedIn: 'root'
})
export class MuscleSizeService extends ModelsService {
    constructor(httpClient: HttpClient, tokenManager: TokenManager) {
        super(httpClient, tokenManager, 'muscle-sizes');
    }
    
    getMuscleSizeById(id: number): Observable<MuscleSize> {
        return this.webClient.get<MuscleSize>(id.toString());
    }

    getMaxMuscleSize(muscleId: number): Observable<MuscleSize> {
        return this.webClient.get<MuscleSize>(`max-muscle-size?muscleId=${muscleId}`);
    }

    getMinMuscleSize(muscleId: number): Observable<MuscleSize> {
        return this.webClient.get<MuscleSize>(`min-muscle-size?muscleId=${muscleId}`);
    }

    getMuscleSizeByDate(muscleId: number, date: Date): Observable<MuscleSize> {
        return this.webClient.get<MuscleSize>(`by-date?date=${date}&muscleId=${muscleId}`);
    }

    getMuscleSizes(muscleId: number, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<MuscleSize>> {
        return this.webClient.get<ApiResult<MuscleSize>>(`?muscleId=${muscleId}`, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    getMuscleSizesInCentimeters(muscleId: number, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<MuscleSize>> {
        return this.webClient.get<ApiResult<MuscleSize>>(`in-centimeters?muscleId=${muscleId}`, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    getMuscleSizesInInches(muscleId: number, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<MuscleSize>> {
        return this.webClient.get<ApiResult<MuscleSize>>(`in-inches?muscleId=${muscleId}`, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateMuscleSize(bodyWeight:MuscleSize): Observable<Object> {
        return this.webClient.put(`/${bodyWeight.id}`, bodyWeight);
    }

    createMuscleSize(bodyWeight:MuscleSize): Observable<MuscleSize>{
        return this.webClient.post<MuscleSize>(this.emptyPath, bodyWeight);
    }

    deleteMuscleSize(id: number): Observable<Object> {
        return this.webClient.delete(id.toString());
    }
}
