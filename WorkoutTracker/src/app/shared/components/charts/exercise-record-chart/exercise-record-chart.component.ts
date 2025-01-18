import { Component, Input } from '@angular/core';
import { MatSnackBar  } from '@angular/material/snack-bar';

import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { PreferencesManager } from 'src/app/shared/helpers/managers/preferences-manager';
import { BaseChartComponent } from '../base-chart.component';
import { ChartData, ChartOptions } from 'chart.js';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';
import { ExerciseRecord } from 'src/app/exercise-records/exercise-record';
import { ExerciseType } from 'src/app/exercises/models/exercise-type';

@Component({
  selector: 'app-exercise-record-chart',
  templateUrl: './exercise-record-chart.component.html',
  styleUrls: ['./exercise-record-chart.component.css']
})
export class ExerciseRecordChartComponent extends BaseChartComponent<ExerciseRecord> {
  @Input()
  exerciseType!: ExerciseType;

  constructor(
    impersonationManager: ImpersonationManager, 
    tokenManager: TokenManager,
    preferencesManager: PreferencesManager,
    snackBar: MatSnackBar) 
  {
    super(impersonationManager, tokenManager, preferencesManager, snackBar);
    this.chartType = 'bar';
  } 

  public lineChartData: ChartData = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Exercise Record',
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
        title: {
          display: true,
          text: 'Value',
        },
        min: 0,
      },
    },
  };

  roundWeight = roundNumber;
  
  updateChart(): void {
    const measurementStr = (() => {
      switch (this.exerciseType) {
        case ExerciseType.Reps:
          return "Reps";
        case ExerciseType.Time:
          return "Time - seconds";
        case ExerciseType.WeightAndReps:
          return "Weight - weight * reps";
        case ExerciseType.WeightAndTime:
          return "Weight and Time - weight * seconds";
        default:
          throw new Error(`Unexpected exerciseType value`);
      }
    })();

    this.lineChartData.datasets[0].label = `Exercise Record (${measurementStr})`;

    this.lineChartData.labels = this.data.filter(d => d.exerciseType === this.exerciseType).map((item) => item.date);
    this.lineChartData.datasets[0].data = this.data.filter(d => d.exerciseType === this.exerciseType).map((item) => {
      switch (item.exerciseType) {
        case ExerciseType.Reps:
          return item.reps;
        case ExerciseType.Time:
          return item.time!.hours * 60 * 60 + item.time!.minutes * 60 + item.time!.seconds;
        case ExerciseType.WeightAndReps:
          return this.roundWeight(item.weight!.weight * item.reps!);
        case ExerciseType.WeightAndTime:
            return this.roundWeight((item.time!.hours * 60 * 60 + item.time!.minutes * 60 + item.time!.seconds) * item.weight!.weight);
        default:
          throw new Error(`Unexpected exerciseType value`);
      }
    });
  }
}
