import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { TagModule } from 'primeng/tag';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { SelectModule } from 'primeng/select';
import { MessageService } from 'primeng/api';
import {
  PurchasePriceService,
  ProductPurchasePriceDto,
} from '../../core/services/purchase-price.service';
import { CustomerService } from '../../core/services/customer.service';
import { ProductService } from '../../core/services/product.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-purchase-price-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    FormsModule,
    TagModule,
    IconFieldModule,
    InputIconModule,
    SelectModule,
  ],
  providers: [MessageService],
  template: `
    <div class="card">
      <div class="flex items-center justify-between mb-4">
        <div class="font-semibold text-xl">Purchase Price List</div>
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
        [globalFilterFields]="['productSku', 'vendorName', 'price', 'validFrom']"
        [rowHover]="true"
        dataKey="id"
      >
        <ng-template pTemplate="header">
          <tr>
            <th pSortableColumn="vendorName">
              Vendor <p-sortIcon field="vendorName"></p-sortIcon>
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
            <td>{{ price.vendorName }}</td>
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
export class PurchasePriceList implements OnInit {
  private purchasePriceService = inject(PurchasePriceService);
  private messageService = inject(MessageService);

  // Signals
  prices = signal<ProductPurchasePriceDto[]>([]);
  loading = signal(false);

  ngOnInit() {
    this.loadPrices();
  }

  loadPrices() {
    this.loading.set(true);
    this.purchasePriceService
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
