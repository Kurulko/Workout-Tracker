import { ModelsService } from "../../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { ApiResult } from "../../shared/models/api-result";
import { ExerciseType } from "../models/exercise-type";
import { Exercise } from "../models/exercise";
import { ExerciseDetails } from "../models/exercise-details";
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

    getInternalExercises(type: ExerciseType|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterQuery:string|null): Observable<ApiResult<Exercise>> {
        return this.webClient.get<ApiResult<Exercise>>("internal-exercises", this.getExercisesHttpParams(type, pageIndex, pageSize, sortColumn, sortOrder, filterQuery));
    }

    updateInternalExercise(exercise: Exercise): Observable<Object> {
        return this.webClient.put(`internal-exercise/${exercise.id}`, exercise);
    }

    updateInternalExerciseMuscles(exerciseId: number, muscleIds: number[]): Observable<Object> {
        return this.webClient.put(`internal-exercise/${exerciseId}/muscles`, muscleIds);
    }

    updateInternalExerciseEquipments(exerciseId: number, equipmentIds: number[]): Observable<Object> {
        return this.webClient.put(`internal-exercise/${exerciseId}/equipments`, equipmentIds);
    }

    updateInternalExerciseAliases(exerciseId: number, aliases: string[]): Observable<Object> {
        return this.webClient.put(`internal-exercise/${exerciseId}/aliases`, aliases);
    }

    createInternalExercise(exercise: Exercise): Observable<Exercise>{
        return this.webClient.post<Exercise>("internal-exercise", exercise);
    }

    deleteInternalExercise(id: number): Observable<Object> {
        return this.webClient.delete(`internal-exercise/${id}`);
    }

    updateInternalExercisePhoto(id: number, photo: File | null): Observable<Object> {
        const formData = new FormData();

        if (photo) {
            formData.append('fileUpload', photo);
        }

        return this.webClient.put(`internal-exercise-photo/${id}`, formData);
    }

    deleteInternalExercisePhoto(id: number): Observable<Object>{
        return this.webClient.delete(`internal-exercise-photo/${id}`);
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

    getUserExercises(type: ExerciseType|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterQuery:string|null): Observable<ApiResult<Exercise>> {
        return this.webClient.get<ApiResult<Exercise>>("user-exercises", this.getExercisesHttpParams(type, pageIndex, pageSize, sortColumn, sortOrder, filterQuery));
    }

    updateUserExercise(exercise: Exercise): Observable<Object> {
        return this.webClient.put(`user-exercise/${exercise.id}`, exercise);
    }

    updateUserExerciseMuscles(exerciseId: number, muscleIds: number[]): Observable<Object> {
        return this.webClient.put(`user-exercise/${exerciseId}/muscles`, muscleIds);
    }

    updateUserExerciseEquipments(exerciseId: number, equipmentIds: number[]): Observable<Object> {
        return this.webClient.put(`user-exercise/${exerciseId}/equipments`, equipmentIds);
    }

    updateUserExerciseAliases(exerciseId: number, aliases: string[]): Observable<Object> {
        return this.webClient.put(`user-exercise/${exerciseId}/aliases`, aliases);
    }

    createUserExercise(exercise: Exercise): Observable<Exercise>{
        return this.webClient.post<Exercise>("user-exercise", exercise);
    }

    deleteUserExercise(id: number): Observable<Object> {
        return this.webClient.delete(`user-exercise/${id}`);
    }

    updateUserExercisePhoto(id: number, photo: File | null): Observable<Object> {
        const formData = new FormData();

        if (photo) {
            formData.append('fileUpload', photo);
        }

        return this.webClient.put(`user-exercise-photo/${id}`, formData);
    }

    deleteUserExercisePhoto(id: number): Observable<Object>{
        return this.webClient.delete(`user-exercise-photo/${id}`);
    }


    getExerciseById(id: number): Observable<Exercise> {
        return this.webClient.get<Exercise>(`exercise/${id}`);
    }

    getExerciseByName(name: string): Observable<Exercise> {
        return this.webClient.get<Exercise>(`exercise/by-name/${name}`);
    }

    getAllExercises(type: ExerciseType|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterQuery:string|null): Observable<ApiResult<Exercise>> {
        return this.webClient.get<ApiResult<Exercise>>("all-exercises", this.getExercisesHttpParams(type, pageIndex, pageSize, sortColumn, sortOrder, filterQuery));
    }

    getUsedExercises(type: ExerciseType|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterQuery:string|null): Observable<ApiResult<Exercise>> {
        return this.webClient.get<ApiResult<Exercise>>("used-exercises", this.getExercisesHttpParams(type, pageIndex, pageSize, sortColumn, sortOrder, filterQuery));
    }

    getExerciseWorkouts(exerciseId:number, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Workout>> {
        return this.webClient.get<ApiResult<Workout>>(`${exerciseId}/workouts`, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    private getExercisesHttpParams(type: ExerciseType|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterQuery:string|null) : HttpParams {
        var httpParams = this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, null, null);
        
        if(filterQuery !== null){
            httpParams = httpParams.set('filterQuery', filterQuery)
        }

        if(type !== null){
            httpParams = httpParams.set('type', type)
        }

        return httpParams;
    }
}
