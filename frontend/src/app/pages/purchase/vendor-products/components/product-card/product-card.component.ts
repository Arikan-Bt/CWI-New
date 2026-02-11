import { Component, input, signal, computed, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { InputNumberModule } from 'primeng/inputnumber';
import { ProductDto } from '../../../../../core/models/product.models';
import { environment } from '../../../../../../environments/environment';
import { CartService } from 'src/app/core/services/cart.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    TagModule,
    CurrencyPipe,
    InputNumberModule,
    FormsModule
  ],
  template: `
    <div class="group bg-white dark:bg-surface-900 border border-surface-200 dark:border-surface-700 rounded-2xl overflow-hidden hover:shadow-xl dark:hover:shadow-primary-900/10 transition-all duration-300 h-full flex flex-col relative z-0">
      
      <!-- Stok Durumu -->
      <div class="absolute top-3 right-3 z-10">
        @if (product().isInStock) {
          <p-tag value="In Stock" severity="success" [rounded]="true" styleClass="px-2 py-1 text-[10px] font-bold shadow-sm"></p-tag>
        } @else {
          <p-tag value="Out of Stock" severity="danger" [rounded]="true" styleClass="px-2 py-1 text-[10px] font-bold shadow-sm"></p-tag>
        }
      </div>

      <!-- Görsel Alanı -->
      <div class="relative aspect-[4/5] bg-surface-50 dark:bg-surface-950/50 flex items-center justify-center p-6 overflow-hidden group-hover:bg-surface-100 dark:group-hover:bg-surface-900/50 transition-colors duration-300">
        @if (imageError()) {
          <div class="flex flex-col items-center justify-center text-surface-400 dark:text-surface-600 gap-2">
            <i class="pi pi-image text-4xl opacity-50"></i>
            <span class="text-[10px] font-bold uppercase tracking-widest opacity-70">No Image</span>
          </div>
        } @else {
          <img 
            [src]="imageUrl()" 
            [alt]="product().name"
            (error)="onImageError()"
            class="w-full h-full object-contain mix-blend-multiply dark:mix-blend-normal transition-transform duration-500 group-hover:scale-110" 
            loading="lazy"
          />
        }
      </div>

      <!-- İçerik -->
      <div class="p-4 flex flex-col flex-1 gap-3 relative bg-white dark:bg-surface-900">
        <div class="flex-1">
          <h3 class="font-bold text-surface-900 dark:text-surface-50 text-sm leading-tight mb-1 line-clamp-2 min-h-[2.5rem]" [title]="product().name">
            {{ product().name }}
          </h3>
          <span class="text-[10px] font-bold text-surface-400 dark:text-surface-500 uppercase tracking-widest block mb-1">
            {{ product().brandName }}
          </span>
          <p class="text-[10px] font-medium text-surface-500 dark:text-surface-400 flex items-center gap-1">
            <i class="pi pi-barcode text-[9px]"></i>
            {{ product().sku }}
          </p>
        </div>

        <div class="space-y-2 pt-3 border-t border-surface-100 dark:border-surface-800">
          <div class="flex justify-between items-baseline">
            <span class="text-[10px] uppercase font-bold text-surface-400 dark:text-surface-500 tracking-wider">Price</span>
            <span class="text-base font-bold text-primary-600 dark:text-primary-400 font-mono">
              {{ product().purchasePrice | currency:'USD':'symbol':'1.2-2' }}
            </span>
          </div>
        </div>

        <!-- Buton ve Adet -->
        <!-- Buton ve Adet -->
        <div class="flex flex-col gap-2 pt-2">
            @if (product().isInStock) {
              <div (click)="$event.stopPropagation()" class="w-full">
                  <p-inputNumber 
                      [(ngModel)]="quantity" 
                      [showButtons]="true" 
                      buttonLayout="horizontal" 
                      inputId="horizontal" 
                      spinnerMode="horizontal" 
                      [step]="1"
                      [min]="1" 
                      [max]="product().stockCount"
                      [inputStyleClass]="'w-full text-center p-0 text-sm h-9 border-none bg-transparent font-medium'"
                      decrementButtonClass="p-button-text p-button-secondary w-9 h-9 !rounded-none !bg-transparent hover:!bg-surface-100 dark:hover:!bg-surface-800"
                      incrementButtonClass="p-button-text p-button-secondary w-9 h-9 !rounded-none !bg-transparent hover:!bg-surface-100 dark:hover:!bg-surface-800"
                      incrementButtonIcon="pi pi-plus text-xs" 
                      decrementButtonIcon="pi pi-minus text-xs"
                      styleClass="w-full border border-surface-200 dark:border-surface-700 rounded-lg overflow-hidden bg-surface-50 dark:bg-surface-900"
                      [allowEmpty]="false">
                  </p-inputNumber>
              </div>
            }
            <button 
                pButton 
                pRipple
                [label]="product().isInStock ? 'Add to Cart' : 'Out of Stock'" 
                [icon]="product().isInStock ? 'pi pi-shopping-cart' : 'pi pi-bell'" 
                [outlined]="!product().isInStock"
                [severity]="product().isInStock ? 'primary' : 'secondary'"
                [disabled]="!product().isInStock"
                class="w-full p-button-sm justify-center text-xs h-9"
                class="w-full p-button-sm justify-center text-xs h-9"
                (click)="addToCart($event)"
            ></button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
    :host ::ng-deep .p-inputnumber-input {
        width: 100%;
        padding: 0.5rem 0.25rem;
        font-size: 0.875rem;
        text-align: center;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductCardComponent {
  product = input.required<ProductDto>();
  cartService = inject(CartService);
  messageService = inject(MessageService);
  
  imageError = signal(false);
  quantity = signal(1);

  imageUrl = computed(() => {
    // CDN url construction
    // https://cdn.arikantime.com/ProductImages/{i.Product.Sku}.jpg
    // environment.cdnUrl already contains https://cdn.arikantime.com
    const cdn = environment.cdnUrl || '';
    const sku = this.product().sku;
    return `${cdn}/ProductImages/${sku}.jpg`;
  });

  onImageError() {
    this.imageError.set(true);
  }

  addToCart(event: Event) {
    event.stopPropagation();
    this.cartService.addToCart(this.product(), this.quantity());
    this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: `${this.product().name} added to cart`
    });
    this.quantity.set(1); // Reset quantity
  }
}
