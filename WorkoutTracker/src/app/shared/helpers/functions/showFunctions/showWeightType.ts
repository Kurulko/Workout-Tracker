import { WeightType } from "../../../models/enums/weight-type";

export function showWeightType(type: WeightType): string {
    return WeightType[type];
}