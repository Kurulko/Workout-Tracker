import { Component, Input, EventEmitter, Output, ViewChild, OnInit, OnChanges } from '@angular/core';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { MainComponent } from '../base/main.component';
import { PreferencesManager } from '../../helpers/managers/preferences-manager';

@Component({
  template: '',
})
export abstract class BaseTableComponent<T> extends MainComponent implements OnInit, OnChanges {
    @Input()
    data!: T[];

    @Input()
    isPaginator: boolean = true;
    
    @Input()
    pageIndex: number = 0;

    @Input()
    pageSize: number = 10;

    @Input()
    totalCount!: number;

    @Input()
    sortColumn: string = "id";

    @Input()
    sortOrder: "asc" | "desc" = "asc";

    @Input()
    pageSizeOptions: number[] = [10, 20, 50];

    @Input()
    displayedColumns!: string[];

    @Output() sortChange = new EventEmitter<{ sortColumn: string; sortOrder: string }>();
    @Output() pageChange = new EventEmitter<{ pageIndex: number; pageSize: number }>();
    @Output() deleteItem = new EventEmitter<any>();

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    isData: boolean = false;
    constructor(
        impersonationManager: ImpersonationManager, 
        tokenManager: TokenManager,
        preferencesManager: PreferencesManager,
        snackBar: MatSnackBar) 
    {
        super(impersonationManager, tokenManager, preferencesManager, snackBar);
    }

    ngOnInit() {
        this.onDataChanges();
    }

    ngOnChanges(){
        this.onDataChanges();
    }

    onDataChanges() {
        this.isData = this.data.length > 0;
    }

    onSortChange(): void {
        this.sortColumn = this.sort.active;
        this.sortOrder = this.sort.direction as "asc" | "desc";

        this.sortChange.emit({
            sortColumn: this.sortColumn,
            sortOrder: this.sortOrder,
        });
    }

    onPageChange(): void {
        this.pageIndex = this.paginator.pageIndex;
        this.pageSize = this.paginator.pageSize;

        this.pageChange.emit({
            pageIndex: this.pageIndex,
            pageSize: this.pageSize,
        });
    }

    onDelete(item: any): void {
        this.deleteItem.emit(item); 
    }
}