import { Component, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { ImpersonationManager } from './shared/helpers/managers/impersonation-manager';
import { TokenManager } from './shared/helpers/managers/token-manager';
import { WeightType } from './shared/models/enums/weight-type';
import { SizeType } from './shared/models/enums/size-type';
import { PreferencesManager } from './shared/helpers/managers/preferences-manager';
import { environment } from 'src/environments/environment.prod';

@Component({
    template: '',
})
export abstract class BaseComponent implements OnDestroy {
  private destroySubject = new Subject();

  isLoggedIn: boolean = false;
  isAdmin: boolean = false;
  isImpersonating: boolean = false;
  preferableWeightType?: WeightType;
  preferableSizeType?: SizeType;

  constructor (
    protected impersonationManager: ImpersonationManager, 
    protected tokenManager: TokenManager,
    protected preferencesManager: PreferencesManager
  ) 
  {
    this.subscribeManagers();
    this.initializeData();
  }
  
  subscribeManagers() {
    this.tokenManager.isAuthenticationChanged()
      .pipe(takeUntil(this.destroySubject))
      .subscribe(result => {
        this.isLoggedIn = result;
      });

    this.tokenManager.isAdminChanged()
      .pipe(takeUntil(this.destroySubject))
      .subscribe(result => {
        this.isAdmin = result;
      });

    this.impersonationManager.isImpersonationChanged()
      .pipe(takeUntil(this.destroySubject))
      .subscribe(result => {
        this.isImpersonating = result;
      })

    this.preferencesManager.isPreferableWeightTypeChanged()
      .pipe(takeUntil(this.destroySubject))
      .subscribe(result => {
        this.preferableWeightType = result;
      })

    this.preferencesManager.isPreferableSizeTypeChanged()
      .pipe(takeUntil(this.destroySubject))
      .subscribe(result => {
        this.preferableSizeType = result;
      })
  };

  initializeData(): void {
    this.isLoggedIn = this.tokenManager.isAuthenticated();
    this.isAdmin = this.tokenManager.isAdmin();
    this.isImpersonating = this.impersonationManager.isImpersonating();
    this.preferableWeightType = this.preferencesManager.getPreferableWeightType();
    this.preferableSizeType = this.preferencesManager.getPreferableSizeType();
  }

  ngOnDestroy() {
    this.destroySubject.next(true);
    this.destroySubject.complete();
  }
}