import { ExerciseType } from "src/app/exercises/models/exercise-type";
import { ExerciseSet } from "../../../models/exercises/exercise-set";
import { showWeightTypeShort } from "./showWeightTypeShort";
import { TimeSpan } from "src/app/shared/models/time-span";
import { ModelWeight } from "src/app/shared/models/model-weight";
import { showCountOfSomethingStr } from "./showCountOfSomethingStr";
import { roundNumber } from "../roundNumber";
import { showBigNumberStr } from "./showBigNumberStr";
import { ExerciseRecord } from "src/app/exercises/models/exercise-record";

export function showExerciseValue(exercise: ExerciseSet|ExerciseRecord): string {
  switch (exercise.exerciseType) {
    case ExerciseType.Reps:
      return showReps(exercise.reps!);
    case ExerciseType.Time:
      return showTime(exercise.time!);
    case ExerciseType.WeightAndReps:
      return `${showReps(exercise.reps!)} x ${showWeight(exercise.weight!)}`;
    case ExerciseType.WeightAndTime:
      return `${showTime(exercise.time!)} x ${showWeight(exercise.weight!)}`;
    default:
      throw new Error(`Unexpected exerciseType value`);
  }
}

export function showWeight(modelWeight: ModelWeight): string {
  return `${showBigNumberStr(roundNumber(modelWeight.weight))} ${showWeightTypeShort(modelWeight.weightType)}`;
}

export function showTime(time: TimeSpan): string {
  var timeValuesStr: string[] = [];
  if(time.hours)
    timeValuesStr.push(showCountOfSomethingStr(time.hours, 'hour', 'hours'));

  if(time.minutes)
    timeValuesStr.push(showCountOfSomethingStr(time.minutes, 'minute', 'minutes'));

  if(time.seconds)
    timeValuesStr.push(showCountOfSomethingStr(time.seconds, 'second', 'seconds'));

  return timeValuesStr.join(' ');
}

export function showReps(reps: number): string {
  return showCountOfSomethingStr(reps, 'rep', 'reps');
}