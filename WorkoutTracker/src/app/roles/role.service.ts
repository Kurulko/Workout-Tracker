import { ModelsService } from "../shared/services/models.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Role } from "./role";
import { ApiResult } from "../shared/models/api-result";

@Injectable({
    providedIn: 'root'
})
export class RoleService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'roles');
    }
    
    getRoleById(id: string): Observable<Role> {
        return this.webClient.get<Role>(id);
    }

    getRoleNameById(id: string): Observable<string> {
        return this.webClient.get<string>(`name-by-id/${id}`);
    }

    getRoleByName(name: string): Observable<Role> {
        return this.webClient.get<Role>(`by-name/${name}`);
    }

    getRoleIdByName(name: string): Observable<string> {
        return this.webClient.get<string>(`id-by-name/${name}`);
    }

    getRoles(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null): Observable<ApiResult<Role>> {
        return this.webClient.get<ApiResult<Role>>(this.emptyPath, this.getApiResultHttpParams(pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery));
    }

    updateRole(role:Role): Observable<Object> {
        return this.webClient.put(`/${role.id}`, role);
    }

    createRole(role:Role): Observable<Role>{
        return this.webClient.post<Role>(this.emptyPath, role);
    }

    deleteRole(id: string): Observable<Object> {
        return this.webClient.delete(id);
    }

    roleExists(id: string): Observable<boolean> {
        return this.webClient.get<boolean>(`role-exists/${id}`);
    }

    roleExistsByName(name: string): Observable<boolean> {
        return this.webClient.get<boolean>(`role-exists-by-name/${name}`);
    }
}
