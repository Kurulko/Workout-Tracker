import { Injectable } from '@angular/core';
import { Observable, throwError, catchError } from 'rxjs'
import { TokenManager } from '../helpers/token-manager';
import { WebClient } from '../helpers/web-client';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

export abstract class BaseService {
    private readonly _pathBase:string = "api";
    protected readonly webClient: WebClient;

    constructor(httpClient: HttpClient, private readonly tokenManager: TokenManager, controllerName: string) {
        this.webClient = new WebClient(httpClient, `${this._pathBase}/${controllerName}`);
    }
}