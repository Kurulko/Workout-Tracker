import { WorkoutModel } from "../shared/models/workout-model";
import { ChildMuscle } from "./child-muscle";

export interface Muscle extends WorkoutModel {
    image: string|null;
    imageFile: File|null;
    
    isMeasurable: boolean;
    parentMuscleId: number|null;
    parentMuscleName: string|null;
    childMuscles: ChildMuscle[]|null;
}