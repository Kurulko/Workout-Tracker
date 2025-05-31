import { Gender } from "../../../models/enums/gender";

export function showGender(gender: Gender): string {
    return Gender[gender];
}