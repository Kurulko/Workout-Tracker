import { ModelsService } from "../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { BodyWeight } from "./body-weight";
import { ApiResult } from "../shared/models/api-result";

@Injectable({
    providedIn: 'root'
})
export class BodyWeightService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'body-weights');
    }
    
    getBodyWeightById(id: number): Observable<BodyWeight> {
        return this.webClient.get<BodyWeight>(id.toString());
    }

    getMaxBodyWeight(): Observable<BodyWeight> {
        return this.webClient.get<BodyWeight>('max-body-weight');
    }

    getMinBodyWeight(): Observable<BodyWeight> {
        return this.webClient.get<BodyWeight>('min-body-weight');
    }

    private getBodyWeightsHttpParams(date: Date|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) : HttpParams {
        var httpParams = this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
        
        if(date){
            httpParams = httpParams.set('date', date.toDateString())
        }

        return httpParams;
    }

    getBodyWeightsInKilograms(date: Date|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<BodyWeight>> {
        return this.webClient.get<ApiResult<BodyWeight>>('in-kilograms', 
            this.getBodyWeightsHttpParams(date, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    getBodyWeightsInPounds(date: Date|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<BodyWeight>> {
        return this.webClient.get<ApiResult<BodyWeight>>('in-pounds', 
            this.getBodyWeightsHttpParams(date, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateBodyWeight(bodyWeight:BodyWeight): Observable<Object> {
        return this.webClient.put(`/${bodyWeight.id}`, bodyWeight);
    }

    createBodyWeight(bodyWeight:BodyWeight): Observable<BodyWeight>{
        return this.webClient.post<BodyWeight>(this.emptyPath, bodyWeight);
    }

    deleteBodyWeight(id: number): Observable<Object> {
        return this.webClient.delete(id.toString());
    }
}
