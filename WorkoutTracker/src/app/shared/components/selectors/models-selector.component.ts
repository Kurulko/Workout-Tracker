import { Component, Input, OnInit } from '@angular/core';
import { BaseSelectorComponent } from './base-selector.component';

@Component({
  template: ''
})
export abstract class ModelsSelectorComponent<T> extends BaseSelectorComponent<T> implements OnInit {
  @Input()
  pageIndex:number = 0;

  @Input()
  pageSize:number = 9999;
  
  @Input() 
  sortColumn:string = "name";
  
  @Input() 
  sortOrder:string = "asc"; 
  
  @Input() 
  filterColumn:string|null = null;
  
  @Input()
  filterQuery:string|null = null;

  abstract loadData(): void;
  
  ngOnInit(): void {
    this.loadData();
  }

  compareWithById(item1: any, item2: any): boolean {
    return item1 && item2 ? item1.id === item2.id : item1 === item2;
  }

  protected validateItemId(selectedItemId: any) {
    if(!this.required)
      return null;

    return selectedItemId ? null : { required: true };
  }

  protected validateItems(selectedItems: any[]|undefined, isNoneOptionSelected: boolean) {
    if(!this.required)
      return null;

    return selectedItems && selectedItems.length > 0 && !isNoneOptionSelected ? null : { required: true };
  }
}