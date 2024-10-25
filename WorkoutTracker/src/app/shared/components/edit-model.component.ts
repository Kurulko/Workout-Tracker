import { MatSnackBar } from '@angular/material/snack-bar';
import { BaseComponent } from './base.component';
import { Component } from '@angular/core';

@Component({
    template: '',
})
export abstract class EditModelComponent<T> extends BaseComponent{
    errorMessage?: string;

    constructor(snackBar: MatSnackBar){
        super(snackBar);
    }

    abstract onSubmit() : void;

    protected modelUpdatedSuccessfully(modelName:string){
        this.operationDoneSuccessfully('updated', modelName);
    }

    protected modelAddedSuccessfully(modelName:string){
        this.operationDoneSuccessfully('added', modelName);
    }
}
