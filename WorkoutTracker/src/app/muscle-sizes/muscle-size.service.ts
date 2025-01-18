import { ModelsService } from "../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { MuscleSize } from "./muscle-size";
import { ApiResult } from "../shared/models/api-result";

@Injectable({
    providedIn: 'root'
})
export class MuscleSizeService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'muscle-sizes');
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

    private getMuscleSizesHttpParams(muscleId: number|null, date: Date|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) : HttpParams {
        var httpParams = this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
        
        if(muscleId){
            httpParams = httpParams.set('muscleId', muscleId)
        }

        if(date){
            httpParams = httpParams.set('date', date.toDateString())
        }

        return httpParams;
    }
    
    getMuscleSizesInCentimeters(muscleId: number|null, date: Date|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<MuscleSize>> {
      
        return this.webClient.get<ApiResult<MuscleSize>>("in-centimeters", this.getMuscleSizesHttpParams(muscleId, date, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    getMuscleSizesInInches(muscleId: number|null, date: Date|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<MuscleSize>> {
        return this.webClient.get<ApiResult<MuscleSize>>("in-inches", this.getMuscleSizesHttpParams(muscleId, date, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
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
