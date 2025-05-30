import { ExerciseRecord } from "src/app/exercises/models/exercise-record";
import { ExerciseRecordGroup } from "../../models/exercises/exercise-record-group";
import { ExerciseSetGroup } from "../../models/exercises/exercise-set-group";

export function toExerciseRecordGroups(exerciseSetGroups: ExerciseSetGroup[], date: Date): ExerciseRecordGroup[] {
    return exerciseSetGroups.map(esg => <ExerciseRecordGroup>{
      id: esg.id,
      exerciseId: esg.exerciseId,
      exerciseRecords: esg.exerciseSets.map(es => <ExerciseRecord>{
        id: es.id,
        date : date,
        exerciseId: es.exerciseId,
        reps: es.reps,
        time: es.time,
        weight: es.weight,
      })
    });
}