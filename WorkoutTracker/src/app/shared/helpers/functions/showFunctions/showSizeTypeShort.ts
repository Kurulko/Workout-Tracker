import { SizeType } from "../../../models/enums/size-type";

export function showSizeTypeShort(type: SizeType): string {
    switch (type) {
        case SizeType.Centimeter:
            return "cm";
        case SizeType.Inch:
            return "inch";
        default: 
            return '';
            // throw new Error(`Unexpected SizeType value: ${type}`);
     }
}