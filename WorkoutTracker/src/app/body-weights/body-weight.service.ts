import { ModelsService } from "../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { TokenManager } from "../shared/helpers/token-manager";
import { Observable } from "rxjs";
import { BodyWeight } from "./body-weight";
import { ApiResult } from "../shared/models/api-result.model";

@Injectable({
    providedIn: 'root'
})
export class BodyWeightService extends ModelsService {
    constructor(httpClient: HttpClient, tokenManager: TokenManager) {
        super(httpClient, tokenManager, 'body-weights');
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

    getBodyWeightByDate(date: Date): Observable<BodyWeight> {
        return this.webClient.get<BodyWeight>(`by-date/${date}`);
    }

    getBodyWeights(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<BodyWeight>> {
        return this.webClient.get<ApiResult<BodyWeight>>(this.emptyPath, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateBodyWeight(bodyWeight:BodyWeight): Observable<Object> {
        return this.webClient.put(`/${bodyWeight.id}`, bodyWeight);
    }

    createBodyWeight(bodyWeight:BodyWeight): Observable<BodyWeight>{
        return this.webClient.post<BodyWeight>(this.emptyPath,bodyWeight);
    }

    deleteBodyWeight(id: number): Observable<Object> {
        return this.webClient.delete(id.toString());
    }
}
