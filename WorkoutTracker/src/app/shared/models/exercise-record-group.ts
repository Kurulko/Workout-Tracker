import { DbModel } from "src/app/shared/models/db-model";
import { ExerciseType } from "src/app/exercises/models/exercise-type";
import { ModelWeight } from "./model-weight";
import { ExerciseRecord } from "src/app/exercise-records/exercise-record";

export interface ExerciseRecordGroup extends DbModel {
    sets: number;
    weight: ModelWeight;

    exerciseId: number;
    exerciseName: string;
    exerciseType: ExerciseType;

    exerciseRecords: ExerciseRecord[];
}