import { Component, EventEmitter, Output, Input, forwardRef } from '@angular/core';
import { NG_VALIDATORS, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Equipment } from 'src/app/equipments/equipment';
import { EquipmentService } from 'src/app/equipments/equipment.service';
import { BaseEquipmentSelectorComponent } from '../base-equipment-selector.component';

@Component({
  selector: 'app-multiple-equipment-selector',
  templateUrl: './multiple-equipment-selector.component.html',
  styleUrls: ['./multiple-equipment-selector.component.css'],
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => MultipleEquipmentSelectorComponent),
      multi: true,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MultipleEquipmentSelectorComponent),
      multi: true,
    },
  ],
})
export class MultipleEquipmentSelectorComponent extends BaseEquipmentSelectorComponent<Equipment[]> {
  @Output() equipmentsChange = new EventEmitter<Equipment[]>();
  
  selectedEquipments?: Equipment[];

  constructor(equipmentService: EquipmentService) {
    super(equipmentService);
  }

  onEquipmentsSelected() {
    var equipments = this.isNoneOptionSelected ? [] : this.selectedEquipments;

    this.equipmentsChange.emit(equipments);
    this.onChange(equipments);
    this.onTouched();
  }

  writeValue(value?: Equipment[]): void {
    this.selectedEquipments = value;
  }

  isDisabledNoneOption(): boolean {
    if(!this.selectedEquipments)
      return false;

    return this.selectedEquipments.length > 0 && !this.isNoneOptionSelected;
  }

  isNoneOptionSelected = false;
  noneOptionSelected(){
    this.isNoneOptionSelected = !this.isNoneOptionSelected;
  }

  validate() {
    return this.validateItems(this.selectedEquipments, this.isNoneOptionSelected);
  }
}
