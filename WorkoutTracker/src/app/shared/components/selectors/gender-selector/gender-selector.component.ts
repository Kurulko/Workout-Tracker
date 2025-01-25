import { Component, Input, EventEmitter, Output, OnInit, forwardRef } from '@angular/core';
import { BaseSelectorComponent } from '../base-selector.component';
import { getEnumElements } from 'src/app/shared/helpers/functions/getFunctions/getEnumElements';
import { Gender } from 'src/app/shared/models/gender';
import { showGender } from 'src/app/shared/helpers/functions/showFunctions/showGender';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-gender-selector',
  templateUrl: './gender-selector.component.html',
  styleUrls: ['./gender-selector.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => GenderSelectorComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => GenderSelectorComponent),
      multi: true,
    },
  ],
})
export class GenderSelectorComponent extends BaseSelectorComponent<Gender> implements OnInit {
  @Output() genderChange = new EventEmitter<Gender>();

  genders = getEnumElements(Gender);
  selectedGender?: Gender;

  showGender = showGender;
 
  ngOnInit(): void {
    this.selectedGender = this.value;
  }

  onGenderSelected() {
    this.genderChange.emit(this.selectedGender);
    this.onChange(this.selectedGender);
    this.onTouched();
  }

  writeValue(value?: Gender): void {
    this.selectedGender = value;
  }

  validate() {
    return this.validateEnum(this.selectedGender);
  }
}
