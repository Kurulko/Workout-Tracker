import { ModelsService } from "../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { BodyWeight } from "./body-weight";
import { ApiResult } from "../shared/models/api-result";
import { DateTimeRange } from "../shared/models/date-time-range";

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

    private getBodyWeightsHttpParams(range: DateTimeRange|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) : HttpParams {
        var params = this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
        params = this.getRangeParams(params, range);
        return params;
    }

    getBodyWeightsInKilograms(range: DateTimeRange|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<BodyWeight>> {
        return this.webClient.get<ApiResult<BodyWeight>>('in-kilograms', 
            this.getBodyWeightsHttpParams(range, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    getBodyWeightsInPounds(range: DateTimeRange|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<BodyWeight>> {
        return this.webClient.get<ApiResult<BodyWeight>>('in-pounds', 
            this.getBodyWeightsHttpParams(range, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
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
