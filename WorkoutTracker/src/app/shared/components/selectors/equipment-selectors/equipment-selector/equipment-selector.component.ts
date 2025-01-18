import { Component, EventEmitter, Output, Input, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { EquipmentService } from 'src/app/equipments/equipment.service';
import { BaseEquipmentSelectorComponent } from '../base-equipment-selector.component';

@Component({
  selector: 'app-equipment-selector',
  templateUrl: './equipment-selector.component.html',
  styleUrls: ['./equipment-selector.component.css'],
  providers: [
      {
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => EquipmentSelectorComponent),
        multi: true,
      },
  ],
})
export class EquipmentSelectorComponent extends BaseEquipmentSelectorComponent<number> {
  @Output() equipmentIdChange = new EventEmitter<number>();
 
  selectedEquipmentId?: number;

  constructor(equipmentService: EquipmentService) {
    super(equipmentService);
  }

  onEquipmentIdSelected() {
    this.equipmentIdChange.emit(this.selectedEquipmentId);
    this.onChange(this.selectedEquipmentId);
    this.onTouched();
  }

  writeValue(value?: number): void {
    this.selectedEquipmentId = value;
  }
}
