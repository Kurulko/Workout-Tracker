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
import { WorkoutRecord } from 'src/app/workouts/models/workout-record';

@Component({
  selector: 'app-workout-record-chart',
  templateUrl: './workout-record-chart.component.html',
  styleUrls: ['./workout-record-chart.component.css']
})
export class WorkoutRecordChartComponent extends BaseChartComponent<WorkoutRecord> {
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
        label: 'Workout Record (kg)',
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
      },
      y: {
        beginAtZero: true,
        title: {
          display: true,
          text: 'Weight',
        },
        min: 0,
      },
    },
  };

  roundWeight = roundNumber;
  showWeightTypeShort = showWeightTypeShort;
  
  updateChart(): void {
    let weightType = this.data[0]?.weight?.weightType ?? WeightType.Kilogram;
    let weightTypeShortStr = showWeightTypeShort(weightType);
    this.lineChartData.datasets[0].label = `Workout Record (${weightTypeShortStr})`;

    this.lineChartData.labels = this.data.map((item) => item.date);
    this.lineChartData.datasets[0].data = this.data.map((item) => this.roundWeight(item.weight.weight));
  }
}
