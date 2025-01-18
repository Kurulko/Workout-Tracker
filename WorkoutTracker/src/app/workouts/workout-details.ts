import { TimeSpan } from "src/app/shared/models/time-span";
import { ModelWeight } from "../shared/models/model-weight";
import { Muscle } from "../muscles/muscle";
import { Equipment } from "../equipments/equipment";
import { Workout } from "./workout";

export interface WorkoutDetails {
    workout: Workout;

    countOfWorkouts: number;
    sumOfWeight: ModelWeight;
    sumOfTime: TimeSpan;

    muscles: Muscle[];
    equipments: Equipment[];
}