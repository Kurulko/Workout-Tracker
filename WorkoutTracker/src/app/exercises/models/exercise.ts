import { Equipment } from "src/app/equipments/models/equipment";
import { ChildMuscle } from "src/app/muscles/models/child-muscle";
import { WorkoutModel } from "src/app/shared/models/base/workout-model";
import { Workout } from "src/app/workouts/models/workout";
import { ExerciseType } from "./exercise-type";

export interface Exercise extends WorkoutModel {
    image: string|null;
    description: string|null;
    type: ExerciseType;
    isCreatedByUser: boolean;
    aliases: string[];

    equipments: Equipment[];
    workouts: Workout[];
    muscles: ChildMuscle[];
}