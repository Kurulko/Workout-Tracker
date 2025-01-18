import { ModelWeight } from "src/app/shared/models/model-weight";
import { Exercise } from "./exercise";
import { TimeSpan } from "src/app/shared/models/time-span";

export interface ExerciseDetails {
    exercise: Exercise;
    countOfTimes: number;

    sumOfWeight: ModelWeight|null;
    sumOfTime: TimeSpan|null;
    sumOfReps: number|null;
}