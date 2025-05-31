import { Component } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';

import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { BaseChartComponent } from '../base-chart.component';
import { ChartData, ChartOptions } from 'chart.js';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';
import { showSizeTypeShort } from 'src/app/shared/helpers/functions/showFunctions/showSizeTypeShort';
import { SizeType } from 'src/app/shared/models/enums/size-type';
import { MuscleSize } from 'src/app/muscles/models/muscle-size';

@Component({
  selector: 'app-muscle-size-chart',
  templateUrl: './muscle-size-chart.component.html',
  styleUrls: ['./muscle-size-chart.component.css']
})
export class MuscleSizeChartComponent extends BaseChartComponent<MuscleSize> {
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
        label: 'Muscle Size (cm)',
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
          text: 'Size',
        },
        min: 0,
      },
    },
  };

  roundSize = roundNumber;
  showSizeTypeShort = showSizeTypeShort;
  
  updateChart(): void {
    let sizeType = this.data[0]?.size?.sizeType ?? SizeType.Centimeter;
    let sizeTypeShortStr = showSizeTypeShort(sizeType);
    this.lineChartData.datasets[0].label = `Muscle Size (${sizeTypeShortStr})`;

    this.lineChartData.labels = this.data.map((item) => item.date);
    this.lineChartData.datasets[0].data = this.data.map((item) => this.roundSize(item.size.size));
  }
}
