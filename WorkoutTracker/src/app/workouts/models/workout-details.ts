import { TimeSpan } from "src/app/shared/models/time-span";
import { ModelWeight } from "../../shared/models/model-weight";
import { Workout } from "./workout";
import { Muscle } from "src/app/muscles/models/muscle";
import { Equipment } from "src/app/equipments/models/equipment";

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