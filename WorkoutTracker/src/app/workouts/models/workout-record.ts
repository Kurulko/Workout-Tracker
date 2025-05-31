import { Exercise } from "src/app/exercises/models/exercise";
import { DbModel } from "src/app/shared/models/base/db-model";
import { ExerciseRecordGroup } from "src/app/shared/models/exercises/exercise-record-group";
import { ModelWeight } from "src/app/shared/models/model-weight";
import { TimeSpan } from "src/app/shared/models/time-span";

export interface WorkoutRecord extends DbModel {
    time: TimeSpan;
    date: Date;
    weight: ModelWeight;

    workoutId: number;
    workoutName: string;

    exercises: Exercise[];
    exerciseRecordGroups: ExerciseRecordGroup[];
}