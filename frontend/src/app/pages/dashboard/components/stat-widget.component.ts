import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardWidget, StatCardData } from '../../../core/models/dashboard.model';

@Component({
  selector: 'app-stat-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="h-full bg-surface-0 dark:bg-surface-900 shadow-sm p-4 rounded-xl border border-surface-200 dark:border-surface-700 flex flex-col justify-between transition-all hover:shadow-md"
    >
      <div class="flex justify-between items-start mb-2">
        <div>
          <span class="block text-surface-500 dark:text-surface-400 font-medium mb-1">{{
            widget.title
          }}</span>
          <div class="text-surface-900 dark:text-surface-0 font-bold text-2xl">
            {{ data.value }}
          </div>
        </div>
        <div
          class="flex items-center justify-center bg-primary-100 dark:bg-primary-900/30 rounded-xl w-10 h-10"
        >
          <i [class]="data.icon + ' text-primary-500 text-xl'"></i>
        </div>
      </div>
      <span
        [ngClass]="{
          'text-green-500': data.trendDirection === 'Up',
          'text-red-500': data.trendDirection === 'Down',
          'text-surface-500': data.trendDirection === 'Neutral'
        }"
        class="font-medium text-sm"
      >
        {{ data.trend }}
        <span class="text-surface-500 dark:text-surface-400 font-normal ml-1">{{
          data.description
        }}</span>
      </span>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatWidgetComponent {
  @Input({ required: true }) widget!: DashboardWidget;

  get data(): StatCardData {
    return this.widget.data as StatCardData;
  }
}
