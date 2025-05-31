import { DbModel } from "./db-model";

export interface WorkoutModel extends DbModel {
    name: string;
}