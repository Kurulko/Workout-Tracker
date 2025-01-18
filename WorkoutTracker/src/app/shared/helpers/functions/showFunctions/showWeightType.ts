import { WeightType } from "../../../models/weight-type";

export function showWeightType(type: WeightType): string {
    return WeightType[type];
}