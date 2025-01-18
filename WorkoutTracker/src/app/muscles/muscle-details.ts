import { Exercise } from "../exercises/models/exercise";
import { MuscleSize } from "../muscle-sizes/muscle-size";
import { WorkoutModel } from "../shared/models/workout-model";
import { Muscle } from "./muscle";

export interface MuscleDetails extends WorkoutModel {
    muscle: Muscle;

    exercises: Exercise[]|null;
    muscleSizes: MuscleSize[]|null;
}