import { DbModel } from "src/app/shared/models/base/db-model";
import { ModelSize } from "src/app/shared/models/model-size";

export interface MuscleSize extends DbModel {
    date: Date;
    size: ModelSize;
    muscleId: number;
    muscleName: string;
    musclePhoto: string|null;
}

