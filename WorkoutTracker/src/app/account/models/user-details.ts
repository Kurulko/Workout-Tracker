import { Gender } from "src/app/shared/models/gender";
import { ModelSize } from "../../shared/models/model-size";
import { ModelWeight } from "../../shared/models/model-weight";

export interface UserDetails {
    gender: Gender|null;
    height: ModelSize|null;
    weight: ModelWeight|null;
    dateOfBirth: Date|null;
    bodyFatPercentage: number|null;
}