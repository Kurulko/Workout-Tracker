import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

import { BodyWeight } from './body-weight';
import { EditModelComponent } from '../shared/components/base/edit-model.component';
import { BodyWeightService } from './body-weight.service';
import { PreferencesManager } from '../shared/helpers/managers/preferences-manager';
import { ModelWeight } from '../shared/models/model-weight';
import { ImpersonationManager } from '../shared/helpers/managers/impersonation-manager';
import { TokenManager } from '../shared/helpers/managers/token-manager';

@Component({
  selector: 'app-body-weight-edit',
  templateUrl: './edit-body-weight.component.html',
})
export class BodyWeightEditComponent extends EditModelComponent<BodyWeight> implements OnInit {
  bodyWeight!: BodyWeight;
  maxDate: Date = new Date();

  readonly bodyWeightsPath = "/body-weights";

  constructor(
    private activatedRoute: ActivatedRoute,  
    private bodyWeightService: BodyWeightService, 
    router: Router,  
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(router, impersonationManager, tokenManager, preferencesManager, snackBar);
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.id = idParam ? +idParam : 0;
    if (this.id) {
      // Edit mode
      this.bodyWeightService.getBodyWeightById(this.id)
      .pipe(this.catchLoadDataError(this.bodyWeightsPath))
      .subscribe(result => {
        this.bodyWeight = result;
        this.title = `Edit Body Weight `;
      });
    }
    else {
      // Add mode
      this.title = "Create new Body Weight";
      this.bodyWeight = <BodyWeight>{ date : new Date(), weight: <ModelWeight>{} };

      if(this.preferencesManager.hasPreferableWeightType())
        this.bodyWeight.weight.weightType = this.preferencesManager.getPreferableWeightType()!;
    }
  }

  onSubmit() {
    if (this.id) {
      // Edit mode
      this.bodyWeightService.updateBodyWeight(this.bodyWeight)
      .pipe(this.catchError())
      .subscribe(_ => {
          this.modelUpdatedSuccessfully('Body Weight')
          console.log("BodyWeight " + this.bodyWeight!.id + " has been updated.");
          this.router.navigate([this.bodyWeightsPath]);
      });
    }
    else {
      // Add mode
      this.bodyWeightService.createBodyWeight(this.bodyWeight)
      .pipe(this.catchError())
      .subscribe(result => {
          this.modelAddedSuccessfully('Body Weight')
          console.log("BodyWeight " + result.id + " has been created.");
          this.router.navigate([this.bodyWeightsPath]);
      });
    }
  }
}