import { SizeType } from "src/app/shared/models/enums/size-type";

export const oneInchInCentimeters = 2.54;
export const oneCentimeterInInches = 0.393701;

export function convertSizeValue(size: number, fromSizeType: SizeType, toSizeType: SizeType): number {
    if(fromSizeType === toSizeType)
        return size;
    else if(fromSizeType == SizeType.Centimeter && toSizeType == SizeType.Inch) {
        return size * oneCentimeterInInches;
    }
    else if(fromSizeType == SizeType.Inch && toSizeType == SizeType.Centimeter) {
        return size * oneInchInCentimeters;
    }
    else {
        throw new Error(`Unexpected SizeType value: ${fromSizeType} and/or ${toSizeType}`)
    }
}

