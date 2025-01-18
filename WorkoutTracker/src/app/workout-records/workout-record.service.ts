import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ModelsService } from 'src/app/shared/services/models.service';
import { ApiResult } from 'src/app/shared/models/api-result';
import { WorkoutRecord } from './workout-record';

@Injectable({
    providedIn: 'root'
})
export class WorkoutRecordService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'workout-records');
    }
    
    getWorkoutRecordById(id: number): Observable<WorkoutRecord> {
        return this.webClient.get<WorkoutRecord>(id.toString());
    }

    getWorkoutRecords(workoutId: number|null, date: Date|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<WorkoutRecord>> {
        var httpParams = this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
        
        if(workoutId){
            httpParams = httpParams.set('workoutId', workoutId)
        }
        
        if(date){
            httpParams = httpParams.set('date', date.toDateString())
        }

        return this.webClient.get<ApiResult<WorkoutRecord>>(this.emptyPath, httpParams);
    }


    updateWorkoutRecord(workoutRecord:WorkoutRecord): Observable<Object> {
        return this.webClient.put(`/${workoutRecord.id}`, workoutRecord);
    }

    createWorkoutRecord(workoutRecord:WorkoutRecord): Observable<WorkoutRecord>{
        return this.webClient.post<WorkoutRecord>(this.emptyPath, workoutRecord);
    }

    deleteWorkoutRecord(id: number): Observable<Object> {
        return this.webClient.delete(id.toString());
    }
}
