import { Component, Input, OnInit } from '@angular/core';
import { Chart, ChartData, ChartOptions, registerables } from 'chart.js';
import { roundNumber } from 'src/app/shared/helpers/functions/roundNumber';
import DataLabelsPlugin from 'chartjs-plugin-datalabels';
import { showCountOfSomethingStr } from 'src/app/shared/helpers/functions/showFunctions/showCountOfSomethingStr';

@Component({
  selector: 'app-workouts-vs-rest-chart',
  templateUrl: './workouts-vs-rest-chart.component.html',
  styleUrls: ['./workouts-vs-rest-chart.component.css']
})
export class WorkoutsVsRestChartComponent implements OnInit {
  @Input()
  width?: string;

  @Input()
  workoutDays!: number;

  @Input()
  restDays!: number;

  totalDays!: number;

  constructor(){
    Chart.register(...registerables, DataLabelsPlugin);
  }

  ngOnInit(): void {
    this.workoutsChartData.datasets[0].data =  [this.restDays, this.workoutDays];
    this.totalDays = this.restDays + this.workoutDays;
  }

  workoutsChartData: ChartData = {
    labels:  ['Rest', 'Workouts'],
    datasets: [
      {
        data: [],
        backgroundColor: ['#FF6384', '#36A2EB'],
      },
    ],
  };

  workoutsChartOptions: ChartOptions = {
      responsive: true,
      plugins: {
        datalabels: {
          formatter: (value, context) => {
            const percentage = this.getPercentageFromValue(value);
            return `${percentage}%`;
          },
          color: '#fff',
          font: {
            weight: 'bold',
            size: 16
          },
          anchor: 'center',
          align: 'center', 
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              const value = context.raw as number;
              const percentage = this.getPercentageFromValue(value);
              return `${context.label}: ${value} days/${this.totalDays} (${percentage}%)`;
            }
          }
        }
      }
    };
  
    getPercentageFromValue(value: number): number {
      const percentage = roundNumber(((value / this.totalDays) * 100), 0);
      return percentage;
    }

    showCountOfDays(days: number){
      return showCountOfSomethingStr(days, "day", "days")
    }
}
