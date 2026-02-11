import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderReportItem, OrderDetailItem } from '../../../../core/models/orders-report.models';

@Component({
  selector: 'app-order-report-print',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="border-2 border-black">
      <!-- Title Row -->
      <div class="flex justify-between items-center p-2 border-b-2 border-black bg-white">
        <div class="font-bold text-gray-700 uppercase">SALES ORDERS</div>
        <div class="font-bold text-red-600 uppercase">
          {{ order?.orderDetails }}
        </div>
      </div>

      <div class="flex justify-between items-center p-3 border-b-2 border-black">
        <div class="text-xl font-bold text-gray-600">
          {{ order?.currentAccountDescription }}
        </div>
        <div class="text-sm font-semibold text-gray-600">
          Order Date : {{ order?.orderDate | date: 'dd.MM.yyyy' }}
        </div>
      </div>

      <!-- Manually Constructed Data Table -->
      <table class="w-full text-sm text-left">
        <thead>
          <tr class="text-gray-500 font-bold border-b-2 border-black">
            @if (showPhotos) {
              <th class="p-2 border-r border-black w-20">Picture</th>
            }
            <th class="p-2 border-r border-black w-48">Product Code</th>
            <th class="p-2 border-r border-black">Product Name</th>
            <th class="p-2 border-r border-black w-24 text-right">QTY (PCS)</th>
            <th class="p-2 border-r border-black w-24 text-right">Amount</th>
            <th class="p-2 w-24 text-right">Total</th>
          </tr>
        </thead>
        <tbody class="text-gray-700 font-semibold">
          @for (item of details; track item.productCode) {
            <tr class="border-b border-black">
              @if (showPhotos) {
                <td class="p-1 border-r border-black text-center">
                  @if (item.picture) {
                    <img [src]="item.picture" class="w-10 h-10 object-contain mx-auto" />
                  }
                </td>
              }
              <td class="p-2 border-r border-black">{{ item.productCode }}</td>
              <td class="p-2 border-r border-black">{{ item.productName }}</td>
              <td class="p-2 border-r border-black text-right">
                {{ item.qty | number }}
              </td>
              <td class="p-2 border-r border-black text-right">
                {{ item.amount | currency: 'USD' : 'symbol' : '1.2-2' }}
              </td>
              <td class="p-2 text-right">
                {{ item.total | currency: 'USD' : 'symbol' : '1.2-2' }}
              </td>
            </tr>
          }
        </tbody>
      </table>

      <!-- Footer Info Section -->
      <div class="flex border-t-2 border-black">
        <!-- Left Details -->
        <div class="flex-1 p-4 border-r-2 border-black">
          <div class="mb-6">
            <div class="font-bold text-gray-700 mb-2">Order Description</div>
            <div></div>
          </div>

          <div class="mb-1 text-sm">
            <span class="font-bold text-gray-600">Address : </span>
            <span class="text-gray-500">Av. 14 Janvier Slim Center Khezama Est</span>
          </div>
          <div class="text-sm">
            <span class="font-bold text-gray-600">Payment Type : </span>
          </div>
        </div>

        <!-- Right Totals -->
        <div class="w-80">
          <div class="flex justify-between items-center p-2 border-b border-black">
            <span class="text-sm font-semibold text-gray-600">Total Qty</span>
            <span class="font-bold text-gray-800">{{ order?.totalQty | number }}</span>
          </div>
          <div class="flex justify-between items-center p-2 border-b border-black">
            <span class="text-sm font-semibold text-gray-600">Grand Total</span>
            <div class="flex items-center gap-1 font-bold text-gray-800">
              {{ order?.total | currency: 'USD' : 'symbol' : '1.2-2' }}
            </div>
          </div>
          <div class="flex justify-between items-center p-2 border-b-2 border-black">
            <span class="text-sm font-semibold text-gray-600">Discount</span>
            <div class="flex items-center gap-1 font-bold text-gray-800">
              {{ 0 | currency: 'USD' : 'symbol' : '1.2-2' }}
            </div>
          </div>
          <div class="flex justify-between items-center p-2 bg-gray-50">
            <span class="text-sm font-bold text-gray-900">Total Amount</span>
            <div class="flex items-center gap-1 text-lg font-black text-gray-900">
              {{ order?.total | currency: 'USD' : 'symbol' : '1.2-2' }}
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderReportPrintComponent {
  @Input() order: OrderReportItem | null = null;
  @Input() details: OrderDetailItem[] = [];
  @Input() showPhotos = false;

  today = new Date();
}
