import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-photo-upload-dialog',
  templateUrl: './photo-upload-dialog.component.html',
  styleUrls: ['./photo-upload-dialog.component.css']
})
export class PhotoUploadDialogComponent {
  modelName: string;
  label: string;
  required: boolean;
  width?: string;

  selectedFile: File | null = null;
  previewUrl: string | null = null;

  constructor(public dialogRef: MatDialogRef<PhotoUploadDialogComponent>,
    @Inject(MAT_DIALOG_DATA) data: { 
      modelName?: string; 
      label?: string ; 
      required?: boolean;
      width?: string
    }
  ) {
      this.modelName = data.modelName ?? "Photo";
      this.label = data.label ?? "Upload Photo";
      this.required = data.required ?? false;
      this.width = data.width;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];

      const reader = new FileReader();
      reader.onload = (e) => {
        this.previewUrl = e.target?.result as string;
      };
      reader.readAsDataURL(this.selectedFile);
    }
  }

  onSave(): void {
    this.dialogRef.close(this.selectedFile);
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }
}
