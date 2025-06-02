import { Component, EventEmitter, Input, Output, forwardRef, ViewChild } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';

import { BaseEditorComponent } from '../../base-editor.component';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-multi-string-input',
  templateUrl: './multi-string-input.component.html',
  styleUrls: ['./multi-string-input.component.css'],
  providers: [
    {
        provide: NG_VALIDATORS,
        useExisting: forwardRef(() => ExerciseAliasesEditorComponent),
        multi: true,
    },
    {
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => ExerciseAliasesEditorComponent),
        multi: true,
    },
  ],
})
export class ExerciseAliasesEditorComponent extends BaseEditorComponent<string[]> {
  @Output() validityChange = new EventEmitter<boolean>();

  @ViewChild(MatPaginator, { static: false }) paginator!: MatPaginator;
  @ViewChild(MatSort, { static: false }) sort!: MatSort;
    
  public exerciseAliases!: string[];
  displayedColumns: string[] = ['index', 'name', 'actions'];
  sortColumn: string = 'name';

  dataSource = new MatTableDataSource<{ name: string }>();

  refreshTable(): void {
    const items = this.exerciseAliases.map(name => ({ name }));
    this.dataSource.data = items;

    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }

    if (this.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  private updateValue() {
    this.onChange(this.exerciseAliases); 
    this.onTouched();
  }

  validate() {
    const isValid = !this.required || this.exerciseAliases.length > 0;
    this.validityChange.emit(isValid);
    return isValid ? null : { required: true };
  }

  writeValue(value: string[]): void {
    this.exerciseAliases = value ?? [];
    this.refreshTable();
  }

  editingExerciseAliasName: string | null = null;
  editingExerciseAliasInput: string | null = null;
  isEditingExerciseAlias(name: string): boolean {
    return this.editingExerciseAliasName === name;
  }

  startEditingExerciseAlias(name: string): void {
    this.editingExerciseAliasName = name;
    this.editingExerciseAliasInput = name;
  }

  cancelEditingExerciseAlias(): void {
    this.editingExerciseAliasName = null;
  }

  isEditingNameValid: boolean = true;
  onNameChange(nameEditing: any): void {
    this.isEditingNameValid = nameEditing.valid; 
  }

  saveExerciseAlias(): void {
    var index = this.exerciseAliases.indexOf(this.editingExerciseAliasName!);
    if (index !== -1) {
      this.exerciseAliases[index] = this.editingExerciseAliasInput!;
      this.cancelEditingExerciseAlias();
      this.updateValue();
      this.refreshTable();
    }
  }
  
  isAddingExerciseAlias: boolean = false;
  addingExerciseAliasName: string | null = null;

  startAddingExerciseAlias(): void {
    this.isAddingExerciseAlias = true;
  }

  cancelAddingExerciseAlias(): void {
    this.isAddingExerciseAlias = false;
    this.addingExerciseAliasName = null;
  }

  addExerciseAlias(): void {
    this.exerciseAliases.push(this.addingExerciseAliasName!);
    this.cancelAddingExerciseAlias();

    this.updateValue();
    this.refreshTable();
  }

  deleteItem = async (name: string): Promise<void> => {
    var index = this.exerciseAliases.indexOf(name);
    if (index !== -1) {
      this.exerciseAliases.splice(index, 1);
      this.updateValue();
      this.refreshTable();
    }
  };
}