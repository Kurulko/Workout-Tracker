import { WorkoutModel } from "../shared/models/workout-model.model";

export interface Equipment extends WorkoutModel {
    image: number[]|null;
    isOwnedByUser: boolean;
}