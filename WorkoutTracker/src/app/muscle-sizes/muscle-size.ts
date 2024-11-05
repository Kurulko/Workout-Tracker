import { Muscle } from "../muscles/muscle";
import { DbModel } from "../shared/models/db-model.model";
import { SizeType } from "./size-type";

export interface MuscleSize extends DbModel {
    date: Date;
    size: number;
    sizeType: SizeType;
    muscleId: number;
    muscle: Muscle;
}

