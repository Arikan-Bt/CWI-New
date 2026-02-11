import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { DashboardService } from '../../core/services/dashboard.service';
import { StatWidgetComponent } from './components/stat-widget.component';
import { ChartWidgetComponent } from './components/chart-widget.component';
import { TableWidgetComponent } from './components/table-widget.component';
import { BannerWidgetComponent } from './components/banner-widget.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    StatWidgetComponent,
    ChartWidgetComponent,
    TableWidgetComponent,
    BannerWidgetComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Dashboard {
  private service = inject(DashboardService);
  dashboard = toSignal(this.service.getDashboard());
}
