import { Component, OnInit, signal, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { DialogModule } from 'primeng/dialog';
import { FileUploadModule } from 'primeng/fileupload';
import { ImageModule } from 'primeng/image';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ProductService } from '../../core/services/product.service';
import { ProductDto, ProductFilter } from '../../core/models/product.models';
import { finalize } from 'rxjs/operators';
import { TooltipModule } from 'primeng/tooltip';
import { environment } from '../../../environments/environment';
import { ImageService } from '../../core/services/image.service';

@Component({
  selector: 'app-product-visual-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    ToastModule,
    ConfirmDialogModule,
    IconFieldModule,
    InputIconModule,
    DialogModule,
    FileUploadModule,
    ImageModule,
    TooltipModule,
  ],
  providers: [MessageService, ConfirmationService],
  template: `
    <div class="card">
      <div class="flex items-center justify-between mb-4">
        <div class="font-semibold text-xl">Product Visual Management</div>
        <div class="flex gap-2">
          <p-iconfield iconPosition="left">
            <p-inputicon class="pi pi-search"></p-inputicon>
            <input
              pInputText
              type="text"
              (input)="onSearch($event)"
              placeholder="Search Products..."
              class="w-full"
            />
          </p-iconfield>
        </div>
      </div>

      <p-confirmDialog></p-confirmDialog>
      <p-toast></p-toast>

      <p-table
        #dt
        [value]="products()"
        [lazy]="true"
        (onLazyLoad)="loadProducts($event)"
        [paginator]="true"
        [rows]="10"
        [totalRecords]="totalRecords()"
        [loading]="loading()"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
        responsiveLayout="scroll"
        styleClass="p-datatable-sm"
        [globalFilterFields]="['sku', 'name', 'brandName']"
      >
        <ng-template pTemplate="header">
          <tr>
            <th style="width: 15%">Image</th>
            <th pSortableColumn="sku">SKU <p-sortIcon field="sku"></p-sortIcon></th>
            <th pSortableColumn="name">Product Name <p-sortIcon field="name"></p-sortIcon></th>
            <th pSortableColumn="brandName">Brand <p-sortIcon field="brandName"></p-sortIcon></th>
            <th style="width: 10rem" class="text-center">Actions</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-product>
          <tr>
            <td>
              @if (getImageUrl(product)) {
                <p-image
                  [src]="getImageUrl(product)"
                  width="50"
                  [preview]="true"
                  (onImageError)="onImageError(product)"
                ></p-image>
              } @else {
                <div
                  class="w-[50px] h-[50px] bg-surface-50 dark:bg-surface-800 flex flex-col items-center justify-center rounded border border-surface-200 dark:border-surface-700"
                >
                  <i
                    class="pi pi-image text-surface-400 dark:text-surface-600 text-xl opacity-50"
                  ></i>
                  <span
                    class="text-[9px] font-bold text-surface-400 dark:text-surface-600 uppercase tracking-widest opacity-70 leading-none mt-1"
                  >
                    No Img
                  </span>
                </div>
              }
            </td>
            <td>
              <span class="font-medium">{{ product.sku }}</span>
            </td>
            <td>{{ product.name }}</td>
            <td>{{ product.brandName }}</td>
            <td class="text-center">
              <p-button
                icon="pi pi-images"
                label="Manage Images"
                [text]="true"
                severity="info"
                (onClick)="openImageModal(product)"
                pTooltip="Manage Product Images"
                tooltipPosition="bottom"
              ></p-button>
            </td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage">
          <tr>
            <td colspan="5" class="text-center p-8">
              <div class="flex flex-col items-center gap-2 text-gray-400">
                <i class="pi pi-images text-4xl opacity-20"></i>
                <span>No products found.</span>
              </div>
            </td>
          </tr>
        </ng-template>
      </p-table>

      <!-- Resim Yönetimi Modalı -->
      <p-dialog
        [(visible)]="modalVisible"
        [header]="'Manage Images: ' + (selectedProduct?.name || '')"
        [modal]="true"
        [style]="{ width: '800px' }"
        [closable]="true"
        (onHide)="closeModal()"
      >
        <div class="flex flex-col gap-6 pt-2">
          <!-- Resim Yükleme Alanı -->
          <div class="p-4 border border-dashed surface-border rounded-lg surface-section">
            <h3 class="font-medium mb-3 text-color">Add New Image</h3>
            <p-fileUpload
              mode="advanced"
              chooseLabel="Select Image"
              uploadLabel="Upload"
              cancelLabel="Cancel"
              [customUpload]="true"
              (uploadHandler)="onUpload($event)"
              accept="image/*"
              [maxFileSize]="5000000"
              [auto]="true"
              styleClass="w-full"
            >
              <ng-template pTemplate="content">
                @if (uploading()) {
                  <div class="p-4 text-center">
                    <i class="pi pi-spin pi-spinner text-2xl text-blue-500"></i>
                    <span class="ml-2">Uploading...</span>
                  </div>
                }
              </ng-template>
            </p-fileUpload>
          </div>

          <!-- Mevcut Resimler Galerisi -->
          <div>
            <h3 class="font-medium mb-3 text-color">Current Images</h3>

            @if (loadingImages()) {
              <div class="flex justify-center p-8">
                <i class="pi pi-spin pi-spinner text-3xl text-blue-500"></i>
              </div>
            } @else if (productImages().length === 0) {
              <div class="text-center p-8 surface-section rounded text-color-secondary">
                <i class="pi pi-image text-3xl mb-2"></i>
                <p>No images available for this product.</p>
              </div>
            } @else {
              <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
                @for (img of productImages(); track img) {
                  <div
                    class="relative group border surface-border rounded-lg overflow-hidden shadow-sm hover:shadow-md transition-shadow"
                  >
                    <p-image
                      [src]="resolveImageUrl(img)"
                      [preview]="true"
                      alt="Product Image"
                      width="100%"
                      styleClass="w-full h-32 object-cover block"
                    ></p-image>

                    <button
                      pButton
                      icon="pi pi-trash"
                      class="p-button-danger p-button-rounded p-button-sm absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity shadow-lg"
                      (click)="deleteImage(img)"
                      pTooltip="Delete Image"
                    ></button>
                  </div>
                }
              </div>
            }
          </div>
        </div>

        <ng-template pTemplate="footer">
          <p-button label="Close" severity="secondary" (onClick)="closeModal()"></p-button>
        </ng-template>
      </p-dialog>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        width: 100%;
      }
      ::ng-deep .p-fileupload-content {
        padding: 1rem !important;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductVisualManagement {
  products = signal<(ProductDto & { imageFallback?: number })[]>([]);
  totalRecords = signal(0);
  loading = signal(false);

  // Modal State
  modalVisible = false;
  selectedProduct: ProductDto | null = null;
  productImages = signal<string[]>([]);
  loadingImages = signal(false);
  uploading = signal(false);

  private productService = inject(ProductService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private imageService = inject(ImageService);

  // Pagination & Filter
  private currentPage = 1;
  private pageSize = 10;
  private currentSearch = '';

  loadProducts(event: any) {
    this.currentPage = Math.floor((event.first ?? 0) / (event.rows ?? 10)) + 1;
    this.pageSize = event.rows || 10;
    this.fetchProducts();
  }

  fetchProducts() {
    this.loading.set(true);
    const filter: ProductFilter = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      searchTerm: this.currentSearch,
    };

    this.productService
      .getVendorProducts(filter)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          // Note: Assuming getVendorProducts returns generic products list for now.
          // Adjust if a specific endpoint for all products is preferred.
          if (res && res.data) {
            this.products.set(res.data.map((p) => ({ ...p, imageFallback: 0 })));
            this.totalRecords.set(res.totalCount);
          }
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load products.',
          });
        },
      });
  }

  onSearch(event: any) {
    this.currentSearch = event.target.value;
    this.currentPage = 1;
    this.fetchProducts();
  }

  openImageModal(product: ProductDto) {
    this.selectedProduct = product;
    this.modalVisible = true;
    this.fetchProductImages(product.id);
  }

  fetchProductImages(productId: number) {
    this.loadingImages.set(true);
    this.productService
      .getProductDetail(productId)
      .pipe(finalize(() => this.loadingImages.set(false)))
      .subscribe({
        next: (data) => {
          if (data && data.images) {
            this.productImages.set(data.images);
          } else {
            this.productImages.set([]);
          }
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load product images.',
          });
        },
      });
  }

  closeModal() {
    this.modalVisible = false;
    this.selectedProduct = null;
    this.productImages.set([]);
  }

  onUpload(event: any) {
    if (!this.selectedProduct) return;

    const file = event.files[0];
    if (!file) return;

    this.uploading.set(true);
    this.productService
      .uploadImage(this.selectedProduct.id, file)
      .pipe(
        finalize(() => {
          this.uploading.set(false);
          event.files = []; // Clear uploaded files
        }),
      )
      .subscribe({
        next: (res) => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Image uploaded successfully.',
          });
          if (this.selectedProduct) {
            this.fetchProductImages(this.selectedProduct.id);
            // Also refresh the main table to show thumbnail update if needed
            this.fetchProducts();
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to upload image.',
          });
        },
      });
  }

  deleteImage(imageUrl: string) {
    if (!this.selectedProduct) return;

    this.confirmationService.confirm({
      message: 'Are you sure you want to delete this image?',
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.productService.deleteImage(this.selectedProduct!.id, imageUrl).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Image deleted successfully.',
            });
            if (this.selectedProduct) {
              this.fetchProductImages(this.selectedProduct.id);
              this.fetchProducts();
            }
          },
          error: () => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to delete image.',
            });
          },
        });
      },
    });
  }

  getImageUrl(product: any): string {
    return this.imageService.getImageUrl({
      sku: product.sku,
      imageUrl: product.imageUrl,
      imageFallback: product.imageFallback,
    });
  }

  onImageError(product: any) {
    if (typeof product.imageFallback === 'undefined') {
      product.imageFallback = 0;
    }
    product.imageFallback = (product.imageFallback || 0) + 1;
    this.products.update((current) => [...current]);
  }

  resolveImageUrl(path: string): string {
    return this.imageService.resolveImageUrl(path);
  }
}
