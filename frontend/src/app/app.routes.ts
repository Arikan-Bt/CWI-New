import { Routes } from '@angular/router';
import { AppLayout } from './layout/component/app.layout';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    component: AppLayout,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'pages/settings/users',
        loadComponent: () =>
          import('./pages/settings/user-management').then((m) => m.UserManagement),
      },
      {
        path: 'pages/settings/roles',
        loadComponent: () =>
          import('./pages/settings/role-management').then((m) => m.RoleManagement),
      },
      {
        path: 'pages/settings/customers',
        loadComponent: () =>
          import('./pages/settings/customer-management').then((m) => m.CustomerManagement),
      },
      {
        path: 'pages/settings/warehouses',
        loadComponent: () =>
          import('./pages/settings/warehouse-management').then((m) => m.WarehouseManagement),
      },
      {
        path: 'pages',
        loadChildren: () => import('./pages/pages.routes').then((m) => m.default),
      },
    ],
  },
  {
    path: 'auth',
    loadChildren: () => import('./pages/auth/auth.routes'),
  },
  { path: '**', redirectTo: '/auth/error' },
];
