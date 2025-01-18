import { Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { Observable } from "rxjs";
import { TokenModel } from '../../models/token.model';
import { BaseManager } from './base-manager';

@Injectable({
  providedIn: 'root'
})
export class TokenManager extends BaseManager implements OnDestroy {
    private authenticationChanged = new Subject<boolean>();
    private adminChanged = new Subject<boolean>();

    private readonly tokenStorageName: string = 'token';
    readonly adminRoleStr: string = 'Admin';

    public isAuthenticated() : boolean {        
        return this.getActiveToken() ? true : false; 
    }

    public isAdmin() : boolean {
        if (this.isAuthenticated()) {
            const tokenModel = this.getActiveToken()!;
            return tokenModel.roles.indexOf(this.adminRoleStr) !== -1;
        }

        return false; 
    }

    private getActiveToken(): TokenModel|undefined{
        const tokenModel = this.getTokenFromStorage();

        if(tokenModel){
            const today = new Date();
            const expirationDate = new Date(tokenModel.expirationDate);
            const isActive =  expirationDate > today;

            if(!isActive){
                this.clearToken();
                return undefined;
            }

            return tokenModel;
        }

        return undefined;
    }
    
    public isAuthenticationChanged(): Observable<boolean> {
        return this.authenticationChanged.asObservable();
    }

    public isAdminChanged(): Observable<boolean> {
        return this.adminChanged.asObservable();
    }
    
    public getToken(): string|undefined {
        if(!this.isAuthenticated())
            return undefined;

        return this.getTokenFromStorage()?.tokenStr;
    }

    private getTokenFromStorage(): TokenModel|undefined {
        let localStorageToken = localStorage[this.tokenStorageName];
        
        if(this.hasStorageValue(localStorageToken))
            return JSON.parse(localStorageToken) as TokenModel;

        return undefined;
    }

    public setToken(tokenModel: TokenModel): void {
        localStorage[this.tokenStorageName] = JSON.stringify(tokenModel);
        this.authenticationChanged.next(this.isAuthenticated());
        this.adminChanged.next(this.isAdmin());
    }

    public failToken(): void {
        this.clearToken();
    }

    public logout(): void {
        this.clearToken();
    }

    private clearToken(): void {
        localStorage.removeItem(this.tokenStorageName);
        this.authenticationChanged.next(false);
        this.adminChanged.next(false);
    }

    ngOnDestroy(): void {
        this.authenticationChanged.complete();
        this.adminChanged.complete();
    }
}