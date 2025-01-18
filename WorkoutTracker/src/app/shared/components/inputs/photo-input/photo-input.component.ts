import { Component, Input, forwardRef } from '@angular/core';
import { BaseEditorComponent } from '../../base-editor.component';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { PhotoUploadDialogComponent } from '../../dialogs/photo-upload-dialog/photo-upload-dialog.component';

@Component({
  selector: 'app-photo-input',
  templateUrl: './photo-input.component.html',
  styleUrls: ['./photo-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PhotoInputComponent),
      multi: true,
    },
  ],
})
export class PhotoInputComponent extends BaseEditorComponent<File> {
  @Input()
  previewUrl: string | null = null;

  private _imageFile: File|null = null;

  constructor(private dialog: MatDialog){
    super();
  }
  
  ngOnInit(): void {
    this._imageFile = this.value ?? null;
    this.modelName = this.modelName ?? "Photo";
  }

  get imageFile(): File|null {
    return this._imageFile;
  }

  set imageFile(value: File|null) {
    this._imageFile = value;
    this.onChange(value ?? undefined); 
    this.onTouched();
  }

  writeValue(value?: File): void {
    this._imageFile = value ?? null;
  }

  onFileSelected(file: File | null): void {
    if (file) {
      this.imageFile = file;

      const reader = new FileReader();
      reader.onload = (e) => {
        this.previewUrl = e.target?.result as string;
      };
      reader.readAsDataURL(this.imageFile);
    }
  }

  openUploadPhotoDialog(){    
    const dialogRef = this.dialog.open(PhotoUploadDialogComponent, {
      data: {
        modelName: this.modelName,
        label: this.label,
        required: this.required,
        width: "100%"
      }
    });

    dialogRef.afterClosed().subscribe((file: File | null) => {
      this.onFileSelected(file);
    });
  }

  deletePhoto() {
    this.imageFile = null;
    this.previewUrl = null;
  }
}

