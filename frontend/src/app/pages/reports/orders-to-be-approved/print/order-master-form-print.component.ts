import { Component, Input, ChangeDetectionStrategy, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderReportItem, OrderDetailItem } from '../../../../core/models/orders-report.models';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-order-master-form-print',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="master-form-container" id="master-form-print">
      <!-- Header -->
      <div class="form-header">
        <div class="logo-left">
          <!-- CWI Logosu -->
          <img src="assets/images/Logo.png" alt="Project Logo" />
        </div>
        <div class="title-container">
          <h1 class="master-title">MASTER FORM</h1>
        </div>
        <div class="logo-right">
          <!-- Dinamik Polo Logosu -->
          @if (poloLogoUrl()) {
            <img [src]="poloLogoUrl()" [alt]="order?.brand || 'Polo Logo'" />
          } @else {
            <div class="brand-name-fallback">{{ order?.brand }}</div>
          }
        </div>
      </div>

      <!-- Table -->
      <table class="master-table">
        <thead>
          <tr>
            <th class="col-image">Image</th>
            <th class="col-details">Product Code</th>
            <th class="col-qty">QTY (PCS)</th>
            <th class="col-amount">Amount</th>
          </tr>
        </thead>
        <tbody>
          @for (item of details; track item.productCode) {
            <tr>
              <td class="image-cell">
                <div class="img-wrapper">
                  <img
                    [src]="environment.cdnUrl + '/ProductImages/' + item.productCode + '.jpg'"
                    (error)="$any($event.target).src = 'assets/images/no-image.png'"
                    alt="Product Image"
                  />
                </div>
              </td>
              <td class="details-cell">
                <div class="product-code-header">{{ item.productCode }}</div>
                <div class="attributes-grid">
                  @for (attr of parsedAttributes(item.attributes); track attr.key) {
                    <div class="attribute-item">
                      <span class="attr-value">{{ attr.value }}</span>
                    </div>
                  }
                </div>
              </td>
              <td class="qty-cell">{{ item.qty }}</td>
              <td class="amount-cell">{{ item.amount | currency: 'USD' : 'symbol' : '1.2-2' }}</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [
    `
      .master-form-container {
        padding: 20mm;
        background: white;
        color: #333;
        font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
      }

      .form-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 30px;
        padding-bottom: 20px;
      }

      .logo-left img {
        max-height: 60px;
        width: auto;
      }

      .logo-right img {
        max-height: 70px;
        width: auto;
      }

      .brand-name-fallback {
        font-weight: bold;
        font-size: 14px;
        text-transform: uppercase;
      }

      .title-container {
        text-align: center;
        flex: 1;
      }

      .master-title {
        font-size: 32px;
        font-weight: 400;
        color: #666;
        letter-spacing: 4px;
        margin: 0;
        text-transform: uppercase;
      }

      .master-table {
        width: 100%;
        border-collapse: collapse;
        table-layout: fixed;
      }

      .master-table th {
        background-color: #f0f0f0;
        border-top: 1px solid #ccc;
        border-bottom: 1px solid #ccc;
        padding: 12px 8px;
        text-align: left;
        color: #555;
        font-size: 14px;
        font-weight: bold;
      }

      .col-image {
        width: 15%;
      }
      .col-details {
        width: 60%;
      }
      .col-qty {
        width: 12%;
        text-align: center !important;
      }
      .col-amount {
        width: 13%;
        text-align: right !important;
      }

      .master-table td {
        border-bottom: 1px solid #eee;
        padding: 15px 8px;
        vertical-align: top;
        page-break-inside: avoid;
      }

      .image-cell {
        text-align: center;
      }

      .img-wrapper {
        width: 100%;
        height: 100px;
        display: flex;
        align-items: center;
        justify-content: center;
      }

      .img-wrapper img {
        max-width: 100%;
        max-height: 100%;
        object-fit: contain;
      }

      .product-code-header {
        font-weight: bold;
        color: #444;
        margin-bottom: 10px;
        font-size: 15px;
      }

      .attributes-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 4px 20px;
        color: #777;
        font-size: 11px;
      }

      .attribute-item {
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }

      .attr-label {
        font-weight: bold;
      }

      .qty-cell {
        text-align: center;
        font-size: 15px;
        color: #444;
      }

      .amount-cell {
        text-align: right;
        font-size: 15px;
        color: #444;
      }

      @media print {
        .master-form-container {
          padding: 10mm;
          background: white !important;
        }
        .master-table th {
          background-color: #f0f0f0 !important;
          -webkit-print-color-adjust: exact;
          print-color-adjust: exact;
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderMasterFormPrintComponent {
  @Input() order: OrderReportItem | null = null;
  @Input() details: OrderDetailItem[] = [];
  @Input() project: 'CWI' | 'AWC' = 'CWI';

  protected readonly environment = environment;

  poloLogoUrl = computed(() => {
    const brand = this.order?.brand || '';
    // Marka ismine göre logo belirleme (Asset'ler hazır olduğunda güncellenebilir)
    if (brand.toUpperCase().includes('BEVERLY HILLS POLO CLUB')) {
      if (brand.toUpperCase().endsWith('Y')) {
        return 'assets/images/polo-logo-y.png';
      }
      return 'assets/images/polo-logo.png';
    }
    return null;
  });

  parsedAttributes(attrString: string | undefined): { key: string; value: string }[] {
    if (!attrString) return [];
    try {
      const attrs = JSON.parse(attrString);
      // Görseldeki standart alanları önceliklendirelim
      const standardOrder = [
        'Gender',
        'Dial Colour',
        'Movement Function',
        'Glass Material',
        'Case Material',
        'Case Size',
        'Strap/Bracelet Material',
        'Case Color',
        'Strap/Bracelet Colour',
        'Movement Type',
        'Atm',
      ];

      const result: { key: string; value: string }[] = [];

      standardOrder.forEach((key) => {
        if (attrs[key]) {
          result.push({ key, value: attrs[key] });
        }
      });

      // Diğer alanları da ekleyelim (eğer varsa)
      Object.keys(attrs).forEach((key) => {
        if (!standardOrder.includes(key)) {
          result.push({ key, value: attrs[key] });
        }
      });

      return result;
    } catch {
      return [];
    }
  }
}
