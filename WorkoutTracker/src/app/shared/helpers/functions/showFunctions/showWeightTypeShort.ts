import { WeightType } from "../../../models/weight-type";

export function showWeightTypeShort(type: WeightType): string {
    switch (type) {
        case WeightType.Kilogram:
            return "kg";
        case WeightType.Pound:
            return "lb";
        default: 
            return '';
            // throw new Error(`Unexpected WeightType value: ${type}`);
    }
}