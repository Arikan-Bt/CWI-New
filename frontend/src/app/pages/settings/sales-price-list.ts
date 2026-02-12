import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { MessageService } from 'primeng/api';
import { SalesPriceService, ProductSalesPriceDto } from '../../core/services/sales-price.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-sales-price-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    FormsModule,
    IconFieldModule,
    InputIconModule,
  ],
  providers: [MessageService],
  template: `
    <div class="card">
      <div class="flex items-center justify-between mb-4">
        <div class="font-semibold text-xl">Sales Price List</div>
        <div class="flex gap-2">
          <p-iconfield iconPosition="left">
            <p-inputicon class="pi pi-search"></p-inputicon>
            <input
              pInputText
              type="text"
              (input)="dt.filterGlobal($any($event.target).value, 'contains')"
              placeholder="Search Prices..."
            />
          </p-iconfield>
        </div>
      </div>

      <p-table
        #dt
        [value]="prices()"
        [rows]="10"
        [paginator]="true"
        [loading]="loading()"
        responsiveLayout="scroll"
        [globalFilterFields]="['productSku', 'customerName', 'price', 'validFrom']"
        [rowHover]="true"
        dataKey="id"
      >
        <ng-template pTemplate="header">
          <tr>
            <th pSortableColumn="customerName">
              Customer <p-sortIcon field="customerName"></p-sortIcon>
            </th>
            <th pSortableColumn="productSku">
              Product No <p-sortIcon field="productSku"></p-sortIcon>
            </th>
            <th pSortableColumn="price">Price <p-sortIcon field="price"></p-sortIcon></th>
            <th pSortableColumn="validFrom">Date <p-sortIcon field="validFrom"></p-sortIcon></th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-price>
          <tr>
            <td>{{ price.customerName }}</td>
            <td>{{ price.productSku }}</td>
            <td>{{ price.price | number: '1.2-2' }}</td>
            <td>{{ price.validFrom | date: 'dd/MM/yyyy' }}</td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage">
          <tr>
            <td colspan="4" class="text-center p-4">No prices found.</td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesPriceList implements OnInit {
  private salesPriceService = inject(SalesPriceService);
  private messageService = inject(MessageService);

  // Signals
  prices = signal<ProductSalesPriceDto[]>([]);
  loading = signal(false);

  ngOnInit() {
    this.loadPrices();
  }

  loadPrices() {
    this.loading.set(true);
    this.salesPriceService
      .getPrices()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.prices.set(res.data ?? []);
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: res.error || 'Failed to load prices.',
            });
          }
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load prices.',
          });
        },
      });
  }
}
