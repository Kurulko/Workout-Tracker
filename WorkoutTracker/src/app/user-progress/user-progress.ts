import { ModelWeight } from "../shared/models/model-weight";
import { TimeSpan } from "../shared/models/time-span";

export interface UserProgress {
    totalWorkouts: number;
    totalWeightLifted: ModelWeight;
    totalDuration: TimeSpan;
    firstWorkoutDate: Date|null;
    countOfDaysSinceFirstWorkout: number;
    averageWorkoutDuration: TimeSpan;
    frequencyPerWeek: number;
    workoutDates: Date[]|null;
}