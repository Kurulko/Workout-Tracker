import { TokenModel } from "../../shared/models/token.model";

export interface AuthResult {
    success: boolean;
    message: string;
    token: TokenModel|null;
  }