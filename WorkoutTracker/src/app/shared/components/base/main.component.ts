import { Component, Input } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig  } from '@angular/material/snack-bar';
import { OperatorFunction } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { throwError } from 'rxjs';
import { StatusCodes } from 'http-status-codes';
import { ImpersonationManager } from '../../helpers/managers/impersonation-manager';
import { TokenManager } from '../../helpers/managers/token-manager';
import { BaseComponent } from 'src/app/base.component';
import { getErrors } from '../../helpers/functions/getFunctions/getErrors';
import { PreferencesManager } from '../../helpers/managers/preferences-manager';
import { showBigNumberStr } from '../../helpers/functions/showFunctions/showBigNumberStr';

@Component({
    selector: 'main-app',
    templateUrl: './main.component.html',
})
export class MainComponent extends BaseComponent {
  @Input() title?: string;

  validationErrors: { [key: string]: string[] } = {};

  constructor( 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager, 
    preferencesManager: PreferencesManager,
    private snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager)
  }

  getErrors = getErrors;
  showBigNumberStr = showBigNumberStr;

  protected errorOccured(error:string) {
    this.showSnackbar(`ERROR: ${error}`)
  }

  protected operationDoneSuccessfully(modelName:string, actionName:string) {
    this.showSnackbar(`${modelName} ${actionName} successfully`)
  }

  protected modelDeletedSuccessfully(modelName:string){
    this.operationDoneSuccessfully(modelName, 'deleted');
  }

  protected modelUpdatedSuccessfully(modelName:string){
    this.operationDoneSuccessfully(modelName, 'updated');
  }

  protected modelAddedSuccessfully(modelName:string){
    this.operationDoneSuccessfully(modelName, 'added');
  }
  
  protected showSnackbar(message: string): void {
    const config = new MatSnackBarConfig();
    config.duration = 1500;
    config.verticalPosition = 'top';

    this.snackBar.open(message, 'Close', config);
  }

  protected catchError<T>() : OperatorFunction<T, T> {
    return catchError((errorResponse: HttpErrorResponse) => {
      console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);

      if(errorResponse.status === StatusCodes.INTERNAL_SERVER_ERROR)
        this.showSnackbar('Internal error occurred');
      else if(typeof errorResponse.error.errors === 'object') 
        this.validationErrors = errorResponse.error.errors;
      else {

        let errorMessage = 'An unknown error occurred';
        if (errorResponse.error instanceof ErrorEvent) {
          // Client-side or network error
          errorMessage = `Client-side error: ${errorResponse.error.message}`;
        } else {
          // Server-side error
          errorMessage = `Server returned code: ${errorResponse.status}, message: ${errorResponse.message}`;
          if (errorResponse.error) {
            if (typeof errorResponse.error === 'string') {
              // If the server returned a string error
              errorMessage = errorResponse.error;
            } else if (typeof errorResponse.error === 'object') {
              // If the server returned an object (e.g., JSON)
              errorMessage = errorResponse.error.message || JSON.stringify(errorResponse.error);
            }
          }
        }
        this.showSnackbar(errorMessage);
      }
      return throwError(() => errorResponse);
    });
  }
}