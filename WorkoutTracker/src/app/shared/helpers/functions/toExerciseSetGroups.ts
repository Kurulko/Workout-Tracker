import { ExerciseRecordGroup } from "../../models/exercise-record-group";
import { ExerciseSet } from "../../models/exercise-set";
import { ExerciseSetGroup } from "../../models/exercise-set-group";

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