import { MatSnackBar } from '@angular/material/snack-bar';
import { MainComponent } from './main.component';
import { Component } from '@angular/core';
import { ImpersonationManager } from '../../helpers/managers/impersonation-manager';
import { TokenManager } from '../../helpers/managers/token-manager';
import { PreferencesManager } from '../../helpers/managers/preferences-manager';
import { catchError, OperatorFunction, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { StatusCodes } from 'http-status-codes';

@Component({
    template: '',
})
export abstract class EditModelComponent<T> extends MainComponent {
    id?: number|string;

    constructor(
        protected router: Router,  
        impersonationManager: ImpersonationManager, 
        tokenManager: TokenManager,
        preferencesManager: PreferencesManager,
        snackBar: MatSnackBar) 
    {
        super(impersonationManager, tokenManager, preferencesManager, snackBar);
    }

    abstract onSubmit() : void;
    abstract loadData() : void;

    protected catchLoadDataError<T>(pathToBack: string) : OperatorFunction<T, T> {
        return catchError((errorResponse: HttpErrorResponse) => {
           console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);
          
            if (errorResponse.status === StatusCodes.NOT_FOUND) {
                this.router.navigate([pathToBack]);
            }
    
            this.showSnackbar(errorResponse.message);
            return throwError(() => errorResponse);
        });
      }
}
