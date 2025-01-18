import { DbModel } from "../shared/models/db-model";
import { ModelWeight } from "../shared/models/model-weight";

export interface BodyWeight extends DbModel {
    date: Date;
    weight: ModelWeight;
}

