import { Injectable } from '@angular/core';
import { RouterStateSnapshot, Router, UrlTree, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { TokenManager } from '../managers/token-manager';
import { BaseGuard } from './base.guard';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard extends BaseGuard {
  constructor(tokenManager: TokenManager, router: Router) {
    super(tokenManager, router)
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) : boolean | UrlTree | Observable<boolean | UrlTree> | Promise<boolean | UrlTree> {
    if (this.tokenManager.isAuthenticated()) {
      return true;
    }
    
    this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
}