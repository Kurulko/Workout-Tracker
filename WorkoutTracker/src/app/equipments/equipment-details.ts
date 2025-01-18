import { Exercise } from "../exercises/models/exercise";
import { Equipment } from "./equipment";

export interface EquipmentDetails {
    equipment: Equipment;
    exercises: Exercise[]|null;
}