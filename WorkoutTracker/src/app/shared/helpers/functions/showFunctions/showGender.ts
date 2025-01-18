import { Gender } from "../../../models/gender";

export function showGender(gender: Gender): string {
    return Gender[gender];
}