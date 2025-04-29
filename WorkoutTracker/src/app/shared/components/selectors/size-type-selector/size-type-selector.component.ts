import { Component, EventEmitter, Output, Input, OnInit, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';
import { BaseSelectorComponent } from '../base-selector.component';
import { getEnumElements } from 'src/app/shared/helpers/functions/getFunctions/getEnumElements';
import { SizeType } from 'src/app/shared/models/size-type';
import { showSizeType } from 'src/app/shared/helpers/functions/showFunctions/showSizeType';
import { showSizeTypeShort } from 'src/app/shared/helpers/functions/showFunctions/showSizeTypeShort';
import { convertSizeValue } from 'src/app/shared/helpers/functions/convertos/convertSizeValue';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';

@Component({
  selector: 'app-size-type-selector',
  templateUrl: './size-type-selector.component.html',
  styleUrls: ['./size-type-selector.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => SizeTypeSelectorComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SizeTypeSelectorComponent),
      multi: true,
    },
  ],
})
export class SizeTypeSelectorComponent extends BaseSelectorComponent<SizeType> implements OnInit {
  @Input() size?: number;

  @Input() isShortForm: boolean = false;
  @Output() sizeTypeChange = new EventEmitter<SizeType>();
  
  sizeTypes = getEnumElements(SizeType);
  
  selectedSizeType?: SizeType;

  showSizeType = showSizeType;
  showSizeTypeShort = showSizeTypeShort;
 
  ngOnInit(): void {
    this.selectedSizeType = this.value;
  }

  convertSizeValue = convertSizeValue;
  roundNumber = roundNumber;
  
  onSizeTypeSelected() {
    this.sizeTypeChange.emit(this.selectedSizeType);
    this.onChange(this.selectedSizeType);
    this.onTouched();
  }

  writeValue(value?: SizeType): void {
    this.selectedSizeType = value;
  }

  validate() {
    return this.validateEnum(this.selectedSizeType);
  }
}
