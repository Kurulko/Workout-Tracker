import { WeightType } from "src/app/shared/models/enums/weight-type";

export const oneKilogramInPounds = 0.453592;
export const onePoundInKilograms = 2.20462;

export function convertWeightValue(weight: number, fromWeightType: WeightType, toWeightType: WeightType): number {
    if(fromWeightType === toWeightType)
        return weight;
    else if(fromWeightType == WeightType.Kilogram && toWeightType == WeightType.Pound) {
        return weight * onePoundInKilograms;
    }
    else if(fromWeightType == WeightType.Pound && toWeightType == WeightType.Kilogram) {
        return weight * oneKilogramInPounds;
    }
    else {
        throw new Error(`Unexpected WeightType value: ${fromWeightType} and/or ${toWeightType}`)
    }
}

