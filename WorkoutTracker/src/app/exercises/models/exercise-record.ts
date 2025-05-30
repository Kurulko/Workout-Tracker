import { ExerciseSet } from "src/app/shared/models/exercises/exercise-set";

export interface ExerciseRecord extends ExerciseSet {
    date: Date;
}