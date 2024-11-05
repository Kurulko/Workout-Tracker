import { Component, Input } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig  } from '@angular/material/snack-bar';
import { FormGroup, AbstractControl } from '@angular/forms';
import { OperatorFunction } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { throwError } from 'rxjs';
import { INTERNAL_SERVER_ERROR, StatusCodes } from 'http-status-codes';

@Component({
    selector: 'base-app',
    templateUrl: './base.component.html',
})
export class BaseComponent {
    @Input() title?: string;

    validationErrors: { [key: string]: string[] } = {};
    form!: FormGroup;

    constructor(private snackBar: MatSnackBar) {
    }

    getErrors(control: AbstractControl, displayName: string, customMessages: { [k: string]: string } = {}): string[] {
      var errors: string[] = [];
      Object.keys(control.errors || {}).forEach((key) => {
        switch (key) {
          case 'required':
            errors.push(`${displayName} ${customMessages?.[key] ?? "is required."}`);
            break;
          case 'minlength':
            const minlengthError = control.getError('minlength');
            errors.push(`${displayName} ${customMessages?.[key] ?? `must be at least ${minlengthError?.requiredLength} characters long.`}`);
            break;
          case 'pattern':
            errors.push(`${displayName} ${customMessages?.[key] ?? "contains invalid characters."}`);
            break;
          default:
            errors.push(`${displayName} ${customMessages?.[key] ?? "is invalid."}`);
            break;
        }
      });
      return errors;
    }

    protected errorOccured(error:string) {
        this.showSnackbar(`ERROR: ${error}`)
    }

    protected  operationDoneSuccessfully(actionName:string, modelName:string) {
      this.showSnackbar(`${modelName} ${actionName} successfully`)
    }

    protected showSnackbar(message: string): void {
        const config = new MatSnackBarConfig();
        config.duration = 2500;
        config.verticalPosition = 'top';

        this.snackBar.open(message, 'Close', config);
    }

    protected catchError<T>() : OperatorFunction<T, T>{
      return catchError((errorResponse: HttpErrorResponse) => {
        console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);

        if(errorResponse.status === StatusCodes.INTERNAL_SERVER_ERROR)
          this.showSnackbar('Error occurred');
        else if(typeof errorResponse.error.errors === 'object') 
          this.validationErrors = errorResponse.error.errors;
        else
          this.showSnackbar(errorResponse.message);

        return throwError(() => errorResponse);
      });
    }
}