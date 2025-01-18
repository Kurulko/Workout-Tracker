import { Injectable, OnDestroy } from '@angular/core';
import { Observable, Subject } from "rxjs";
import { BaseManager } from './base-manager';

@Injectable({
    providedIn: 'root'
})
export class ImpersonationManager extends BaseManager implements OnDestroy {
    private impersonationKey = 'is-impersonating';
    private impersonationChanged = new Subject<boolean>();
    
    startImpersonating() {
        this.setImpersonation(true);
        this.impersonationChanged.next(true);
    }
   
    finishImpersonating() {
        this.setImpersonation(false);
        this.impersonationChanged.next(false);
    }

    isImpersonating(): boolean {
        return this.getImpersonationFromStorage() ? true : false; 
    }
    
    isImpersonationChanged(): Observable<boolean> {
        return this.impersonationChanged.asObservable();
    }

    private setImpersonation(value: boolean) {
        localStorage.setItem(this.impersonationKey, JSON.stringify(value));
    }

    private getImpersonationFromStorage(): boolean|undefined {
        let localStorageImpersonation = localStorage[this.impersonationKey];

        if(this.hasStorageValue(localStorageImpersonation))
            return JSON.parse(localStorageImpersonation) as boolean;

        return undefined;
    }

    ngOnDestroy(): void {
        this.impersonationChanged.complete();
    }
}
