import { Component, OnInit, ViewChild, Input, ContentChild, TemplateRef } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, Subject, throwError } from 'rxjs';
import { StatusCodes } from 'http-status-codes';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ApiResult } from '../models/api-result.model';
import { BaseComponent } from './base.component';
import { MatSnackBar  } from '@angular/material/snack-bar';

@Component({
  template: ''
})
export abstract class ModelsTableComponent<T> extends BaseComponent {
    public data!: MatTableDataSource<T>;
  
    defaultPageIndex: number = 0;
    defaultPageSize: number = 10;
    public defaultSortColumn: string = "id";
    public defaultSortOrder: "asc" | "desc" = "asc";
    defaultFilterColumn: string = "id";
    filterQuery?: string;
      
    displayedColumns!: string[];
   
    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    filterTextChanged: Subject<string> = new Subject<string>();
  
    constructor(snackBar: MatSnackBar) {
      super(snackBar);
    }

    abstract getModels(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) : Observable<ApiResult<T>>;
  
    onFilterTextChanged(filterText: string) {
      if (this.filterTextChanged.closed) {
        this.filterTextChanged
          .pipe(debounceTime(1000), distinctUntilChanged())
          .subscribe(query => {
            this.loadData(query);
          });
      }
      this.filterTextChanged.next(filterText);
    }
  
    loadData(query?: string) {
      this.filterQuery = query;

      var pageEvent = new PageEvent();
      pageEvent.pageIndex = this.defaultPageIndex;
      pageEvent.pageSize = this.defaultPageSize;
      
      this.getData(pageEvent);
    }
  
    getData(event: PageEvent) {
      var sortColumn = (this.sort) ? this.sort.active : this.defaultSortColumn;
      var sortOrder = (this.sort) ? this.sort.direction : this.defaultSortOrder;
      var filterColumn = (this.filterQuery) ? this.defaultFilterColumn : null;
      var filterQuery = (this.filterQuery) ? this.filterQuery : null;
  
      this.getModels(event.pageIndex, event.pageSize, sortColumn, sortOrder, filterColumn, filterQuery)
        .pipe(catchError((errorResponse: HttpErrorResponse) => {
          console.error(`Error occurred: ${errorResponse.message} - ${errorResponse.status}`);
          return throwError(() => errorResponse);
        }))
        .subscribe((apiResult: ApiResult<T>) => {
          this.paginator.length = apiResult.totalCount;
          this.paginator.pageIndex = apiResult.pageIndex;
          this.paginator.pageSize = apiResult.pageSize;
          this.data = new MatTableDataSource<T>(apiResult.data);
        });
    }

    protected modelDeletedSuccessfully(modelName:string){
      this.operationDoneSuccessfully('deleted', modelName);
  }
}