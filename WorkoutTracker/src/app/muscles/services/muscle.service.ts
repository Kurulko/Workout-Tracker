import { ModelsService } from "../../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ApiResult } from "../../shared/models/api-result";
import { MuscleDetails } from "../models/muscle-details";
import { Exercise } from "../../exercises/models/exercise";
import { UploadWithPhoto } from "../../shared/models/upload-with-photo";
import { Muscle } from "../models/muscle";

@Injectable({
    providedIn: 'root'
})
export class MuscleService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'muscles');
    }
    
    getMuscleById(id: number): Observable<Muscle> {
        return this.webClient.get<Muscle>(id.toString());
    }

    getMuscleByName(name: string): Observable<Muscle> {
        return this.webClient.get<Muscle>(`by-name/${name}`);
    }

    getMuscleDetailsById(id: number): Observable<MuscleDetails> {
        return this.webClient.get<MuscleDetails>(`${id}/details`);
    }

    getMuscleDetailsByName(name: string): Observable<MuscleDetails> {
        return this.webClient.get<MuscleDetails>(`by-name/${name}/details`);
    }

    getMuscles(parentMuscleId: number|null, isMeasurable: boolean|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Muscle>> {
        var httpParams = this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
        
        if(parentMuscleId){
            httpParams = httpParams.set('parentMuscleId', parentMuscleId)
        }

        if(isMeasurable){
            httpParams = httpParams.set('isMeasurable', isMeasurable)
        }

        return this.webClient.get<ApiResult<Muscle>>(this.emptyPath, httpParams);
    }

    getParentMuscles(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Muscle>> {
        return this.webClient.get<ApiResult<Muscle>>("parent-muscles", this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    getChildMuscles(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Muscle>> {
        return this.webClient.get<ApiResult<Muscle>>("child-muscles", this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    getMuscleExercises(muscleId: number|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Exercise>> {
        var httpParams = this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
        return this.webClient.get<ApiResult<Exercise>>(`${muscleId}/exercises`, httpParams);
    }

    updateMuscle(muscle: Muscle): Observable<Object> {
        return this.webClient.put(`/${muscle.id}`, muscle);
    }

    updateMuscleChildren(muscleId: number, muscleChildIds: number[]): Observable<Object> {
        return this.webClient.put(`/${muscleId}/children`, muscleChildIds);
    }

    createMuscle(muscle: Muscle): Observable<Muscle>{
        return this.webClient.post<Muscle>(this.emptyPath, muscle);
    }

    deleteMuscle(id: number): Observable<Object> {
        return this.webClient.delete(id.toString());
    }

    updateMusclePhoto(id: number, photo: File | null): Observable<Object> {
        const formData = new FormData();

        if (photo) {
            formData.append('fileUpload', photo);
        }

        return this.webClient.put(`muscle-photo/${id}`, formData);
    }

    deleteMusclePhoto(id: number): Observable<Object>{
        return this.webClient.delete(`muscle-photo/${id}`);
    }
}
