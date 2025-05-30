import { Component } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';

import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { BaseChartComponent } from '../base-chart.component';
import { CartesianScaleOptions, ChartData, ChartOptions } from 'chart.js';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';
import { showWeightTypeShort } from 'src/app/shared/helpers/functions/showFunctions/showWeightTypeShort';
import { WeightType } from 'src/app/shared/models/enums/weight-type';
import { BodyWeight } from 'src/app/body-weights/models/body-weight';

@Component({
  selector: 'app-body-weight-chart',
  templateUrl: './body-weight-chart.component.html',
  styleUrls: ['./body-weight-chart.component.css']
})
export class BodyWeightChartComponent extends BaseChartComponent<BodyWeight> {
  constructor(
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
  } 

  public lineChartData: ChartData = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Body Weight (kg)',
        borderColor: '#42A5F5',
        backgroundColor: 'rgba(66, 165, 245, 0.2)',
        fill: true,
      },
    ],
  };

  public lineChartOptions: ChartOptions = {
    responsive: true,
    scales: {
      x: {
        type: 'time',
        time: {
          unit: 'day',
        },
        title: {
          display: true,
          text: 'Date',
        },
      },// as unknown as CartesianScaleOptions,
      y: {
        beginAtZero: true,
        title: {
          display: true,
          text: 'Weight',
        },
        min: 0,
      },// as unknown as CartesianScaleOptions,
    },
  };

  roundWeight = roundNumber;
  showWeightTypeShort = showWeightTypeShort;
  
  updateChart(): void {
    let weightType = this.data[0]?.weight?.weightType ?? WeightType.Kilogram;
    let weightTypeShortStr = showWeightTypeShort(weightType);
    this.lineChartData.datasets[0].label = `Body Weight (${weightTypeShortStr})`;
    // this.lineChartOptions!.scales!.[x]!.title!.text = 
    // if(this.lineChartOptions.scales?.['y']?.title?.text)
    // this.lineChartOptions.scales?.['y']?.title?.text = `Weight (${weightTypeShortStr})`; 

    this.lineChartData.labels = this.data.map((item) => item.date);
    this.lineChartData.datasets[0].data = this.data.map((item) => this.roundWeight(item.weight.weight));
  }
}
