import { TokenModel } from "../../shared/models/tokens/token.model";

export interface AuthResult {
  success: boolean;
  message: string;
  token: TokenModel|null;
}