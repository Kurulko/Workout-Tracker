import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { AuthResult } from '../models/auth-result.model';
import { MainComponent } from '../../shared/components/base/main.component';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';

export abstract class AuthComponent extends MainComponent {
  authResult?: AuthResult;
  returnUrl: string | null = null;

  constructor(protected router: Router, 
    route: ActivatedRoute, 
    protected authService: AuthService, 
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);

    route.queryParams.subscribe(params => {
      this.returnUrl = params['returnUrl'] || null;
    });
  }

  abstract getAuthResult() : Observable<AuthResult>;

  pipeAuthResult() : Observable<AuthResult> {
    return this.getAuthResult().pipe(this.catchError());
  }    
}