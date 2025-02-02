import { ModelsService } from "../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { ApiResult } from "../shared/models/api-result";
import { ExerciseRecord } from "./exercise-record";
import { ExerciseType } from "../exercises/models/exercise-type";

@Injectable({
    providedIn: 'root'
})
export class ExerciseRecordService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'exercise-records');
    }
    
    getExerciseRecordById(id: number): Observable<ExerciseRecord> {
        return this.webClient.get<ExerciseRecord>(id.toString());
    }

    private getExerciseRecordsHttpParams(exerciseId: number|null, exerciseType: ExerciseType|null, date: Date|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) : HttpParams {
        var httpParams = this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
        
        if(exerciseId){
            httpParams = httpParams.set('exerciseId', exerciseId)
        }
        else if(exerciseType != null || exerciseType != undefined){
            httpParams = httpParams.set('exerciseType', exerciseType)
        }
       
        if(date){
            httpParams = httpParams.set('date', date.toDateString())
        }

        return httpParams;
    }

    getExerciseRecords(exerciseId: number|null, exerciseType: ExerciseType|null, date: Date|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<ExerciseRecord>> {
        return this.webClient.get<ApiResult<ExerciseRecord>>(this.emptyPath, 
            this.getExerciseRecordsHttpParams(exerciseId, exerciseType, date, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateExerciseRecord(exerciseRecord:ExerciseRecord): Observable<Object> {
        return this.webClient.put(`/${exerciseRecord.id}`, exerciseRecord);
    }

    createExerciseRecord(exerciseRecord:ExerciseRecord): Observable<ExerciseRecord>{
        return this.webClient.post<ExerciseRecord>(this.emptyPath, exerciseRecord);
    }

    deleteExerciseRecord(id: number): Observable<Object> {
        return this.webClient.delete(id.toString());
    }
}
