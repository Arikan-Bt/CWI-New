import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { RippleModule } from 'primeng/ripple';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../core/services/auth.service';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { CommonModule } from '@angular/common';
import { LayoutService } from '../../layout/service/layout.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    CheckboxModule,
    InputTextModule,
    PasswordModule,
    FormsModule,
    RouterModule,
    RippleModule,
    TranslateModule,
  ],
  template: `
    <div class="flex gap-4 fixed top-8 right-8">
      <p-button
        type="button"
        (onClick)="toggleDarkMode()"
        [rounded]="true"
        [icon]="isDarkTheme ? 'pi pi-moon' : 'pi pi-sun'"
        severity="secondary"
      />
    </div>
    <div
      class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-screen overflow-hidden"
    >
      <div class="flex flex-col items-center justify-center">
        <div
          style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)"
        >
          <div
            class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20"
            style="border-radius: 53px"
          >
            <div class="text-center mb-8">
              <img
                src="/assets/images/Logo.png"
                alt="Arıkan Saat Logo"
                class="mb-8 w-16 shrink-0 mx-auto"
                style="height: 4rem; width: auto;"
              />
              <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">
                {{ 'LOGIN.WELCOME' | translate }}
              </div>
              <span class="text-muted-color font-medium">{{
                'LOGIN.SIGN_IN_CONTINUE' | translate
              }}</span>
            </div>

            <div>
              <label
                for="userName1"
                class="block text-surface-900 dark:text-surface-0 text-xl font-medium mb-2"
                >{{ 'LOGIN.EMAIL' | translate }}</label
              >
              <input
                pInputText
                id="userCode1"
                type="text"
                [placeholder]="'LOGIN.EMAIL_PLACEHOLDER' | translate"
                class="w-full md:w-120 mb-8"
                [(ngModel)]="userCode"
              />

              <label
                for="password1"
                class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2"
                >{{ 'LOGIN.PASSWORD' | translate }}</label
              >
              <p-password
                id="password1"
                [(ngModel)]="password"
                [placeholder]="'LOGIN.PASSWORD_PLACEHOLDER' | translate"
                [toggleMask]="true"
                styleClass="mb-4"
                [fluid]="true"
                [feedback]="false"
                (keydown.enter)="onLogin()"
              ></p-password>

              <div class="flex items-center justify-between mt-2 mb-8 gap-8">
                <div class="flex items-center">
                  <p-checkbox
                    [(ngModel)]="rememberMe"
                    id="rememberme1"
                    binary
                    class="mr-2"
                  ></p-checkbox>
                  <label for="rememberme1">{{ 'LOGIN.REMEMBER_ME' | translate }}</label>
                </div>
                <span
                  class="font-medium no-underline ml-2 text-right cursor-pointer text-primary"
                  >{{ 'LOGIN.FORGOT_PASSWORD' | translate }}</span
                >
              </div>
              <p-button
                [label]="'LOGIN.SIGN_IN' | translate"
                styleClass="w-full"
                [loading]="loading"
                (onClick)="onLogin()"
              ></p-button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class Login {
  userCode: string = '';
  password: string = '';
  rememberMe: boolean = false;
  loading: boolean = false;

  constructor(
    private translate: TranslateService,
    private authService: AuthService,
    private messageService: MessageService,
    private router: Router,
    public layoutService: LayoutService
  ) {
    translate.addLangs(['en']);
    translate.setDefaultLang('en');
    translate.use('en');
  }

  get isDarkTheme() {
    return this.layoutService.layoutConfig().darkTheme;
  }

  toggleDarkMode() {
    this.layoutService.layoutConfig.update((state) => ({ ...state, darkTheme: !state.darkTheme }));
  }

  onLogin() {
    if (!this.userCode || !this.password) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Please enter user code and password.',
        life: 3000,
      });
      return;
    }

    this.loading = true;
    this.authService.login(this.userCode, this.password).subscribe({
      next: (result) => {
        this.loading = false;
        if (result.success) {
          this.router.navigate(['/dashboard']);
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: result.error || 'Invalid user code or password.',
            life: 3000,
          });
        }
      },
      error: (err) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'A system error occurred. Please try again later.',
          life: 3000,
        });
        console.error('Login error:', err);
      },
    });
  }
}
