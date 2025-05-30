import { WorkoutModel } from "src/app/shared/models/base/workout-model";

export interface ChildMuscle extends WorkoutModel {
    image: string|null;
}