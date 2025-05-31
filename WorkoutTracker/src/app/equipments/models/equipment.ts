import { WorkoutModel } from "src/app/shared/models/base/workout-model";

export interface Equipment extends WorkoutModel {
    image: string|null;
    
    isOwnedByUser: boolean;
}