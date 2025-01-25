import { BaseService } from "../../shared/services/base.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { map, Observable } from "rxjs";
import { TokenModel } from "../models/token.model";
import { TokenViewModel } from "../models/token.view-model";
import { toTokenModel } from "../helpers/functions/toTokenModel";

@Injectable({
    providedIn: 'root'
})
export class ImpersonationService extends BaseService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'impersonation');
    }

    impersonate(userId: string) : Observable<TokenModel> {
        return this.webClient.post<TokenViewModel>(`impersonate/${userId}`)
            .pipe(map((tokenViewModel:TokenViewModel) => toTokenModel(tokenViewModel)!))
    }
   
    revert() : Observable<TokenModel>  {
        return this.webClient.post<TokenViewModel>('revert')
            .pipe(map((tokenViewModel:TokenViewModel) => toTokenModel(tokenViewModel)!))
    }

    isImpersonating(): Observable<boolean> {
        return this.webClient.post<boolean>('is-impersonating');
    }
}
