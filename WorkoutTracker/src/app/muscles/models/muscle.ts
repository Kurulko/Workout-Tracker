import { WorkoutModel } from "src/app/shared/models/base/workout-model";
import { ChildMuscle } from "./child-muscle";

export interface Muscle extends WorkoutModel {
    image: string|null;
    isMeasurable: boolean;
    parentMuscleId: number|null;
    parentMuscleName: string|null;
    childMuscles: ChildMuscle[]|null;
}