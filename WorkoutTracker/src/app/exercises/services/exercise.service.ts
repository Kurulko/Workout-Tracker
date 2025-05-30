import { ModelsService } from "../../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { ApiResult } from "../../shared/models/api-result";
import { ExerciseType } from "../models/exercise-type";
import { Exercise } from "../models/exercise";
import { ExerciseDetails } from "../models/exercise-details";
import { UploadWithPhoto } from "src/app/shared/models/upload-with-photo";
import { Workout } from "src/app/workouts/models/workout";

@Injectable({
    providedIn: 'root'
})
export class ExerciseService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'exercises');
    }
    
    getInternalExerciseById(id: number): Observable<Exercise> {
        return this.webClient.get<Exercise>(`internal-exercise/${id}`);
    }

    getInternalExerciseByName(name: string): Observable<Exercise> {
        return this.webClient.get<Exercise>(`internal-exercise/by-name/${name}`);
    }

    getInternalExerciseDetailsById(id: number): Observable<ExerciseDetails> {
        return this.webClient.get<ExerciseDetails>(`internal-exercise/${id}/details`);
    }

    getInternalExerciseDetailsByName(name: string): Observable<ExerciseDetails> {
        return this.webClient.get<ExerciseDetails>(`internal-exercise/by-name/${name}/details`);
    }

    getInternalExercises(type: ExerciseType|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Exercise>> {
        return this.webClient.get<ApiResult<Exercise>>("internal-exercises", this.getExercisesHttpParams(type, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateInternalExercise(exerciseWithPhoto: UploadWithPhoto<Exercise>): Observable<Object> {
        const formData = this.toFormData(exerciseWithPhoto);
        return this.webClient.put(`internal-exercise/${exerciseWithPhoto.model.id}`, formData);
    }

    updateInternalExerciseMuscles(exerciseId: number, muscleIds: number[]): Observable<Object> {
        return this.webClient.put(`internal-exercise/${exerciseId}/muscles`, muscleIds);
    }

    updateInternalExerciseEquipments(exerciseId: number, equipmentIds: number[]): Observable<Object> {
        return this.webClient.put(`internal-exercise/${exerciseId}/equipments`, equipmentIds);
    }

    createInternalExercise(exerciseWithPhoto: UploadWithPhoto<Exercise>): Observable<Exercise>{
        const formData = this.toFormData(exerciseWithPhoto);
        return this.webClient.post<Exercise>("internal-exercise", formData);
    }

    deleteInternalExercise(id: number): Observable<Object> {
        return this.webClient.delete(`internal-exercise/${id}`);
    }

    internalExerciseExists(id: number): Observable<boolean> {
        return this.webClient.get<boolean>(`internal-exercise-exists/${id}`);
    }

    internalExerciseExistsByName(name: string): Observable<boolean> {
        return this.webClient.get<boolean>(`internal-exercise-exists-by-name/${name}`);
    }


    getUserExerciseById(id: number): Observable<Exercise> {
        return this.webClient.get<Exercise>(`user-exercise/${id}`);
    }

    getUserExerciseByName(name: string): Observable<Exercise> {
        return this.webClient.get<Exercise>(`user-exercise/by-name/${name}`);
    }

    getUserExerciseDetailsById(id: number): Observable<ExerciseDetails> {
        return this.webClient.get<ExerciseDetails>(`user-exercise/${id}/details`);
    }

    getUserExerciseDetailsByName(name: string): Observable<ExerciseDetails> {
        return this.webClient.get<ExerciseDetails>(`user-exercise/by-name/${name}/details`);
    }

    getUserExercises(type: ExerciseType|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Exercise>> {
        return this.webClient.get<ApiResult<Exercise>>("user-exercises", this.getExercisesHttpParams(type, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateUserExercise(exerciseWithPhoto: UploadWithPhoto<Exercise>): Observable<Object> {
        const formData = this.toFormData(exerciseWithPhoto);
        return this.webClient.put(`user-exercise/${exerciseWithPhoto.model.id}`, formData);
    }

    updateUserExerciseMuscles(exerciseId: number, muscleIds: number[]): Observable<Object> {
        return this.webClient.put(`user-exercise/${exerciseId}/muscles`, muscleIds);
    }

    updateUserExerciseEquipments(exerciseId: number, equipmentIds: number[]): Observable<Object> {
        return this.webClient.put(`user-exercise/${exerciseId}/equipments`, equipmentIds);
    }

    createUserExercise(exerciseWithPhoto: UploadWithPhoto<Exercise>): Observable<Exercise>{
        const formData = this.toFormData(exerciseWithPhoto);
        return this.webClient.post<Exercise>("user-exercise", formData);
    }

    deleteUserExercise(id: number): Observable<Object> {
        return this.webClient.delete(`user-exercise/${id}`);
    }

    userExerciseExists(id: number): Observable<boolean> {
        return this.webClient.get<boolean>(`user-exercise-exists/${id}`);
    }

    userExerciseExistsByName(name: string): Observable<boolean> {
        return this.webClient.get<boolean>(`user-exercise-exists-by-name/${name}`);
    }

    getExerciseById(id: number): Observable<Exercise> {
        return this.webClient.get<Exercise>(`exercise/${id}`);
    }

    getExerciseByName(name: string): Observable<Exercise> {
        return this.webClient.get<Exercise>(`exercise/by-name/${name}`);
    }

    getAllExercises(type: ExerciseType|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Exercise>> {
        return this.webClient.get<ApiResult<Exercise>>("all-exercises", this.getExercisesHttpParams(type, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    getUsedExercises(type: ExerciseType|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Exercise>> {
        return this.webClient.get<ApiResult<Exercise>>("used-exercises", this.getExercisesHttpParams(type, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    getExerciseWorkouts(exerciseId:number, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Workout>> {
        return this.webClient.get<ApiResult<Workout>>(`${exerciseId}/workouts`, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    private getExercisesHttpParams(type: ExerciseType|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) : HttpParams {
        var httpParams = this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
        
        if(type !== null){
            httpParams = httpParams.set('type', type)
        }

        return httpParams;
    }

    private toFormData(exerciseWithPhoto: UploadWithPhoto<Exercise>): FormData {
        let formData = new FormData();
    
        const { model: exercise, photo } = exerciseWithPhoto;

        const prefix = 'model.';

        if(exercise.id) {
            formData.append(`${prefix}id`, exercise.id.toString());
        }

        formData.append(`${prefix}name`, exercise.name);
        formData.append(`${prefix}type`, exercise.type.toString());
      
        if (exercise.description) {
            formData.append(`${prefix}description`, exercise.description);
        }

        if (exercise.image) {
            formData.append(`${prefix}image`, exercise.image);
        }

        if (photo) {
            formData.append('photo', photo, photo.name);
        }
    
        return formData;
    }
}
