import { ChildMuscle } from "src/app/muscles/models/child-muscle";
import { Exercise } from "../../exercises/models/exercise";
import { Equipment } from "./equipment";

export interface EquipmentDetails {
    equipment: Equipment;
    exercises: Exercise[]|null;
    muscles: ChildMuscle[]|null;
}