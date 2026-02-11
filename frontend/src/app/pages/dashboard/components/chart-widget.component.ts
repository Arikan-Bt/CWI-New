import { Component, Input, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChartModule } from 'primeng/chart';
import { DashboardWidget, ChartData } from '../../../core/models/dashboard.model';

@Component({
  selector: 'app-chart-widget',
  standalone: true,
  imports: [CommonModule, ChartModule],
  template: `
    <div
      class="h-full bg-surface-0 dark:bg-surface-900 shadow-sm p-5 rounded-xl border border-surface-200 dark:border-surface-700"
    >
      <div class="flex items-center justify-between mb-6">
        <div class="text-lg font-semibold text-surface-900 dark:text-surface-0">
          {{ widget.title }}
        </div>
        <i class="pi pi-chart-bar text-surface-300"></i>
      </div>
      <p-chart
        [type]="$any(data.chartType)"
        [data]="chartData"
        [options]="chartOptions"
        height="300px"
      ></p-chart>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChartWidgetComponent implements OnInit {
  @Input({ required: true }) widget!: DashboardWidget;

  chartData: any;
  chartOptions: any;

  get data(): ChartData {
    return this.widget.data as ChartData;
  }

  ngOnInit() {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue('--text-color') || '#495057';
    const textColorSecondary =
      documentStyle.getPropertyValue('--text-color-secondary') || '#6c757d';
    const surfaceBorder = documentStyle.getPropertyValue('--surface-border') || '#dfe7ef';
    const primaryColor = documentStyle.getPropertyValue('--p-primary-500') || '#3B82F6';

    this.chartData = {
      labels: this.data.labels,
      datasets: [
        {
          label: 'Dataset',
          data: this.data.values,
          fill: false,
          borderColor: primaryColor,
          backgroundColor: primaryColor,
          tension: 0.4,
        },
      ],
    };

    this.chartOptions = {
      maintainAspectRatio: false,
      aspectRatio: 0.8,
      plugins: {
        legend: {
          labels: {
            color: textColor,
          },
        },
      },
      scales: {
        x: {
          ticks: {
            color: textColorSecondary,
          },
          grid: {
            color: surfaceBorder,
            drawBorder: false,
          },
        },
        y: {
          ticks: {
            color: textColorSecondary,
          },
          grid: {
            color: surfaceBorder,
            drawBorder: false,
          },
        },
      },
    };
  }
}
