import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { TokenManager } from '../../shared/helpers/token-manager';
import { AuthResult } from '../models/auth-result.model';
import { BaseComponent } from '../../shared/components/base.component';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { StatusCodes } from 'http-status-codes';

export abstract class AuthComponent extends BaseComponent {
    authResult?: AuthResult;
    returnUrl: string | null = null;

    constructor(protected tokenManager: TokenManager, protected router: Router, route: ActivatedRoute, protected authService: AuthService, snackBar: MatSnackBar){
        super(snackBar);

        route.queryParams.subscribe(params => {
          this.returnUrl = params['returnUrl'] || null;
        });
    }

    abstract getAuthResult() : Observable<AuthResult>;

    protected account(): void{
        this.getAuthResult()
        .pipe(catchError((errorResponse: HttpErrorResponse) => {
              if (errorResponse.status === StatusCodes.BAD_REQUEST) {
                const errorModel: AuthResult = errorResponse.error;
                this.authResult = errorModel; 
                console.error('Bad Request Error:', errorModel.message);
                this.showSnackbar(errorModel.message)
              } else {
                console.error('An error occurred:', errorResponse.message);
              }
              
              return throwError(() => errorResponse);
            }))
        .subscribe((authResult: AuthResult) => {
            this.authResult = authResult;

            this.showSnackbar(authResult.message)
            this.tokenManager.setToken(authResult.token!);

            const redirectUrl = this.returnUrl || '/home';
            this.router.navigateByUrl(redirectUrl);
        })
    }
}