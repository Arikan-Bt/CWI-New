import { Component, inject, signal } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StyleClassModule } from 'primeng/styleclass';
import { Popover } from 'primeng/popover';
import { DatePicker } from 'primeng/datepicker';
import { FormsModule } from '@angular/forms';
import { LayoutService } from '../service/layout.service';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { UserDetailModalComponent } from '../../shared/components/user-detail-modal/user-detail-modal.component';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { finalize } from 'rxjs';
import { CartService } from '../../core/services/cart.service';
import { DrawerModule } from 'primeng/drawer';
import { environment } from '../../../environments/environment';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [
    RouterModule,
    CommonModule,
    StyleClassModule,
    Popover,
    DatePicker,
    FormsModule,
    UserDetailModalComponent,
    DrawerModule,
    BadgeModule,
    ButtonModule,
  ],

  template: ` <div class="layout-topbar">
      <div class="layout-topbar-logo-container">
        <button
          class="layout-menu-button layout-topbar-action"
          (click)="layoutService.onMenuToggle()"
        >
          <i class="pi pi-bars"></i>
        </button>
        <a class="layout-topbar-logo" routerLink="/">
          <img src="/assets/images/Logo.png" alt="logo" style="height: 2.5rem;" />
        </a>
      </div>

      <div class="layout-topbar-actions">
        <div class="layout-config-menu">
          <button type="button" class="layout-topbar-action" (click)="toggleDarkMode()">
            <i
              [ngClass]="{
                'pi ': true,
                'pi-moon': layoutService.isDarkTheme(),
                'pi-sun': !layoutService.isDarkTheme(),
              }"
            ></i>
          </button>
        </div>

        <button
          class="layout-topbar-menu-button layout-topbar-action"
          pStyleClass="@next"
          enterFromClass="hidden"
          enterActiveClass="animate-scalein"
          leaveToClass="hidden"
          leaveActiveClass="animate-fadeout"
          [hideOnOutsideClick]="true"
        >
          <i class="pi pi-ellipsis-v"></i>
        </button>

        <div class="layout-topbar-menu hidden lg:block">
          <div class="layout-topbar-menu-content">
            <button type="button" class="layout-topbar-action" (click)="calendarOp.toggle($event)">
              <i class="pi pi-calendar"></i>
              <span>Calendar</span>
            </button>

            @if (isVendor()) {
              <button type="button" class="layout-topbar-action" (click)="cartVisible.set(true)">
                <div class="relative inline-flex items-center justify-center">
                  <i class="pi pi-shopping-cart text-2xl"></i>
                  @if (cartService.totalItems() > 0) {
                    <span
                      class="absolute -top-2 -right-2 flex items-center justify-center bg-red-600 text-white text-[10px] font-bold rounded-full w-5 h-5 border-2 border-white dark:border-surface-900 leading-none shadow-md z-10"
                    >
                      {{ cartService.totalItems() }}
                    </span>
                  }
                </div>
                <span class="hidden">Cart</span>
              </button>
            }
            <button type="button" class="layout-topbar-action" (click)="showProfile()">
              <i class="pi pi-user"></i>
              <span>Profile</span>
            </button>
            <button type="button" class="layout-topbar-action" (click)="onLogout()">
              <i class="pi pi-power-off"></i>
              <span>Logout</span>
            </button>
          </div>
        </div>

        <p-popover #calendarOp>
          <p-datepicker [(ngModel)]="currentDate" [inline]="true"></p-datepicker>
        </p-popover>
      </div>
    </div>
    <app-user-detail-modal
      [(visible)]="isProfileVisible"
      [initialUser]="currentUser()"
      [isEditMode]="true"
      [isSelfProfile]="true"
      [saving]="saving()"
      (save)="onSaveProfile($event)"
    ></app-user-detail-modal>

    <p-drawer
      [(visible)]="cartVisible"
      position="right"
      styleClass="!w-full md:!w-[30%]"
      [blockScroll]="true"
    >
      <ng-template pTemplate="header">
        <div class="flex items-center gap-2 font-bold text-xl">
          <i class="pi pi-shopping-cart text-primary"></i>
          <span>Cart</span>
        </div>
      </ng-template>
      <div class="flex flex-col h-full">
        <div class="flex-1 overflow-y-auto flex flex-col gap-4 p-2">
          @if (cartService.cartItems().length === 0) {
            <div class="text-center text-gray-500 mt-10">
              <i class="pi pi-shopping-cart text-4xl mb-2"></i>
              <p>Your cart is empty</p>
            </div>
          } @else {
            @for (item of cartService.cartItems(); track item.product.id) {
              <div class="flex gap-4 border-b border-surface-200 dark:border-surface-700 pb-4">
                <div
                  class="w-20 h-20 bg-gray-50 rounded-lg flex items-center justify-center overflow-hidden border border-gray-100 shrink-0"
                >
                  <img
                    [src]="environment.cdnUrl + '/ProductImages/' + item.product.sku + '.jpg'"
                    class="max-w-full max-h-full object-contain"
                  />
                </div>
                <div class="flex-1 min-w-0">
                  <h4 class="font-bold text-sm truncate">{{ item.product.name }}</h4>
                  <p class="text-xs text-gray-500 mb-2">{{ item.product.sku }}</p>
                  <div class="flex items-center justify-between">
                    <span class="font-bold text-primary">{{
                      item.product.purchasePrice | currency: 'USD'
                    }}</span>
                    <div class="flex items-center gap-2">
                      <button
                        class="p-button-rounded p-button-text p-button-sm w-6 h-6 flex items-center justify-center p-0"
                        (click)="cartService.updateQuantity(item.product.id, item.quantity - 1)"
                      >
                        <i class="pi pi-minus text-xs"></i>
                      </button>
                      <span class="text-sm w-4 text-center">{{ item.quantity }}</span>
                      <button
                        class="p-button-rounded p-button-text p-button-sm w-6 h-6 flex items-center justify-center p-0"
                        (click)="cartService.updateQuantity(item.product.id, item.quantity + 1)"
                      >
                        <i class="pi pi-plus text-xs"></i>
                      </button>
                    </div>
                  </div>
                </div>
                <button
                  class="text-red-500 hover:text-red-700"
                  (click)="cartService.removeFromCart(item.product.id)"
                >
                  <i class="pi pi-trash"></i>
                </button>
              </div>
            }
          }
        </div>

        @if (cartService.cartItems().length > 0) {
          <div class="border-t border-surface-200 dark:border-surface-700 pt-4 mt-auto">
            <div class="flex justify-between items-center mb-4 text-lg font-bold">
              <span>Total</span>
              <span>{{ cartService.totalPrice() | currency: 'USD' }}</span>
            </div>
            <button pButton label="Checkout" class="w-full"></button>
          </div>
        }
      </div>
    </p-drawer>`,
})
export class AppTopbar {
  layoutService = inject(LayoutService);
  authService = inject(AuthService);
  userService = inject(UserService);
  router = inject(Router);
  messageService = inject(MessageService);
  cartService = inject(CartService);
  environment = environment;
  cartVisible = signal(false);

