import { DbModel } from "../shared/models/db-model.model";
import { WeightType } from "./weight-type";

export interface BodyWeight extends DbModel{
    date: Date;
    weight: number;
    weightType: WeightType;
}

