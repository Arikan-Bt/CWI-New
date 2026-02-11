import {
  Component,
  EventEmitter,
  Input,
  Output,
  model,
  output,
  ChangeDetectionStrategy,
  effect,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-full-screen-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (visible()) {
    <div
      class="modal-container fixed inset-0 z-[1050] flex flex-col w-screen h-screen overflow-hidden transition-colors duration-200 bg-white dark:bg-surface-950 text-gray-700 dark:text-surface-0"
      style="z-index: 1050 !important;"
    >
      <!-- Header -->
      <div
        class="modal-header flex items-center justify-between px-8 py-6 border-b shrink-0 transition-colors duration-200 border-gray-100 dark:border-surface-800"
      >
        <div class="flex-1 flex items-center">
          <ng-content select="[header]"></ng-content>
        </div>
        <button
          (click)="close($event)"
          type="button"
          class="modal-close-button print:hidden p-2 ml-4 rounded-full transition-colors focus:outline-none hover:bg-gray-100 dark:hover:bg-surface-800 text-gray-400 dark:text-muted-color cursor-pointer"
          aria-label="Close"
          style="z-index: 1051;"
        >
          <i class="pi pi-times text-xl"></i>
        </button>
      </div>

      <!-- Body -->
      <div
        class="modal-body flex-1 overflow-y-auto relative flex flex-col w-full transition-colors duration-200 bg-white dark:bg-surface-950"
      >
        <ng-content></ng-content>
      </div>

      <!-- Footer -->
      <div
        class="modal-footer shrink-0 border-t p-4 mt-auto w-full transition-colors duration-200 bg-white dark:bg-surface-950 border-gray-100 dark:border-surface-800"
      >
        <ng-content select="[footer]"></ng-content>
      </div>
    </div>
    }
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FullScreenModalComponent implements OnDestroy {
  // Using model() for two-way binding with signals
  visible = model<boolean>(false);
  onClose = output<void>();

  constructor() {
    effect(() => {
      if (this.visible()) {
        document.body.style.overflow = 'hidden';
      } else {
        document.body.style.overflow = '';
      }
    });
  }

  ngOnDestroy() {
    document.body.style.overflow = '';
  }

  close(event?: MouseEvent) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.visible.set(false);
    this.onClose.emit();
  }
}
