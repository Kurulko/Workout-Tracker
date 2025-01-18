import { SizeType } from "../../../models/size-type";

export function showSizeType(type: SizeType): string {
    return SizeType[type];
}