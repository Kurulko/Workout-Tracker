import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ModelsService } from 'src/app/shared/services/models.service';
import { UserDetails } from '../models/user-details';

@Injectable({
    providedIn: 'root'
})
export class UserDetailsService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'users/user-details');
    }

    getCurrentUserDetails(): Observable<UserDetails>{
        return this.webClient.get<UserDetails>(this.emptyPath);
    }

    addUserDetails(userDetails: UserDetails): Observable<Object>{
        return this.webClient.post(this.emptyPath, userDetails);
    }

    updateUserDetails(userDetails: UserDetails): Observable<Object> {
        return this.webClient.put(this.emptyPath, userDetails);
    }
}
