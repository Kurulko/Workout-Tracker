import { DbModel } from "../shared/models/db-model";
import { ModelSize } from "../shared/models/model-size";

export interface MuscleSize extends DbModel {
    date: Date;
    size: ModelSize;
    muscleId: number;
    muscleName: string;
    musclePhoto: string|null;
}

