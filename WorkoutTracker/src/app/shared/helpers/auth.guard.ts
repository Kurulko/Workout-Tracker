import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { TokenManager } from './token-manager';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard {
  constructor(private tokenManager: TokenManager, private router: Router) {
    
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) : Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    if (this.tokenManager.isAuthenticated()) {
      return true;
    }
    
    this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
}