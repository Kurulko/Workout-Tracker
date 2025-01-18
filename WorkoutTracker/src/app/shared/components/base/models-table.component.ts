import { Component, OnInit } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { MainComponent } from './main.component';
import { ApiResult } from '../../models/api-result';

@Component({
  template: ''
})
export abstract class ModelsTableComponent<T> extends MainComponent implements OnInit {
  data!: T[];
  isData: boolean =  false;

  pageIndex: number = 0;
  pageSize: number = 10;
  totalCount!: number;

  sortColumn: string = "id";
  sortOrder: "asc" | "desc" = "asc";
  filterColumn?: string;
  filterQuery?: string;
    
  displayedColumns!: string[];

  abstract ngOnInit(): void;

  abstract getModels(
    pageIndex:number, 
    pageSize:number, 
    sortColumn:string, 
    sortOrder:string, 
    filterColumn:string|null, 
    filterQuery:string|null
  ) : Observable<ApiResult<T>>;

  abstract deleteItem(id: any): void;

  loadData() {
    this.getModels(this.pageIndex, this.pageSize, this.sortColumn, this.sortOrder, this.filterColumn ?? null, this.filterQuery ?? null)
      .pipe(this.catchError())
      .subscribe((apiResult: ApiResult<T>) => {
        this.totalCount = apiResult.totalCount;
        this.pageIndex = apiResult.pageIndex;
        this.pageSize = apiResult.pageSize;

        this.data = apiResult.data;
        this.isData = apiResult.data.length !== 0
      });
  }
 
  onSortChange(event: { sortColumn: string; sortOrder: string }): void {
    this.sortColumn = event.sortColumn;
    this.sortOrder = event.sortOrder as 'asc' | 'desc';
    this.loadData();
  }

  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadData();
  }

  onDeleteItem(id: any): void {
    this.deleteItem(id);
  }

  filterTextChanged: Subject<string> = new Subject<string>();
  onFilterTextChanged(filterText: string) {
    // if (this.filterTextChanged.closed) {
    if(this.filterTextChanged.observers.length === 0) {
      this.filterTextChanged
        .pipe(debounceTime(1000), distinctUntilChanged())
        .subscribe(query => {
          this.filterQuery = query;
          this.loadData();
        });
    }
    this.filterTextChanged.next(filterText);
  }
}