import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../core/services/loading.service';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="loadingService.loading()" class="loading-container">
      <div class="loading-content">
        <img src="/assets/images/Logo.png" alt="Arıkan Saat Logo" class="loading-logo" />
        <div class="spinner"></div>
        <span class="loading-text">Please Waiting...</span>
      </div>
    </div>
  `,
  styles: [
    `
      .loading-container {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(var(--surface-ground), 0.9);
        backdrop-filter: blur(8px);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 9999;
      }

      .loading-content {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 1.5rem;
      }

      .loading-logo {
        height: 4rem;
        width: auto;
        animation: pulse 2s infinite ease-in-out;
      }

      .loading-text {
        color: var(--primary-color);
        font-size: 1.25rem;
        font-weight: 500;
        letter-spacing: 0.1rem;
      }

      .spinner {
        width: 40px;
        height: 40px;
        border: 3px solid var(--surface-200);
        border-top-color: var(--primary-color);
        border-radius: 50%;
        animation: spin 1s linear infinite;
      }

      @keyframes spin {
        to {
          transform: rotate(360deg);
        }
      }

      @keyframes pulse {
        0% {
          transform: scale(1);
          opacity: 0.8;
        }
        50% {
          transform: scale(1.05);
          opacity: 1;
        }
        100% {
          transform: scale(1);
          opacity: 0.8;
        }
      }
    `,
  ],
})
export class LoadingComponent {
  loadingService = inject(LoadingService);
}
