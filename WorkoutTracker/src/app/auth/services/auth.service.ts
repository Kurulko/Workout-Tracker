import { BaseService } from "../../shared/services/base.service";
import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { RegisterModel } from "../models/register.model";
import { LoginModel } from "../models/login.model";
import { AuthModel } from '../models/auth.model';
import { AuthResult } from '../models/auth-result.model';
import { TokenModel } from "../../shared/models/tokens/token.model";
import { TokenViewModel } from "../../shared/models/tokens/token.view-model";
import { toTokenModel } from "../../shared/helpers/functions/toTokenModel";
import { map } from 'rxjs/operators';

@Injectable({
    providedIn: 'root'
})
export class AuthService extends BaseService {
    constructor(httpClient: HttpClient) {
        super(httpClient, 'account');
    }
    
    private account(path:string, authModel: AuthModel): Observable<AuthResult> {
        var observableAuthViewResult = this.webClient.post<AuthViewResult>(path, authModel);
        return observableAuthViewResult
            .pipe(map((authViewResult:AuthViewResult) => toAuthResult(authViewResult)));
    }

    login(loginModel: LoginModel): Observable<AuthResult> {
        return this.account('login', loginModel);
    }

    register(registerModel: RegisterModel): Observable<AuthResult> {
        return this.account('register', registerModel);
    }

    token(): Observable<TokenModel> {
        var observableTokenViewModel =  this.webClient.get<TokenViewModel>('token');
        return observableTokenViewModel
            .pipe(map((tokenViewModel:TokenViewModel) => toTokenModel(tokenViewModel)!))
    }

    logout(): Observable<Object> {
        return this.webClient.post('logout');
    }
}

interface AuthViewResult {
    success: boolean;
    message: string;
    token: TokenViewModel|null;
}

function toAuthResult(authViewResult: AuthViewResult) : AuthResult {
    const authResult = <AuthResult>{};

    authResult.success = authViewResult.success;
    authResult.message = authViewResult.message;
    authResult.token = toTokenModel(authViewResult.token);

    return authResult;
}