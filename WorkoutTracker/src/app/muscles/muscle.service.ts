import { ModelsService } from "../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { TokenManager } from "../shared/helpers/token-manager";
import { Observable } from "rxjs";
import { Muscle } from "./muscle";
import { ApiResult } from "../shared/models/api-result.model";

@Injectable({
    providedIn: 'root'
})
export class MuscleService extends ModelsService {
    constructor(httpClient: HttpClient, tokenManager: TokenManager) {
        super(httpClient, tokenManager, 'muscles');
    }
    
    getMuscleById(id: number): Observable<Muscle> {
        return this.webClient.get<Muscle>(id.toString());
    }

    getMuscleByName(name: string): Observable<Muscle> {
        return this.webClient.get<Muscle>(`by-name/${name}`);
    }

    getMuscles(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Muscle>> {
        return this.webClient.get<ApiResult<Muscle>>(this.emptyPath, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateMuscle(muscle:Muscle): Observable<Object> {
        return this.webClient.put(this.emptyPath, muscle);
    }

    createMuscle(muscle:Muscle): Observable<Muscle>{
        return this.webClient.post<Muscle>(this.emptyPath,muscle);
    }

    deleteMuscle(id: number): Observable<Object> {
        return this.webClient.delete(id.toString());
    }

    muscleExists(id: number): Observable<boolean> {
        return this.webClient.get<boolean>(`muscle-exists/${id}`);
    }

    muscleExistsByName(name: string): Observable<boolean> {
        return this.webClient.get<boolean>(`muscle-exists-by-name/${name}`);
    }
}
