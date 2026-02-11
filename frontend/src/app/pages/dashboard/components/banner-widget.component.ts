import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardWidget } from '../../../core/models/dashboard.model';

@Component({
  selector: 'app-banner-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="bg-linear-to-r from-primary-600 to-indigo-600 text-white p-8 rounded-xl shadow-lg relative overflow-hidden"
    >
      <div
        class="absolute top-0 right-0 -mt-4 -mr-4 w-32 h-32 bg-white opacity-10 rounded-full blur-2xl"
      ></div>
      <div class="relative z-10">
        <h2 class="text-3xl font-bold mb-2">{{ widget.title }}</h2>
        <p class="text-primary-100 text-lg opacity-90">{{ data.Message }}</p>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BannerWidgetComponent {
  @Input({ required: true }) widget!: DashboardWidget;
  get data(): any {
    return this.widget.data;
  }
}
