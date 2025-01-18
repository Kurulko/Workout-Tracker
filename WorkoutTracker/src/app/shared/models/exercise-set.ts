import { ExerciseType } from "src/app/exercises/models/exercise-type";
import { DbModel } from "./db-model";
import { TimeSpan } from "./time-span";
import { ModelWeight } from "src/app/shared/models/model-weight";

export interface ExerciseSet extends DbModel {
    totalWeight: ModelWeight|null;
    totalTime: TimeSpan|null;
    totalReps: number|null;

    weight: ModelWeight|null;
    time: TimeSpan|null;
    reps: number|null;

    exerciseId: number;
    exerciseName: string;
    exerciseType: ExerciseType;
    exercisePhoto: string|null;
}