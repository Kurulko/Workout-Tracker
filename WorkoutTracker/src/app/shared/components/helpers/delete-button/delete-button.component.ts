import { Component, Input, TemplateRef, ViewChild  } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'delete-button',
  templateUrl: './delete-button.component.html',
})
export class DeleteButtonComponent {
  @Input() title: string = "Delete";
  @Input() dialogTitle: string = "Delete";
  @Input() dialogContent: string = "This will delete this element that cannot be undone.";
  @Input() deleteMatIcon: string = "delete";

  @Input() deleteFn!: (id: any) => Promise<void>; 
  @Input() itemId!: any;

  @ViewChild('deleteButtonDialogTemplate') deleteButtonDialogTemplate!: TemplateRef<boolean>;

  constructor(private dialog: MatDialog) {

  }

  isDeleting = false;

  async onDelete(): Promise<void> {
    if (!this.deleteFn || this.itemId === undefined) 
      return;

    this.isDeleting = true;

    try {
      await this.deleteFn(this.itemId);
    } catch (error) {
      console.error('Error deleting item:', error);
    } finally {
      this.isDeleting = false;
    }
  }

  openDeleteButtonDialog(){    
    this.dialog.open(this.deleteButtonDialogTemplate, { width: '300px' });
  }
}