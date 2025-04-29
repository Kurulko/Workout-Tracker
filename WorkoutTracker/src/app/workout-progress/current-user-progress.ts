import { ModelWeight } from "../shared/models/model-weight";
import { WorkoutStatus } from "./workout-status";

export interface CurrentUserProgress {
    workoutStatus: WorkoutStatus;

    countOfWorkoutDays: number;
    firstWorkoutDate: Date|null;
    lastWorkoutDate: Date|null;

    currentWorkoutStrikeDays: number;
    currentBodyWeight: ModelWeight;
}
