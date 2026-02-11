import { Component, ChangeDetectionStrategy, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { FullScreenModalComponent } from '../full-screen-modal/full-screen-modal.component';
import { CreateCustomerDto, CustomerDto } from '../../../core/services/customer.service';

@Component({
  selector: 'app-customer-detail-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    FullScreenModalComponent,
  ],
  template: `
    <app-full-screen-modal [visible]="visible()" (visibleChange)="visible.set($event)">
      <div header>
        <span class="font-bold text-xl">{{ isEditMode() ? 'Edit Customer' : 'New Customer' }}</span>
      </div>

      <div class="p-4" content>
        <form [formGroup]="customerForm" class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div class="field col-span-1">
            <label for="code" class="block text-sm font-medium text-gray-700 mb-1">Code</label>
            <input
              id="code"
              type="text"
              pInputText
              formControlName="code"
              class="w-full"
              placeholder="Enter customer code"
            />
          </div>

          <div class="field col-span-1">
            <label for="name" class="block text-sm font-medium text-gray-700 mb-1"
              >Company Name</label
            >
            <input
              id="name"
              type="text"
              pInputText
              formControlName="name"
              class="w-full"
              placeholder="Enter company name"
            />
          </div>

          <div class="field col-span-1">
            <label for="taxOfficeName" class="block text-sm font-medium text-gray-700 mb-1"
              >Tax Office</label
            >
            <input
              id="taxOfficeName"
              type="text"
              pInputText
              formControlName="taxOfficeName"
              class="w-full"
              placeholder="Enter tax office"
            />
          </div>

          <div class="field col-span-1">
            <label for="taxNumber" class="block text-sm font-medium text-gray-700 mb-1"
              >Tax Number</label
            >
            <input
              id="taxNumber"
              type="text"
              pInputText
              formControlName="taxNumber"
              class="w-full"
              placeholder="Enter tax number"
            />
          </div>

          <div class="field col-span-1">
            <label for="phone" class="block text-sm font-medium text-gray-700 mb-1">Phone</label>
            <input
              id="phone"
              type="text"
              pInputText
              formControlName="phone"
              class="w-full"
              placeholder="Enter phone number"
            />
          </div>

          <div class="field col-span-1">
            <label for="email" class="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input
              id="email"
              type="email"
              pInputText
              formControlName="email"
              class="w-full"
              placeholder="Enter email address"
            />
          </div>

          <div class="field col-span-1">
            <label for="city" class="block text-sm font-medium text-gray-700 mb-1">City</label>
            <input
              id="city"
              type="text"
              pInputText
              formControlName="city"
              class="w-full"
              placeholder="Enter city"
            />
          </div>

          <div class="field col-span-1 md:col-span-2">
            <label for="addressLine1" class="block text-sm font-medium text-gray-700 mb-1"
              >Address</label
            >
            <input
              id="addressLine1"
              type="text"
              pInputText
              formControlName="addressLine1"
              class="w-full"
              placeholder="Enter address"
            />
          </div>

          <div class="field col-span-1">
            <label for="status" class="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <p-select
              id="status"
              [options]="statusOptions"
              formControlName="status"
              styleClass="w-full"
              [style]="{ width: '100%' }"
            ></p-select>
          </div>

          <div class="field col-span-1">
            <label for="accountType" class="block text-sm font-medium text-gray-700 mb-1"
              >Account Type</label
            >
            <p-select
              id="accountType"
              [options]="accountTypeOptions"
              formControlName="isVendor"
              styleClass="w-full"
              [style]="{ width: '100%' }"
            ></p-select>
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
          [disabled]="customerForm.invalid"
        ></p-button>
      </div>
    </app-full-screen-modal>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerDetailModalComponent {
  visible = signal(false);
  saving = signal(false);
  isEditMode = signal(false);

  // Use a signal to hold the initial data for the form, or just set the form value.
  // We can use an effect to reset form when visible changes, but simpler to expose open method.

  private fb = inject(FormBuilder);

  customerForm = this.fb.group({
    id: [0],
    code: ['', Validators.required],
    name: ['', Validators.required],
    taxOfficeName: [''],
    taxNumber: [''],
    phone: [''],
    email: ['', [Validators.email]],
    city: [''],
    addressLine1: [''],
    status: ['Active', Validators.required],
    isVendor: [false, Validators.required],
  });

  statusOptions = [
    { label: 'Active', value: 'Active' },
    { label: 'Inactive', value: 'Inactive' },
  ];

  accountTypeOptions = [
    { label: 'Customer', value: false },
    { label: 'Vendor', value: true },
  ];

  private _onSaveCallback?: (data: CreateCustomerDto) => void;

  open(customer?: CustomerDto, onSave?: (data: CreateCustomerDto) => void) {
    this._onSaveCallback = onSave;
    this.isEditMode.set(!!customer);

    if (customer) {
      this.customerForm.patchValue({
        id: customer.id,
        code: customer.code,
        name: customer.name,
        taxOfficeName: customer.taxOfficeName,
        taxNumber: customer.taxNumber,
        phone: customer.phone,
        email: customer.email,
        city: customer.city,
        addressLine1: customer.addressLine1,
        status: customer.status,
        isVendor: customer.isVendor,
      });
    } else {
      this.customerForm.reset({
        id: 0,
        code: '',
        name: '',
        status: 'Active',
        isVendor: false,
      });
    }

    this.visible.set(true);
  }

  onSave() {
    if (this.customerForm.valid && this._onSaveCallback) {
      const formValue = this.customerForm.value;
      const dto: CreateCustomerDto = {
        code: formValue.code || '',
        name: formValue.name || '',
        taxOfficeName: formValue.taxOfficeName || undefined,
        taxNumber: formValue.taxNumber || undefined,
        addressLine1: formValue.addressLine1 || undefined,
        city: formValue.city || undefined,
        phone: formValue.phone || undefined,
        email: formValue.email || undefined,
        status: formValue.status || 'Active',
        isVendor: formValue.isVendor ?? false,
      };
      this._onSaveCallback(dto);
    }
  }

  close() {
    this.visible.set(false);
  }
}
