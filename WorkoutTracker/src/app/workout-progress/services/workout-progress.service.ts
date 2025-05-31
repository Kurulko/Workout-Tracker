import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";

import { DateTimeRange } from 'src/app/shared/models/date-time-range';
import { ModelsService } from 'src/app/shared/services/models.service';
import { CurrentUserProgress } from '../models/current-user-progress';
import { WorkoutProgress } from '../models/workout-progress';

@Injectable({
    providedIn: 'root'
})
export class WorkoutProgressService extends ModelsService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'workout-progress');
    }

    calculateCurrentUserProgress(): Observable<CurrentUserProgress> {
        return this.webClient.get<CurrentUserProgress>("current");
    }

    calculateTotalWorkoutProgress(): Observable<WorkoutProgress> {
        return this.webClient.get<WorkoutProgress>("total");
    }

    calculateWorkoutProgress(range: DateTimeRange|null): Observable<WorkoutProgress> {
        var params =  this.getRangeParams(new HttpParams(), range)
                    
        return this.webClient.get<WorkoutProgress>("total", params);
    }

    calculateWorkoutProgressForMonth(year: number, month: number): Observable<WorkoutProgress> {
        var params =  new HttpParams()
            .set("year", year.toString())
            .set("month", month.toString())
                    
        return this.webClient.get<WorkoutProgress>("total/by-month", params);
    }

    calculateWorkoutProgressForYear(year: number): Observable<WorkoutProgress> {
        var params =  new HttpParams()
            .set("year", year.toString());
                    
        return this.webClient.get<WorkoutProgress>("total/by-year", params);
    }
}
