import { MatSnackBar } from '@angular/material/snack-bar';
import { BaseComponent } from './base.component';
import { OnInit, Component } from '@angular/core';

@Component({
    template: '',
})
export abstract class EditModelComponent<T> extends BaseComponent implements OnInit {
    id?: number;

    constructor(snackBar: MatSnackBar){
        super(snackBar);
    }

    abstract onSubmit() : void;
    abstract loadData() : void;

    ngOnInit() {
        this.loadData();
    }

    protected modelUpdatedSuccessfully(modelName:string){
        this.operationDoneSuccessfully('updated', modelName);
    }

    protected modelAddedSuccessfully(modelName:string){
        this.operationDoneSuccessfully('added', modelName);
    }
}
