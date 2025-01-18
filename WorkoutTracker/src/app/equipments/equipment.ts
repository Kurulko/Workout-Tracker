import { WorkoutModel } from "../shared/models/workout-model";

export interface Equipment extends WorkoutModel {
    image: string|null;
    imageFile: File|null;
    
    isOwnedByUser: boolean;
}