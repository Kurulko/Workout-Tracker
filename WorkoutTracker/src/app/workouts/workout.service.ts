import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ModelsService } from 'src/app/shared/services/models.service';
import { ApiResult } from 'src/app/shared/models/api-result';
import { Workout } from './workout';
import { ExerciseSetGroup } from '../shared/models/exercise-set-group';
import { TimeSpan } from '../shared/models/time-span';
import { WorkoutDetails } from './workout-details';

@Injectable({
    providedIn: 'root'
})
export class WorkoutService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'workouts');
    }

    getWorkoutById(id: number): Observable<Workout> {
        return this.webClient.get<Workout>(`${id}`);
    }

    getWorkoutByName(name: string): Observable<Workout> {
        return this.webClient.get<Workout>(`by-name/${name}`);
    }

    getWorkoutDetailsById(id: number): Observable<WorkoutDetails> {
        return this.webClient.get<WorkoutDetails>(`${id}/details`);
    }

    getWorkoutDetailsByName(name: string): Observable<WorkoutDetails> {
        return this.webClient.get<WorkoutDetails>(`by-name/${name}/details`);
    }

    getWorkouts(exerciseId: number|null, pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Workout>> {
        var httpParams = this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
        
        if(exerciseId){
            httpParams = httpParams.set('exerciseId', exerciseId)
        }

        return this.webClient.get<ApiResult<Workout>>(this.emptyPath, httpParams);
    }

    updateWorkout(workout:Workout): Observable<Object> {
        return this.webClient.put(`${workout.id}`, workout);
    }

    createWorkout(workout:Workout): Observable<Workout>{
        return this.webClient.post<Workout>(this.emptyPath, workout);
    }

    addExerciseSetGroupsToWorkout(workoutId: number, exerciseSetGroups: ExerciseSetGroup[]): Observable<Object>{
        return this.webClient.post(`exercise-set-groups/${workoutId}`, exerciseSetGroups);
    }

    updateWorkoutExerciseSetGroups(workoutId: number, exerciseSetGroups: ExerciseSetGroup[]): Observable<Object>{
        return this.webClient.put(`exercise-set-groups/${workoutId}`, exerciseSetGroups);
    }

    completeWorkout(workoutId: number, date: Date, time: TimeSpan): Observable<Object>{
        return this.webClient.put(`complete/${workoutId}`, { date, time });
    }

    pinWorkout(workoutId: number): Observable<Object>{
        return this.webClient.put(`pin/${workoutId}`);
    }

    unpinWorkout(workoutId: number): Observable<Object>{
        return this.webClient.put(`unpin/${workoutId}`);
    }

    deleteWorkout(id: number): Observable<Object> {
        return this.webClient.delete(`${id}`);
    }

    workoutExists(id: number): Observable<boolean> {
        return this.webClient.get<boolean>(`workout-exists/${id}`);
    }

    workoutExistsByName(name: string): Observable<boolean> {
        return this.webClient.get<boolean>(`workout-exists-by-name/${name}`);
    }
}
