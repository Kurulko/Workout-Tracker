import { DbModel } from "./db-model.model";

export interface WorkoutModel extends DbModel{
    name: string;
}