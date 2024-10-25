import { AuthModel } from './auth.model';

export interface RegisterModel extends AuthModel {
    email: string|null;
    passwordConfirm: string;
}