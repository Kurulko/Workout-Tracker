import { SizeType } from "../../../models/enums/size-type";

export function showSizeType(type: SizeType): string {
    return SizeType[type];
}