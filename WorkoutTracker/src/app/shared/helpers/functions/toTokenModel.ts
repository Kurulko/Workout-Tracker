import { TokenModel } from "../../models/token.model";
import { TokenViewModel } from "../../models/token.view-model";

export function toTokenModel(tokenViewModel: TokenViewModel|null) : TokenModel|null {
    if(tokenViewModel == null)
        return null;
    
    const tokenModel =<TokenModel>{};

    tokenModel.tokenStr = tokenViewModel.tokenStr;
    tokenModel.roles = tokenViewModel.roles;

    const today = new Date();
    today.setDate(today.getDate() + tokenViewModel.expirationDays);
    tokenModel.expirationDate = today;

    return tokenModel;
}