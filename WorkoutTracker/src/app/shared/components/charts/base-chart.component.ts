import { Component, Input, EventEmitter, Output, ViewChild, OnInit, OnChanges } from '@angular/core';
import { ImpersonationManager } from 'src/app/shared/helpers/managers/impersonation-manager';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';
import { MatPaginator } from '@angular/material/paginator';
import { MatSnackBar  } from '@angular/material/snack-bar';
import { MainComponent } from '../base/main.component';
import { PreferencesManager } from '../../helpers/managers/preferences-manager';
import { BaseChartDirective } from 'ng2-charts';
import { ChartType } from 'chart.js';

@Component({
  template: '',
})
export abstract class BaseChartComponent<T> extends MainComponent implements OnInit, OnChanges {
    @Input()
    data!: T[];

    @Input()
    isPaginator: boolean = true;

    @Input()
    pageIndex: number = 0;

    @Input()
    pageSize: number = 10;

    @Input()
    totalCount!: number;

    @Input()
    chartType: ChartType = 'line';

    @Input() 
    width?: string;

    @Input()
    pageSizeOptions: number[] = [10, 20, 50];

    @Output() pageChange = new EventEmitter<{ pageIndex: number; pageSize: number }>();

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(BaseChartDirective) chart: BaseChartDirective | undefined; 

    isData: boolean = false;
    constructor(
        impersonationManager: ImpersonationManager, 
        tokenManager: TokenManager,
        preferencesManager: PreferencesManager,
        snackBar: MatSnackBar) 
    {
        super(impersonationManager, tokenManager, preferencesManager, snackBar);
    }

    abstract updateChart(): void;

    ngOnInit() {
        this.onDataChanges();
    }

    ngOnChanges(){
        this.onDataChanges();
    }

    onDataChanges() {
        this.isData = this.data.length > 0;
        this.updateChart();

        if (this.chart) {
            this.chart.chart!.update(); 
        }
    }

    onPageChange(): void {
        this.pageIndex = this.paginator.pageIndex;
        this.pageSize = this.paginator.pageSize;

        this.pageChange.emit({
            pageIndex: this.pageIndex,
            pageSize: this.pageSize,
        });
    }
}