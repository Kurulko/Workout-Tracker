import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export class WebClient {
    constructor(private httpClient: HttpClient, private pathBase: string) {}

    get(url: string, params?: HttpParams): Observable<Object>;
    get<T>(url: string, params?: HttpParams): Observable<T>;
    
    get<T>(url:string, params?: HttpParams): Observable<T> {
        return this.httpClient.get<T>(`${this.pathBase}/${url}`, { params });
    }
    
    getText(url:string, params?: HttpParams): Observable<string> {
        return this.httpClient.get(`${this.pathBase}/${url}`, {responseType: 'text', params});
    }

    delete(url:string, params?: HttpParams): Observable<Object> {
        return this.httpClient.delete(`${this.pathBase}/${url}`, { params });
    }

    post(url: string, body?: any, params?: HttpParams): Observable<Object>;
    post<T>(url: string, body?: any, params?: HttpParams): Observable<T>;

    post<T>(url: string, body?: any, params?: HttpParams): Observable<T> {
        return this.httpClient.post<T>(`${this.pathBase}/${url}`, body, { params });
    }

    patch(url: string, body?: any, params?: HttpParams): Observable<Object>;
    patch<T>(url: string, body?: any, params?: HttpParams): Observable<T>;

    patch<T>(url: string, body?: any, params?: HttpParams): Observable<T> {
        return this.httpClient.patch<T>(`${this.pathBase}/${url}`, body, { params });
    }

    put(url: string, body?: any, params?: HttpParams): Observable<Object>;
    put<T>(url: string, body?: any, params?: HttpParams): Observable<T>;

    put<T>(url: string, body?: any, params?: HttpParams): Observable<T> {
        return this.httpClient.put<T>(`${this.pathBase}/${url}`, body, { params });
    }
}