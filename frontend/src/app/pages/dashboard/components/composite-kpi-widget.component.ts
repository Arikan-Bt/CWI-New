import { ChangeDetectionStrategy, Component, Input, LOCALE_ID, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CompositeKpiData, DashboardWidget } from '../../../core/models/dashboard.model';

@Component({
  selector: 'app-composite-kpi-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      [attr.data-locale]="locale"
      class="h-full bg-surface-0 dark:bg-surface-900 shadow-sm p-5 rounded-xl border border-surface-200 dark:border-surface-700"
    >
      <div class="flex items-center justify-between mb-4">
        <h5 class="text-lg font-semibold text-surface-900 dark:text-surface-0 m-0">
          {{ widget.title }}
        </h5>
        <i class="pi pi-th-large text-surface-300"></i>
      </div>

      @if (data.sections.length > 0) {
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-5">
        @for (section of data.sections; track section.title) {
        <div class="rounded-xl border border-surface-200 dark:border-surface-700 p-4">
          <div class="text-sm font-semibold text-surface-600 dark:text-surface-300 mb-3">
            {{ section.title }}
          </div>

          <div class="grid grid-cols-1 gap-3">
            @for (item of section.items; track item.label) {
            <div class="rounded-lg bg-surface-50 dark:bg-surface-800/60 p-3">
              <div class="flex items-start justify-between gap-3">
                <div>
                  <div class="text-xs text-surface-500 dark:text-surface-400">{{ item.label }}</div>
                  <div class="text-xl font-bold text-surface-900 dark:text-surface-0">
                    {{ item.value }}
                  </div>
                </div>
                <i [class]="item.icon + ' text-primary-500 text-lg'"></i>
              </div>
              <div
                class="text-xs mt-1"
                [ngClass]="{
                  'text-green-500': item.trendDirection === 'Up',
                  'text-red-500': item.trendDirection === 'Down',
                  'text-surface-500': item.trendDirection === 'Neutral'
                }"
              >
                {{ item.trend }}
              </div>
            </div>
            }
          </div>
        </div>
        }
      </div>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompositeKpiWidgetComponent {
  @Input({ required: true }) widget!: DashboardWidget;
  readonly locale = inject(LOCALE_ID);

  get data(): CompositeKpiData {
    const source = this.widget.data as Partial<CompositeKpiData>;
    return { sections: source.sections ?? [] };
  }
}
