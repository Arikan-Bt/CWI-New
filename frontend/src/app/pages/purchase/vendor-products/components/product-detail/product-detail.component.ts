import { Component, input, inject, signal, effect, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, CurrencyPipe, KeyValuePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../../../core/services/product.service';
import { ProductDetailDto } from '../../../../../core/models/product.models';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { GalleriaModule } from 'primeng/galleria';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { InputNumberModule } from 'primeng/inputnumber';
import { environment } from '../../../../../../environments/environment';
import { CartService } from 'src/app/core/services/cart.service';
import { MessageService } from 'primeng/api';
import { ProductDto } from '../../../../../core/models/product.models';
@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [
    CommonModule,
    TagModule,
    ButtonModule,
    ProgressSpinnerModule,
    GalleriaModule,
    ScrollPanelModule,
    CurrencyPipe,
    KeyValuePipe,
    InputNumberModule,
    FormsModule
  ],
  template: `
    <div class="product-detail-container h-full flex flex-col">
      @if (loading()) {
        <div class="flex-1 flex items-center justify-center min-h-[400px]">
          <p-progressspinner ariaLabel="loading"></p-progressspinner>
        </div>
      } @else if (product(); as p) {
        <div class="flex flex-col lg:flex-row gap-8 h-full">
          <!-- Sol Taraf: Görseller -->
          <div class="w-full lg:w-1/2 flex flex-col gap-4">
             <div class="bg-surface-50 dark:bg-surface-900 rounded-2xl p-6 border border-surface-200 dark:border-surface-700 aspect-square flex items-center justify-center relative overflow-hidden">
                @if (imageError()) {
                  <div class="flex flex-col items-center justify-center text-surface-400 dark:text-surface-600 gap-2">
                    <i class="pi pi-image text-4xl opacity-50"></i>
                    <span class="text-[10px] font-bold uppercase tracking-widest opacity-70">No Image</span>
                  </div>
                } @else {
                  <img 
                    [src]="selectedImage() || (environment.cdnUrl + '/ProductImages/' + p.sku + '.jpg')" 
                    [alt]="p.name"
                    (error)="onImageError()"
                    class="w-full h-full object-contain mix-blend-multiply dark:mix-blend-normal hover:scale-105 transition-transform duration-500"
                  />
                }
             </div>
             <!-- Thumbnails -->
             @if (p.images && p.images.length > 1) {
               <div class="flex gap-2 overflow-x-auto py-2 custom-scrollbar">
                  @for (img of p.images; track img) {
                    <div 
                      class="w-20 h-20 rounded-lg border-2 cursor-pointer p-1 bg-white dark:bg-surface-900 shrink-0"
                      [class.border-primary-500]="selectedImage() === img"
                      [class.border-transparent]="selectedImage() !== img"
                      (click)="selectedImage.set(img)"
                    >
                      <img [src]="img" class="w-full h-full object-contain" />
                    </div>
                  }
               </div>
             }
          </div>

          <!-- Sağ Taraf: Detaylar -->
          <div class="w-full lg:w-1/2 flex flex-col gap-6">
            <div>
              <div class="flex flex-col gap-1 mb-3">
                 <span class="text-sm font-bold text-primary-600 dark:text-primary-400 uppercase tracking-widest">
                   {{ p.brandName }}
                 </span>
                 <div class="flex items-center gap-2 text-surface-500 dark:text-surface-400">
                    <i class="pi pi-barcode"></i>
                    <span class="text-xs font-semibold font-mono">{{ p.sku }}</span>
                 </div>
              </div>
              
              <h1 class="text-2xl md:text-3xl font-bold text-surface-900 dark:text-surface-50 leading-tight mb-4">
                {{ p.name }}
              </h1>

              <div class="flex items-center gap-4 mb-6">
                <div class="text-3xl font-bold text-surface-900 dark:text-surface-0 font-mono">
                  {{ p.purchasePrice | currency:'USD':'symbol':'1.2-2' }}
                </div>
                
                @if (p.isInStock) {
                   <p-tag severity="success" value="In Stock" [rounded]="true" styleClass="px-3"></p-tag>
                } @else {
                   <p-tag severity="danger" value="Out of Stock" [rounded]="true" styleClass="px-3"></p-tag>
                }
              </div>

              <div class="prose dark:prose-invert max-w-none text-surface-600 dark:text-surface-300 text-sm leading-relaxed mb-6">
                {{ p.description }}
              </div>

              <!-- Özellikler Grid -->
               <!-- Özellikler Grid (Sadece Stok) -->
               <div class="grid grid-cols-2 gap-4 mb-4">
                 <div class="bg-surface-50 dark:bg-surface-800/50 p-3 rounded-xl flex items-center gap-3">
                    <div class="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-900/30 flex items-center justify-center text-primary-600 dark:text-primary-400">
                       <i class="pi pi-box"></i>
                    </div>
                    <div>
                      <span class="text-[10px] text-surface-500 uppercase font-bold block mb-0.5">Stock</span>
                      <span class="font-semibold text-sm text-surface-900 dark:text-surface-100">{{ p.stockCount }} Units</span>
                    </div>
                 </div>

                 <!-- Dinamik Attributes -->
                 @if (p.attributes) {
                    @for (attr of p.attributes | keyvalue; track attr.key) {
                       <div class="bg-surface-50 dark:bg-surface-800/50 p-3 rounded-xl flex items-center gap-3">
                          <div class="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-900/30 flex items-center justify-center text-primary-600 dark:text-primary-400">
                             <i class="pi pi-verified"></i>
                          </div>
                          <div>
                            <span class="font-semibold text-sm text-surface-900 dark:text-surface-100">{{ attr.value }}</span>
                          </div>
                       </div>
                    }
                 }
               </div>


            </div>

            <!-- Butonlar -->
            <div class="mt-auto flex gap-3 pt-6">
               @if (p.isInStock) {
                 <p-inputNumber 
                    [(ngModel)]="quantity" 
                    [showButtons]="true" 
                    buttonLayout="horizontal" 
                    inputId="horizontal"
                    spinnerMode="horizontal"
                    [step]="1"
                    [min]="1" 
                    [max]="p.stockCount"
                    decrementButtonClass="p-button-secondary"
                    incrementButtonClass="p-button-secondary" 
                    incrementButtonIcon="pi pi-plus" 
                    decrementButtonIcon="pi pi-minus" 
                    [inputStyleClass]="'w-16 text-center font-bold'"
                    [allowEmpty]="false">
                 </p-inputNumber>
               }
               <button 
                  pButton 
                  label="Add to Cart" 
                  icon="pi pi-shopping-cart" 
                  class="flex-1 p-button-lg"
                  [disabled]="!p.isInStock"
                  (click)="addToCart()"
                ></button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .custom-scrollbar::-webkit-scrollbar {
      height: 4px;
    }
    .custom-scrollbar::-webkit-scrollbar-track {
      background: transparent;
    }
    .custom-scrollbar::-webkit-scrollbar-thumb {
      background: var(--surface-300);
      border-radius: 4px;
    }
    :host ::ng-deep .p-inputnumber-input {
      text-align: center;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductDetailComponent {
  productId = input.required<number>();
  
  private productService = inject(ProductService);
  private cartService = inject(CartService);
  private messageService = inject(MessageService);
  
  product = signal<ProductDetailDto | null>(null);
  loading = signal(false);
  selectedImage = signal<string | null>(null);
  imageError = signal(false);
  quantity = signal(1);

  protected readonly environment = environment;

  constructor() {
    effect(() => {
      const id = this.productId();
      if (id) {
        this.loadProduct(id);
      }
    }, { allowSignalWrites: true });
  }

  loadProduct(id: number) {
    this.loading.set(true);
    this.imageError.set(false); // Reset error state on new load
    this.productService.getProductDetail(id).subscribe({
      next: (data) => {
        this.product.set(data);
        // Varsayılan görseli ayarla (varsa ilk görsel, yoksa null)
        if (data.images && data.images.length > 0) {
            this.selectedImage.set(data.images[0]);
        } else {
            // Görsel yoksa SKU bazlı CDN linkini varsayılan olarak kullanmak için null bırakıyoruz, template halledecek
            this.selectedImage.set(null);
        }
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onImageError() {
    this.imageError.set(true);
  }

  addToCart() {
      const p = this.product();
      if (!p) return;

      const productDto: ProductDto = {
          id: p.id,
          sku: p.sku,
          name: p.name,
          brandName: p.brandName,
          purchasePrice: p.purchasePrice,
          imageUrl: p.images?.[0] || null,
          isInStock: p.isInStock,
          stockCount: p.stockCount
      };

      this.cartService.addToCart(productDto, this.quantity());
      this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `${p.name} added to cart`
      });
      // Optionally close dialog or reset quantity
      this.quantity.set(1);
  }
}
