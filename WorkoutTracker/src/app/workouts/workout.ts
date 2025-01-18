import { WorkoutModel } from "src/app/shared/models/workout-model";
import { ModelWeight } from "../shared/models/model-weight";
import { Exercise } from "../exercises/models/exercise";
import { ExerciseSetGroup } from "../shared/models/exercise-set-group";

export interface Workout extends WorkoutModel {
    description: string|null;
    created: Date;
    started: Date|null;
    isPinned: boolean;
    weight: ModelWeight;

    exercises: Exercise[];
    exerciseSetGroups: ExerciseSetGroup[];
}