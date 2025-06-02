import { WorkoutModel } from "src/app/shared/models/base/workout-model";

export interface ExerciseAlias extends WorkoutModel {
    exerciseId: number;
    exerciseName: string;
}