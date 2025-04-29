import { Component, Input, EventEmitter, Output, OnInit, forwardRef } from '@angular/core';
import { BaseSelectorComponent } from '../base-selector.component';
import { getEnumElements } from 'src/app/shared/helpers/functions/getFunctions/getEnumElements';
import { WeightType } from 'src/app/shared/models/weight-type';
import { showWeightType } from 'src/app/shared/helpers/functions/showFunctions/showWeightType';
import { showWeightTypeShort } from 'src/app/shared/helpers/functions/showFunctions/showWeightTypeShort';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';
import { convertWeightValue } from 'src/app/shared/helpers/functions/convertos/convertWeightValue';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';

@Component({
  selector: 'app-weight-type-selector',
  templateUrl: './weight-type-selector.component.html',
  styleUrls: ['./weight-type-selector.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => WeightTypeSelectorComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => WeightTypeSelectorComponent),
      multi: true,
    },
  ],
})
export class WeightTypeSelectorComponent extends BaseSelectorComponent<WeightType> implements OnInit {
  @Input() weight?: number;

  @Input() isShortForm: boolean = false;
  @Output() weightTypeChange = new EventEmitter<WeightType>();

  weightTypes = getEnumElements(WeightType);
  
  selectedWeightType?: WeightType;

  showWeightType = showWeightType;
  showWeightTypeShort = showWeightTypeShort;
 
  ngOnInit(): void {
    this.selectedWeightType = this.value;
  }

  convertWeightValue = convertWeightValue;
  roundNumber = roundNumber;
  
  onWeightTypeSelected() {
    this.weightTypeChange.emit(this.selectedWeightType);
    this.onChange(this.selectedWeightType);
    this.onTouched();
  }

  writeValue(value?: WeightType): void {
    this.selectedWeightType = value;
  }

  validate() {
    return this.validateEnum(this.selectedWeightType);
  }
}
