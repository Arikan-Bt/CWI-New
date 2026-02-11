import { Component, signal, inject } from '@angular/core';
import {
  RouterOutlet,
  Router,
  NavigationStart,
  NavigationEnd,
  NavigationCancel,
  NavigationError,
} from '@angular/router';
import { LoadingComponent } from './shared/components/loading.component';
import { LoadingService } from './core/services/loading.service';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, LoadingComponent, ToastModule],
  template: `
    <p-toast appendTo="body" />
    <app-loading></app-loading>
    <router-outlet></router-outlet>
  `,
})
export class App {
  protected readonly title = signal('frontend');
  private router = inject(Router);
  private loadingService = inject(LoadingService);

  constructor() {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        this.loadingService.show();
      } else if (
        event instanceof NavigationEnd ||
        event instanceof NavigationCancel ||
        event instanceof NavigationError
      ) {
        setTimeout(() => this.loadingService.hide(), 500); // 500ms delay for smoothness
      }
    });
  }
}
