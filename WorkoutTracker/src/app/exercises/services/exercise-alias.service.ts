import { ModelsService } from "../../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { ApiResult } from "../../shared/models/api-result";
import { ExerciseAlias } from "../models/exercise-alias";

@Injectable({
    providedIn: 'root'
})
export class ExerciseAliasService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'exercise-aliases');
    }
    
    getExerciseAliasById(id: number): Observable<ExerciseAlias> {
        return this.webClient.get<ExerciseAlias>(id.toString());
    }

    getExerciseAliasByName(name: string): Observable<ExerciseAlias> {
        return this.webClient.get<ExerciseAlias>(`by-name/${name}`);
    }

    getExerciseAliases(exerciseId: number, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<ExerciseAlias>> {
        return this.webClient.get<ApiResult<ExerciseAlias>>(this.emptyPath, 
            this.getExerciseAliasesHttpParams(exerciseId, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    createExerciseAlias(exerciseId: number, exerciseAlias: ExerciseAlias): Observable<ExerciseAlias>{
        return this.webClient.post<ExerciseAlias>(exerciseId.toString(), exerciseAlias);
    }

    updateExerciseAlias(exerciseAlias: ExerciseAlias): Observable<Object> {
        return this.webClient.put(exerciseAlias.id.toString(), exerciseAlias);
    }

    deleteExerciseAlias(id: number): Observable<Object> {
        return this.webClient.delete(id.toString());
    }

    private getExerciseAliasesHttpParams(exerciseId: number, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) : HttpParams {
        var httpParams = this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
        
        httpParams = httpParams.set('exerciseId', exerciseId)

        return httpParams;
    }
}