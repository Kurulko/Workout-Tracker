import { ModelsService } from "../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { User } from "./user";
import { ApiResult } from "../shared/models/api-result";
import { PasswordModel } from "../account/models/password-model";
import { Role } from "../roles/role";
import { MuscleSize } from "../muscle-sizes/muscle-size";
import { BodyWeight } from "../body-weights/body-weight";
import { Equipment } from "../equipments/equipment";
import { Workout } from "../workouts/workout";
import { Exercise } from "../exercises/models/exercise";
import { ExerciseRecord } from "../exercise-records/exercise-record";

@Injectable({
    providedIn: 'root'
})
export class UserService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'users');
    }
    
    getUserById(id: string): Observable<User> {
        return this.webClient.get<User>(id);
    }

    getCurrentUser(): Observable<User> {
        return this.webClient.get<User>('current-user');
    }

    getUserNameById(id: string): Observable<string> {
        return this.webClient.get<string>(`name-by-id/${id}`);
    }

    getUserByName(name: string): Observable<User> {
        return this.webClient.get<User>(`username/${name}`);
    }

    getUserIdByName(name: string): Observable<string> {
        return this.webClient.get<string>(`id-by-name/${name}`);
    }

    getUsers(pageIndex: number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<User>> {
        return this.webClient.get<ApiResult<User>>(this.emptyPath, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateUser(user: User): Observable<Object> {
        return this.webClient.put(user.userId, user);
    }

    addUser(user: User): Observable<User>{
        return this.webClient.post<User>(this.emptyPath, user);
    }

    createUser(user: User, password: string): Observable<User>{
        return this.webClient.post<User>('create', {user, password});
    }

    deleteUser(id: string): Observable<Object> {
        return this.webClient.delete(id);
    }

    userExists(id: string): Observable<boolean> {
        return this.webClient.get<boolean>(`user-exists/${id}`);
    }

    userExistsByName(name: string): Observable<boolean> {
        return this.webClient.get<boolean>(`user-exists-by-username/${name}`);
    }


    private getUserModels<T>(path:string, pageIndex: number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) : Observable<ApiResult<T>>{
        return  this.webClient.get<ApiResult<T>>(path, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }
    
    getUserMuscleSizes(pageIndex: number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) 
        : Observable<ApiResult<MuscleSize>>
    {
        return this.getUserModels<MuscleSize>(`user-muscle_sizes`, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
    }

    getUserBodyWeights(pageIndex: number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) 
        : Observable<ApiResult<BodyWeight>>
    {
        return this.getUserModels<BodyWeight>(`user-body_weights`, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
    }

    getUserWorkouts(pageIndex: number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) 
        : Observable<ApiResult<Workout>>
    {
        return this.getUserModels<Workout>(`user-workouts`, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
    }

    getUserExercises(pageIndex: number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) 
        : Observable<ApiResult<Exercise>>
    {
        return this.getUserModels<Exercise>(`user-exercises`, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
    }

    getUserExerciseRecords(pageIndex: number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) 
        : Observable<ApiResult<ExerciseRecord>>
    {
        return this.getUserModels<ExerciseRecord>(`user-exercise_records`, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
    }

    getUserEquipments(pageIndex: number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) 
        : Observable<ApiResult<Equipment>>
    {
        return this.getUserModels<Equipment>(`user-equipments`, pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
    }



    hasPassword(userId:string) : Observable<boolean>{
        return this.webClient.get<boolean>(`${userId}/password`);
    }

    changePassword(changePassword: PasswordModel) : Observable<Object>{
        return this.webClient.put(`password`, changePassword);
    }

    createPassword(userId: string, newPassword: string) : Observable<Object>{
        return this.webClient.post(`password`, {userId, newPassword});
    }



    getCurrentRoles() : Observable<Role[]>{
        return this.webClient.get<Role[]>(`current-user-roles`);
    }

    getUserRoles(userId: string) : Observable<Role[]>{
        return this.webClient.get<Role[]>(`user-roles/${userId}`);
    }

    addRolesToUser(userId:string, roles:string[]) : Observable<Object>{
        return this.webClient.post(`${userId}/role`, roles);
    }

    deleteRoleFromUser(userId:string, role:string) : Observable<Object>{
        return this.webClient.delete(`${userId}/${role}`);
    }

    getUsersByRole(role:string, pageIndex: number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) : Observable<ApiResult<User>>{
        return this.webClient.get<ApiResult<User>>(`users-by-role/${role}`, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }
}
