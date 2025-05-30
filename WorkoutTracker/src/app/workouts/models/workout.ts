import { Exercise } from "src/app/exercises/models/exercise";
import { ExerciseSetGroup } from "src/app/shared/models/exercises/exercise-set-group";
import { ModelWeight } from "src/app/shared/models/model-weight";
import { WorkoutModel } from "src/app/shared/models/base/workout-model";

export interface Workout extends WorkoutModel {
    description: string|null;
    created: Date;
    started: Date|null;
    isPinned: boolean;
    weight: ModelWeight;

    exercises: Exercise[];
    exerciseSetGroups: ExerciseSetGroup[];
}