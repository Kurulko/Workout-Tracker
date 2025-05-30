import { DbModel } from "src/app/shared/models/base/db-model";
import { ModelWeight } from "src/app/shared/models/model-weight";

export interface BodyWeight extends DbModel {
    date: Date;
    weight: ModelWeight;
}

