import { DbModel } from "src/app/shared/models/db-model";
import { TimeSpan } from "src/app/shared/models/time-span";
import { ModelWeight } from "../shared/models/model-weight";
import { ExerciseRecordGroup } from "../shared/models/exercise-record-group";
import { Exercise } from "../exercises/models/exercise";

export interface WorkoutRecord extends DbModel {
    time: TimeSpan;
    date: Date;
    weight: ModelWeight;

    workoutId: number;
    workoutName: string;

    exercises: Exercise[];
    exerciseRecordGroups: ExerciseRecordGroup[];
}