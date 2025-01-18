import { WorkoutModel } from "../shared/models/workout-model";

export interface ChildMuscle extends WorkoutModel {
    image: string|null;
}