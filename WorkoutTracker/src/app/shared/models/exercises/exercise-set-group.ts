import { DbModel } from "src/app/shared/models/base/db-model";
import { ExerciseType } from "src/app/exercises/models/exercise-type";
import { ExerciseSet } from "./exercise-set";
import { ModelWeight } from "../model-weight";

export interface ExerciseSetGroup extends DbModel {
    sets: number;
    weight: ModelWeight;

    exerciseId: number;
    exerciseName?: string;
    exerciseType?: ExerciseType;

    exerciseSets: ExerciseSet[];
}