  currentDate: Date | undefined;
  isProfileVisible = signal(false);
  currentUser = signal<any>(null);
  saving = signal(false);

  toggleDarkMode() {
    this.layoutService.layoutConfig.update((state) => ({ ...state, darkTheme: !state.darkTheme }));
  }

  showProfile() {
    const user = this.authService.getUser();
    if (user?.id) {
      // Fetch full user details to populate the modal correctly
      this.userService.getUserById(user.id).subscribe({
        next: (data) => {
          this.currentUser.set(data);
          this.isProfileVisible.set(true);
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load profile data.',
          });
        },
      });
    }
  }

  onSaveProfile(data: any) {
    this.saving.set(true);

    // Map form data to UpdateProfileRequest
    const updateRequest = {
      firstName: data.name,
      lastName: data.surname,
      email: data.email,
    };

    this.authService
      .updateProfile(updateRequest)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Profile updated successfully.',
          });
          this.isProfileVisible.set(false);

          // Handle password change if user provided new password details
          if (data.currentPassword && data.newPassword) {
            this.changePassword({
              currentPassword: data.currentPassword,
              newPassword: data.newPassword,
              confirmNewPassword: data.confirmNewPassword || data.newPassword,
            });
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: err.error?.detail || 'Failed to update profile',
          });
        },
      });
  }

  changePassword(request: {
    currentPassword: string;
    newPassword: string;
    confirmNewPassword: string;
  }) {
    this.authService.changePassword(request).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Password updated successfully.',
        });
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Warning',
          detail:
            'Profile updated but password change failed: ' + (err.error?.detail || 'Unknown error'),
        });
      },
    });
  }

  onLogout() {
    this.authService.logout();
    this.cartService.clearCart();
    this.router.navigate(['/auth/login']);
  }

  isVendor(): boolean {
    const user = this.authService.getUser();

    if (!user) return false;

    const role = (user.roleName || '').toLowerCase();
    const type = (user.userType || '').toLowerCase();

    return role.includes('vendor') || type.includes('vendor');
  }
}
