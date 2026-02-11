import {
  Component,
  inject,
  OnInit,
  signal,
  ChangeDetectionStrategy,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { SelectButtonModule } from 'primeng/selectbutton';
import { DrawerModule } from 'primeng/drawer';
import { ProductService } from '../../../core/services/product.service';
import { ProductDto, BrandDto, ProductFilter } from '../../../core/models/product.models';
import { finalize } from 'rxjs';
import { FluidModule } from 'primeng/fluid';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { PaginatorModule } from 'primeng/paginator';

import { DialogModule } from 'primeng/dialog';
import { ProductCardComponent } from './components/product-card/product-card.component';
import { ProductDetailComponent } from './components/product-detail/product-detail.component';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-vendor-products',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    CardModule,
    CheckboxModule,
    TagModule,
    IconFieldModule,
    InputIconModule,
    SelectButtonModule,
    DrawerModule,
    FluidModule,
    ProgressSpinnerModule,
    PaginatorModule,

    DialogModule,
    ProductCardComponent,
    ProductDetailComponent
  ],
  template: `
    <div class="vendor-products-page relative z-0">
      <div class="flex flex-col gap-4">
        <!-- Filtre/Başlık Kartı -->
        <div class="card">
          <div class="font-semibold text-xl mb-4">Vendor Products</div>

          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-4 gap-6 items-end">
              <!-- Arama -->
              <div class="flex flex-col gap-2">
                <label for="search" class="font-medium text-sm text-surface-600"
                  >Search Products</label
                >
                <p-iconfield iconPosition="left">
                  <p-inputicon class="pi pi-search"></p-inputicon>
                  <input
                    pInputText
                    id="search"
                    type="text"
                    [(ngModel)]="searchTerm"
                    (input)="onSearch()"
                    placeholder="Search by name, code..."
                  />
                </p-iconfield>
              </div>

              <!-- Düzen Değiştirici -->
              <div class="flex flex-col gap-2">
                <label class="font-medium text-sm text-surface-600">View Layout</label>
                <p-selectButton [options]="layoutOptions" [(ngModel)]="layout" [allowEmpty]="false">
                  <ng-template pTemplate="item" let-item>
                    <i [class]="item.icon"></i>
                  </ng-template>
                </p-selectButton>
              </div>

              <!-- Boşluk -->
              <div class="hidden md:block"></div>

              <!-- İşlemler -->
              <div class="flex justify-end gap-2">
                <p-button
                  label="Filters"
                  icon="pi pi-filter"
                  [outlined]="!isFilterActive()"
                  [severity]="isFilterActive() ? 'primary' : 'secondary'"
                  (onClick)="visibleSidebar.set(true)"
                  badgeClass="bg-red-500"
                  [badge]="activeFilterCount() > 0 ? activeFilterCount().toString() : undefined"
                ></p-button>
              </div>
            </div>

            <div class="mt-4 text-sm text-gray-500 dark:text-muted-color text-right">
              Showing {{ products().length }} of {{ totalCount() }} products
            </div>
          </p-fluid>
        </div>

        <!-- Veri Kartı -->
        <div class="card p-0 relative">
          @if (loading()) {
          <div class="absolute inset-0 flex items-center justify-center bg-white/50 z-10">
            <p-progressspinner styleClass="w-12 h-12" strokeWidth="4"></p-progressspinner>
          </div>
          }

          <!-- Manual Data Display -->
          @if (layout() === 'grid') {
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-6 p-6">
              @for (product of products(); track product.id) {

                <app-product-card [product]="product" (click)="openProductDetail(product.id)" class="cursor-pointer"></app-product-card>
              }

            </div>
            } @else {
            <div class="flex flex-col divide-y divide-gray-100 dark:divide-surface-800">
              @for (product of products(); track product.id) {

                <div class="flex items-center gap-6 p-4 hover:bg-gray-50 dark:hover:bg-surface-800/50 transition-colors cursor-pointer" (click)="openProductDetail(product.id)">
                  <div class="w-24 h-24 bg-gray-50 dark:bg-surface-950 rounded-lg flex items-center justify-center overflow-hidden border border-gray-100 dark:border-surface-800 shrink-0">
                    <img [src]="environment.cdnUrl + '/ProductImages/' + product.sku + '.jpg'" class="max-w-full max-h-full object-contain" />
                  </div>
                  <div class="flex-1 min-w-0">
                    <span class="text-[10px] font-bold text-gray-400 uppercase tracking-widest">{{ product.brandName }}</span>
                    <h3 class="font-bold text-gray-800 dark:text-color text-lg truncate">
                      {{ product.name }}
                    </h3>
                    <p class="text-xs text-gray-500">Code: {{ product.sku }}</p>
                    <div class="mt-2">
                      <p-tag [value]="product.isInStock ? 'In Stock' : 'Out Of Stock'" [severity]="product.isInStock ? 'success' : 'danger'" [rounded]="true" styleClass="px-2 py-0.5 text-[10px]"></p-tag>
                    </div>
                  </div>
                  <div class="w-48 flex flex-col gap-1 items-end shrink-0">
                      <span class="text-xs text-gray-500 uppercase font-semibold">Price</span>
                      <span class="font-bold text-primary-600 text-lg">{{ product.purchasePrice | currency : 'USD' : 'symbol' : '1.2-2' }}</span>
                  </div>
                  <div class="flex gap-2 shrink-0">
                    @if (product.isInStock) {
                    <p-button icon="pi pi-shopping-cart" label="Add" size="small"></p-button>
                    } @else {
                    <p-button icon="pi pi-times-circle" label="Out" size="small" [disabled]="true" [text]="true" severity="danger"></p-button>
                    }
                  </div>
                </div>
                }
            </div>
          }

          <!-- Paginator -->
          <p-paginator
            (onPageChange)="onPageChange($event)"
            [first]="(products().length > 0) ? (products()[0].id && 0) : 0" 
            [rows]="20" 
            [totalRecords]="totalCount()" 
            [showCurrentPageReport]="false">
          </p-paginator>
        </div>
      </div>

      <!-- Filtre Drawer -->
      <p-drawer
        [(visible)]="visibleSidebar"
        position="right"
        [baseZIndex]="10000"
        appendTo="body"
        styleClass="w-full md:w-[24rem]"
      >
        <ng-template pTemplate="header">
          <div class="flex items-center gap-2 font-bold text-xl text-gray-800 dark:text-color">
            <i class="pi pi-filter text-primary-500"></i>
            <span>Filters</span>
          </div>
        </ng-template>

        <div class="flex flex-col gap-8 h-full">
          <div class="flex-1 overflow-y-auto pr-2">
            <!-- Stok Durumu Filtresi -->
            <div class="mb-6">
              <div
                class="text-xs font-black text-gray-400 uppercase tracking-widest mb-4 flex items-center gap-2"
              >
                <i class="pi pi-box"></i>
                <span>STOCK STATUS</span>
              </div>
              <div
                class="bg-gray-50 dark:bg-surface-800/30 rounded-xl p-4 border border-gray-100 dark:border-surface-700"
              >
                <div class="flex items-center gap-3">
                  <p-checkbox
                    [binary]="true"
                    [(ngModel)]="onlyInStock"
                    (onChange)="onFilterChange()"
                  ></p-checkbox>
                  <span class="text-sm font-medium text-gray-600 dark:text-surface-300"
                    >Only In Stock</span
                  >
                </div>
              </div>
            </div>

            <!-- Markalar -->
            <div>
              <div
                class="text-xs font-black text-gray-400 uppercase tracking-widest mb-4 flex items-center gap-2"
              >
                <i class="pi pi-tag"></i>
                <span>BRANDS</span>
              </div>

              <!-- Hızlı Arama -->
              <div class="mb-3">
                <p-iconfield iconPosition="left">
                  <p-inputicon class="pi pi-search text-xs"></p-inputicon>
                  <input
                    pInputText
                    type="text"
                    [(ngModel)]="brandSearch"
                    placeholder="Find a brand..."
                    class="p-inputtext-sm w-full bg-gray-50 dark:bg-surface-800/30 border-gray-200 dark:border-surface-700"
                  />
                </p-iconfield>
              </div>

              <div class="flex flex-col gap-2 max-h-[500px] overflow-y-auto custom-scrollbar">
                @for (brand of filteredBrands(); track brand.id) {
                <div
                  class="flex items-center gap-3 group cursor-pointer p-2 hover:bg-gray-50 dark:hover:bg-surface-800 rounded-lg transition-colors"
                  (click)="toggleBrand(brand.id)"
                >
                  <p-checkbox
                    [binary]="true"
                    [ngModel]="selectedBrands().includes(brand.id)"
                    (click)="$event.stopPropagation()"
                    (onChange)="onFilterChange()"
                  ></p-checkbox>
                  <span
                    class="text-sm font-medium text-gray-600 dark:text-surface-300 group-hover:text-primary-600 transition-colors flex-1"
                  >
                    {{ brand.name }}
                  </span>
                  @if (selectedBrands().includes(brand.id)) {
                  <i class="pi pi-check text-primary-500 text-xs"></i>
                  }
                </div>
                }
              </div>
            </div>
          </div>

          <!-- Alt Butonlar -->
          <div
            class="mt-auto border-t border-gray-100 dark:border-surface-800 pt-6 flex flex-col gap-3"
          >
            <button
              pButton
              label="Show Results"
              icon="pi pi-check"
              class="w-full"
              (click)="visibleSidebar.set(false)"
            ></button>
            <button
              pButton
              label="Reset All Filters"
              icon="pi pi-refresh"
              [text]="true"
              class="w-full p-button-secondary text-sm"
              (click)="resetFilters()"
              [disabled]="!isFilterActive()"
            ></button>
          </div>
        </div>
      </p-drawer>


      <!-- Ürün Detay Dialog -->
      <p-dialog 
        [(visible)]="showDetailDialog" 
        [modal]="true" 
        [style]="{ width: '90vw', maxWidth: '1200px', height: '90vh' }" 
        [breakpoints]="{ '960px': '95vw' }"
        [maximizable]="false"
        [dismissableMask]="true"
        [draggable]="false"
        [resizable]="false"
        [contentStyle]="{ height: '100%' }"
        header="Product Details"
        appendTo="body"
        [baseZIndex]="20000"
        styleClass="p-0 border-0 rounded-2xl overflow-hidden"
      >
        @if (showDetailDialog() && selectedProductId()) {
           <app-product-detail [productId]="selectedProductId()!"></app-product-detail>
        }
      </p-dialog>
    </div>
  `,
  styles: [
    `
      :host ::ng-deep {
        .p-dataview-content {
          background: transparent;
        }
        .p-paginator {
          background: transparent;
          border: none;
          border-top: 1px solid var(--p-surface-border);
          padding: 1rem;
        }
        .p-selectbutton .p-button {
          padding: 0.5rem 0.75rem;
        }
        .custom-scrollbar::-webkit-scrollbar {
          width: 4px;
        }
        .custom-scrollbar::-webkit-scrollbar-track {
          background: transparent;
        }
        .custom-scrollbar::-webkit-scrollbar-thumb {
          background: var(--p-surface-300);
          border-radius: 10px;
        }

        .p-drawer {
          border-top-left-radius: 12px;
          border-bottom-left-radius: 12px;
          box-shadow: -4px 0 20px rgba(0, 0, 0, 0.05);
        }
      }
      .vendor-products-page {
        background-color: var(--surface-card);
        padding: 2rem;
        border-radius: var(--content-border-radius);
        margin-bottom: 2rem;
        position: relative;
        z-index: 0;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VendorProducts implements OnInit {
  private productService = inject(ProductService);

  // Sinyaller
  products = signal<ProductDto[]>([]);
  brands = signal<BrandDto[]>([]);
  totalCount = signal(0);
  loading = signal(false);

  visibleSidebar = signal(false);
  
  // Detay Dialog Sinyalleri
  showDetailDialog = signal(false);
  selectedProductId = signal<number | null>(null);

  // Filtre Sinyalleri
  searchTerm = signal('');
  selectedBrands = signal<number[]>([]);
  onlyInStock = signal(false);
  brandSearch = signal('');

  layout = signal<'grid' | 'list'>('grid');
  
  protected readonly environment = environment;

  layoutOptions = [
    { icon: 'pi pi-th-large', value: 'grid' },
    { icon: 'pi pi-bars', value: 'list' },
  ];

  // Hesaplanan Değerler
  filteredBrands = computed(() => {
    const term = this.brandSearch().toLowerCase();
    const allBrands = this.brands();
    if (!term) return allBrands;
    return allBrands.filter((b) => b.name.toLowerCase().includes(term));
  });

  activeFilterCount = computed(() => {
    let count = 0;
    if (this.selectedBrands().length > 0) count += 1; // Markalar tek filtre grubu sayılır
    if (this.onlyInStock()) count += 1;
    return count;
  });

  isFilterActive = computed(() => {
    return this.selectedBrands().length > 0 || this.onlyInStock();
  });

  ngOnInit() {
    // HMR trigger
    this.loadBrands();
    this.loadProducts();
  }

  loadBrands() {
    this.productService.getBrands().subscribe({
      next: (data) => this.brands.set(data),
    });
  }

  loadProducts(page: number = 1, pageSize: number = 20) {
    this.loading.set(true);

    // Stok filtresini şimdilik backend'e yansıtmıyoruz, isterseniz Query'e ekleriz
    // Ama UI'da filter logic hazır.
    // Eğer backend'de stok durumu filtresi yoksa, bunu eklemek gerekir.
    // Şu anlık sadece isActive ve brandIds gidiyor.

    const filter: ProductFilter = {
      searchTerm: this.searchTerm(),
      brandIds: this.selectedBrands(),
      pageNumber: page,
      pageSize: pageSize,
    };

    this.productService
      .getVendorProducts(filter)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => {
          this.products.set(response.data);
          this.totalCount.set(response.totalCount);
        },
      });
  }

  onSearch() {
    this.loadProducts();
  }

  onFilterChange() {
    // Otomatik yükle
    this.loadProducts();
  }

  onPageChange(event: any) {
    const page = event.first / event.rows + 1;
    this.loadProducts(page, event.rows);
  }

  toggleBrand(brandId: number) {
    const current = this.selectedBrands();
    if (current.includes(brandId)) {
      this.selectedBrands.set(current.filter((id) => id !== brandId));
    } else {
      this.selectedBrands.set([...current, brandId]);
    }
    this.onFilterChange();
  }

  resetFilters() {
    this.selectedBrands.set([]);
    this.onlyInStock.set(false);
    this.brandSearch.set('');
    this.loadProducts();
  }

  openProductDetail(id: number) {
    this.selectedProductId.set(id);
    this.showDetailDialog.set(true);
  }
}
