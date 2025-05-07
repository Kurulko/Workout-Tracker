import { Equipment } from "src/app/equipments/equipment";
import { ExerciseType } from "./exercise-type";
import { ChildMuscle } from "src/app/muscles/child-muscle";
import { WorkoutModel } from "src/app/shared/models/workout-model";
import { Workout } from "src/app/workouts/workout";

export interface Exercise extends WorkoutModel {
    image: string|null;
    description: string|null;
    type: ExerciseType;
    isCreatedByUser: boolean;

    equipments: Equipment[];
    workouts: Workout[];
    muscles: ChildMuscle[];
}