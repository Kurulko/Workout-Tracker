import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { BaseService } from '../shared/services/base.service';
import { UserProgress } from './user-progress';

@Injectable({
    providedIn: 'root'
})
export class UserProgressService extends BaseService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'user-progress');
    }

    calculateUserProgress(): Observable<UserProgress> {
        return this.webClient.get<UserProgress>(this.emptyPath);
    }
}
