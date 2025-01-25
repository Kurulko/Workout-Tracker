import { Component, Input, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { PhotoUploadDialogComponent } from '../../dialogs/photo-upload-dialog/photo-upload-dialog.component';
import { BaseInputComponent } from '../base-input.component';

@Component({
  selector: 'app-photo-input',
  templateUrl: './photo-input.component.html',
  styleUrls: ['./photo-input.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => PhotoInputComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PhotoInputComponent),
      multi: true,
    },
  ],
})
export class PhotoInputComponent extends BaseInputComponent<File> {
  @Input()
  previewUrl: string | null = null;

  constructor(private dialog: MatDialog){
    super();
  }
  
  ngOnInit() {
    const validators = [];

    if (this.required) {
      validators.push(Validators.required);
    }

    this.internalControl.setValidators(validators);

    if(!this.modelName){
      this.modelName = "Photo";
    }
  }

  imageFile: File|null = null;
  onFileSelected(file: File | null): void {
    if (file) {
      this.internalControl.setValue(file);
      this.imageFile = file;
      const reader = new FileReader();
      reader.onload = (e) => {
        this.previewUrl = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  override writeValue(value: any): void {
    
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
    this.internalControl.setValue(null);
    this.imageFile = null;
    this.previewUrl = null;
  }
}

