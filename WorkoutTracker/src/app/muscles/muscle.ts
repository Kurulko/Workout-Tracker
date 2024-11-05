import { WorkoutModel } from "../shared/models/workout-model.model";

export interface Muscle extends WorkoutModel {
    image: number[]|null;
    parentMuscleId: number|null;
    parentMuscle: Muscle|null;
    childMuscles: Muscle[]|null;
}