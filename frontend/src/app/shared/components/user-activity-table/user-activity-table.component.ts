import { Component, input, signal, inject, effect, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { UserService } from '../../../core/services/user.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-user-activity-table',
  standalone: true,
  imports: [CommonModule, TableModule, TagModule],
  template: `
    <div class="user-activity-table h-full flex flex-col">
      <p-table
        [value]="activities()"
        [loading]="loading()"
        [rows]="10"
        [paginator]="true"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} items"
        [scrollable]="true"
        scrollHeight="flex"
        [tableStyle]="{ 'min-width': '60rem' }"
        styleClass="p-datatable-sm h-full flex flex-col"
      >
        <ng-template pTemplate="header">
          <tr>
            <th style="width: 15%">Date</th>
            <th style="width: 15%">Operation</th>
            <th>Description</th>
            <th style="width: 10%">Status</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-activity>
          <tr>
            <td>{{ activity.date | date : 'dd.MM.yyyy HH:mm' }}</td>
            <td>
              <span class="font-bold text-sm uppercase">{{ activity.operation }}</span>
            </td>
            <td class="text-sm">{{ activity.description }}</td>
            <td>
              <p-tag [value]="activity.status" [severity]="getSeverity(activity.status)"></p-tag>
            </td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage">
          <tr>
            <td colspan="4" class="text-center p-8 text-gray-500">
              <i class="pi pi-info-circle mr-2"></i>
              No recent activities found for this user.
            </td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `,
  styles: [
    `
      .user-activity-table {
        flex: 1;
        min-height: 0;
      }
      :host {
        display: flex;
        flex-direction: column;
        height: 100%;
        width: 100%;
        overflow: hidden;
      }
      :host ::ng-deep {
        .p-datatable {
          display: flex;
          flex-direction: column;
          height: 100%;
        }
        .p-datatable-wrapper {
          flex: 1 1 0px; /* Önemli: height hesaplaması için */
          min-height: 0;
        }
        .p-datatable-sm .p-datatable-thead > tr > th {
          padding: 0.75rem 1rem;
          background-color: var(--p-surface-50);
          color: var(--p-surface-700);
          font-size: 11px;
          text-transform: uppercase;
          letter-spacing: 0.05em;
        }
        .p-datatable-sm .p-datatable-tbody > tr > td {
          padding: 0.75rem 1rem;
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserActivityTableComponent {
  userId = input<any>(null);

  private userService = inject(UserService);

  activities = signal<any[]>([]);
  loading = signal(false);

  constructor() {
    effect(() => {
      const id = this.userId();
      if (id) {
        this.loadActivities(id);
      }
    });
  }

  loadActivities(userId: any) {
    this.loading.set(true);
    this.userService
      .getUserActivities(userId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => this.activities.set(data),
        error: (err) => console.error('Failed to load user activities', err),
      });
  }

  getSeverity(
    status: string
  ): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' | undefined {
    switch (status?.toLowerCase()) {
      case 'received':
      case 'completed':
      case 'success':
        return 'success'; // Yeşil

      case 'approved':
        return 'info'; // Mavi (Primary yerine info kullanıldı)

      case 'shipped':
        return 'info'; // Açık Mavi

      case 'pending':
      case 'packedandwaitingshipment':
        return 'warn'; // Sarı

      case 'canceled':
      case 'failed':
      case 'rejected':
      case 'error':
        return 'danger'; // Kırmızı

      case 'preorder':
        return 'contrast'; // Mor yerine Contrast (Siyah/Beyaz) veya Secondary

      case 'draft':
      default:
        return 'secondary'; // Gri
    }
  }
}
