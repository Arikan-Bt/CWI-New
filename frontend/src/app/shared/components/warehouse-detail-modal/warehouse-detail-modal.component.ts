import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { FullScreenModalComponent } from '../full-screen-modal/full-screen-modal.component';
import {
  CreateWarehouseDto,
  UpdateWarehouseDto,
  WarehouseDto,
} from '../../../core/services/warehouse.service';

@Component({
  selector: 'app-warehouse-detail-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    CheckboxModule,
    FullScreenModalComponent,
  ],
  template: `
    <app-full-screen-modal [visible]="visible()" (visibleChange)="visible.set($event)">
      <div header>
        <span class="font-bold text-xl">{{
          isEditMode() ? 'Edit Warehouse' : 'New Warehouse'
        }}</span>
      </div>

      <div class="p-4" content>
        <form [formGroup]="warehouseForm" class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Code -->
          <div class="field col-span-1">
            <label for="code" class="block text-sm font-medium text-gray-700 mb-1">
              Warehouse Code <span class="text-red-500">*</span>
            </label>
            <input
              id="code"
              type="text"
              pInputText
              formControlName="code"
              class="w-full"
              placeholder="Enter warehouse code"
            />
            @if (warehouseForm.get('code')?.invalid && warehouseForm.get('code')?.touched) {
              <small class="text-red-500">Warehouse code is required</small>
            }
          </div>

          <!-- Name -->
          <div class="field col-span-1">
            <label for="name" class="block text-sm font-medium text-gray-700 mb-1">
              Warehouse Name <span class="text-red-500">*</span>
            </label>
            <input
              id="name"
              type="text"
              pInputText
              formControlName="name"
              class="w-full"
              placeholder="Enter warehouse name"
            />
            @if (warehouseForm.get('name')?.invalid && warehouseForm.get('name')?.touched) {
              <small class="text-red-500">Warehouse name is required</small>
            }
          </div>

          <!-- Address -->
          <div class="field col-span-1 md:col-span-2">
            <label for="address" class="block text-sm font-medium text-gray-700 mb-1">
              Address
            </label>
            <input
              id="address"
              type="text"
              pInputText
              formControlName="address"
              class="w-full"
              placeholder="Enter warehouse address"
            />
          </div>

          <!-- IsActive (sadece edit modda) -->
          @if (isEditMode()) {
            <div class="field col-span-1">
              <div class="flex items-center gap-2">
                <p-checkbox inputId="isActive" formControlName="isActive" [binary]="true" />
                <label for="isActive" class="text-sm font-medium text-gray-700"> Active </label>
              </div>
            </div>
          }

          <!-- IsDefault -->
          <div class="field col-span-1">
            <div class="flex items-center gap-2">
              <p-checkbox inputId="isDefault" formControlName="isDefault" [binary]="true" />
              <label for="isDefault" class="text-sm font-medium text-gray-700">
                Set as Default Warehouse
              </label>
            </div>
            <small class="text-gray-500">
              Default warehouse will be automatically selected in transactions
            </small>
          </div>
        </form>
      </div>

      <div class="flex justify-end gap-3 p-4 border-t" footer>
        <p-button
          label="Cancel"
          icon="pi pi-times"
          styleClass="p-button-text"
          (onClick)="visible.set(false)"
        ></p-button>
        <p-button
          label="Save"
          icon="pi pi-check"
          [loading]="saving()"
          (onClick)="onSave()"
          [disabled]="warehouseForm.invalid"
        ></p-button>
      </div>
    </app-full-screen-modal>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WarehouseDetailModalComponent {
  visible = signal(false);
  saving = signal(false);
  isEditMode = signal(false);

  private fb = inject(FormBuilder);

  warehouseForm = this.fb.group({
    id: [0],
    code: ['', Validators.required],
    name: ['', Validators.required],
    address: [''],
    isActive: [true],
    isDefault: [false],
  });

  private _onSaveCallback?: (data: CreateWarehouseDto | UpdateWarehouseDto) => void;

  /**
   * Modal'ı aç
   * @param warehouse Edit için warehouse objesi (Create için undefined)
   * @param onSave Kaydet callback fonksiyonu
   */
  open(warehouse?: WarehouseDto, onSave?: (data: CreateWarehouseDto | UpdateWarehouseDto) => void) {
    this._onSaveCallback = onSave;
    this.isEditMode.set(!!warehouse);

    if (warehouse) {
      // Edit mode
      this.warehouseForm.patchValue({
        id: warehouse.id,
        code: warehouse.code,
        name: warehouse.name,
        address: warehouse.address || '',
        isActive: warehouse.isActive,
        isDefault: warehouse.isDefault,
      });
    } else {
      // Create mode
      this.warehouseForm.reset({
        id: 0,
        code: '',
        name: '',
        address: '',
        isActive: true,
        isDefault: false,
      });
    }

    this.visible.set(true);
  }

  /**
   * Kaydet butonuna tıklandığında
   */
  onSave() {
    if (this.warehouseForm.valid && this._onSaveCallback) {
      const formValue = this.warehouseForm.value;

      if (this.isEditMode()) {
        // Update
        const dto: UpdateWarehouseDto = {
          id: formValue.id || 0,
          code: formValue.code || '',
          name: formValue.name || '',
          address: formValue.address || undefined,
          isActive: formValue.isActive || true,
          isDefault: formValue.isDefault || false,
        };
        this._onSaveCallback(dto);
      } else {
        // Create
        const dto: CreateWarehouseDto = {
          code: formValue.code || '',
          name: formValue.name || '',
          address: formValue.address || undefined,
          isDefault: formValue.isDefault || false,
        };
        this._onSaveCallback(dto);
      }
    }
  }

  /**
   * Modal'ı kapat
   */
  close() {
    this.visible.set(false);
  }
}
