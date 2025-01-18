import { ExerciseType } from "src/app/exercises/models/exercise-type";

export function showExerciseType(type: ExerciseType): string {
  switch (type) {
    case ExerciseType.WeightAndReps:
      return "Weight and Reps";
    case ExerciseType.WeightAndTime:
      return "Weight and Time";
    default:
      return ExerciseType[type];
  }
}