import { ModelWeight } from "../../shared/models/model-weight";
import { TimeSpan } from "../../shared/models/time-span";

export interface WorkoutProgress {
    baseInfoProgress: BaseInfoProgress;
    totalCompletedProgress: TotalCompletedProgress;
    workoutWeightLiftedProgress: WorkoutWeightLiftedProgress;
    strikeDurationProgress: StrikeDurationProgress;
    workoutDurationProgress: WorkoutDurationProgress;
    bodyWeightProgress: BodyWeightProgress;
}

export interface BaseInfoProgress {
    totalWorkouts: number;
    totalDuration: TimeSpan;
    
    countOfExercisesUsed: number;
    frequencyPerWeek: number;
    workoutDates: Date[]|null;
}

export interface TotalCompletedProgress {
    totalWeightLifted: ModelWeight;
    totalRepsCompleted: number;
    totalTimeCompleted: TimeSpan;
}

export interface WorkoutWeightLiftedProgress {
    averageWorkoutWeightLifted: ModelWeight;
    minWorkoutWeightLifted: ModelWeight;
    maxWorkoutWeightLifted: ModelWeight;
}

export interface StrikeDurationProgress {
    averageWorkoutStrikeDays: number;
    maxWorkoutStrikeDays: number;
    maxRestStrikeDays: number;
}

export interface WorkoutDurationProgress {
    averageWorkoutDuration: TimeSpan;
    minWorkoutDuration: TimeSpan;
    maxWorkoutDuration: TimeSpan;
}

export interface BodyWeightProgress {
    averageBodyWeight: ModelWeight;
    minBodyWeight: ModelWeight;
    maxBodyWeight: ModelWeight;
}

