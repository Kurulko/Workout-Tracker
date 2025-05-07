import { ModelsService } from "../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Muscle } from "./muscle";
import { ApiResult } from "../shared/models/api-result";
import { MuscleDetails } from "./muscle-details";
import { Exercise } from "../exercises/models/exercise";
import { UploadWithPhoto } from "../shared/models/upload-with-photo";

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

    updateMuscle(muscleWithPhoto: UploadWithPhoto<Muscle>): Observable<Object> {
        const formData = this.toFormData(muscleWithPhoto);
        return this.webClient.put(`/${muscleWithPhoto.model.id}`, formData);
    }

    updateMuscleChildren(muscleId: number, muscleChildIds: number[]): Observable<Object> {
        return this.webClient.put(`/${muscleId}/children`, muscleChildIds);
    }

    createMuscle(muscleWithPhoto: UploadWithPhoto<Muscle>): Observable<Muscle>{
        const formData = this.toFormData(muscleWithPhoto);
        return this.webClient.post<Muscle>(this.emptyPath, formData);
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

    private toFormData(muscleWithPhoto: UploadWithPhoto<Muscle>): FormData {
        let formData = new FormData();
        
        const { model: muscle, photo } = muscleWithPhoto;

        const prefix = 'model.';

        if(muscle.id) {
            formData.append(`${prefix}id`, muscle.id.toString());
        }

        formData.append(`${prefix}name`, muscle.name);
      
        if (muscle.parentMuscleId) {
            formData.append(`${prefix}parentMuscleId`, muscle.parentMuscleId.toString());
        }

        if (muscle.image) {
            formData.append(`${prefix}image`, muscle.image);
        }

        if (photo) {
            formData.append('photo', photo, photo.name);
        }
    
        return formData;
    }
}
