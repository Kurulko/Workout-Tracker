import { Exercise } from "../../exercises/models/exercise";
import { WorkoutModel } from "../../shared/models/base/workout-model";
import { Muscle } from "./muscle";
import { MuscleSize } from "./muscle-size";

export interface MuscleDetails extends WorkoutModel {
    muscle: Muscle;

    exercises: Exercise[]|null;
    muscleSizes: MuscleSize[]|null;
}