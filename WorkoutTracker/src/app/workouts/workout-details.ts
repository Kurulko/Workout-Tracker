import { TimeSpan } from "src/app/shared/models/time-span";
import { ModelWeight } from "../shared/models/model-weight";
import { Muscle } from "../muscles/muscle";
import { Equipment } from "../equipments/equipment";
import { Workout } from "./workout";

export interface WorkoutDetails {
    workout: Workout;

    totalWorkouts: number;
    totalWeight: ModelWeight;
    totalDuration: TimeSpan;
    averageWorkoutDuration: TimeSpan;
    countOfDaysSinceFirstWorkout: number;
    dates: Date[]|null;

    muscles: Muscle[];
    equipments: Equipment[];
}