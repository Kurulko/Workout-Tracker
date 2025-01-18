import { Exercise } from "src/app/exercises/models/exercise";

export function getSomeExercises(exercises: Exercise[], countOfExercisesShown: number): Exercise[] {
  return exercises.slice(0, countOfExercisesShown);
}