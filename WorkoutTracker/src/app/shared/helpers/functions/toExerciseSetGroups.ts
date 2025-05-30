import { ExerciseRecordGroup } from "../../models/exercises/exercise-record-group";
import { ExerciseSet } from "../../models/exercises/exercise-set";
import { ExerciseSetGroup } from "../../models/exercises/exercise-set-group";

export function toExerciseSetGroups(exerciseRecordGroups: ExerciseRecordGroup[]): ExerciseSetGroup[] {
    return exerciseRecordGroups.map(esg => <ExerciseSetGroup>{
      id: esg.id,
      exerciseId: esg.exerciseId,
      exerciseName: esg.exerciseName,
      exerciseType: esg.exerciseType,
      exerciseSets: esg.exerciseRecords.map(es => <ExerciseSet>{
        id: es.id,
        exerciseId: es.exerciseId,
        exerciseName: es.exerciseName,
        exerciseType: es.exerciseType,
        reps: es.reps,
        time: es.time,
        weight: es.weight,
      })
    });
  }