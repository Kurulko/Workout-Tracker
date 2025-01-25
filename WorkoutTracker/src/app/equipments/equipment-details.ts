import { Exercise } from "../exercises/models/exercise";
import { ChildMuscle } from "../muscles/child-muscle";
import { Equipment } from "./equipment";

export interface EquipmentDetails {
    equipment: Equipment;
    exercises: Exercise[]|null;
    muscles: ChildMuscle[]|null;
}