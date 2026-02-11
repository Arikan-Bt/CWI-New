import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { DashboardWidget, TableData } from '../../../core/models/dashboard.model';

@Component({
  selector: 'app-table-widget',
  standalone: true,
  imports: [CommonModule, TableModule],
  template: `
    <div
      class="h-full bg-surface-0 dark:bg-surface-900 shadow-sm p-5 rounded-xl border border-surface-200 dark:border-surface-700"
    >
      <div class="flex items-center justify-between mb-4">
        <h5 class="text-lg font-semibold text-surface-900 dark:text-surface-0 m-0">
          {{ widget.title }}
        </h5>
        <span class="text-xs text-surface-500 cursor-pointer hover:text-primary-500">View All</span>
      </div>
      <p-table
        [value]="data.rows"
        [tableStyle]="{ 'min-width': '100%' }"
        styleClass="p-datatable-sm"
      >
        <ng-template pTemplate="header">
          <tr>
            <th
              *ngFor="let head of data.headers"
              class="bg-surface-50 dark:bg-surface-800 text-surface-600 dark:text-surface-300 font-semibold text-sm"
            >
              {{ head }}
            </th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-row>
          <tr
            class="border-b border-surface-100 dark:border-surface-700 last:border-0 hover:bg-surface-50 dark:hover:bg-surface-800/50 transition-colors"
          >
            <td
              *ngFor="let cell of row"
              class="text-sm py-3 text-surface-700 dark:text-surface-200"
            >
              {{ cell }}
            </td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableWidgetComponent {
  @Input({ required: true }) widget!: DashboardWidget;
  get data(): TableData {
    return this.widget.data as TableData;
  }
}
