import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'user',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/user/user.component').then(m => m.UserComponent)
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./pages/admin/admin-dashboard.component').then(m => m.AdminDashboardComponent)
  },
  {
    path: 'admin/orders',
    canActivate: [adminGuard],
    loadComponent: () => import('./pages/admin/admin-orders.component').then(m => m.AdminOrdersComponent)
  },
  {
    path: 'admin/inventory',
    canActivate: [adminGuard],
    loadComponent: () => import('./pages/admin/admin-inventory.component').then(m => m.AdminInventoryComponent)
  },
  {
    path: 'admin/accounts',
    canActivate: [adminGuard],
    loadComponent: () => import('./pages/admin/admin-accounts.component').then(m => m.AdminAccountsComponent)
  },
  {
    path: 'forbidden',
    loadComponent: () => import('./pages/forbidden/forbidden.component').then(m => m.ForbiddenComponent)
  },
  {
    path: 'library',
    loadComponent: () => import('./pages/library/library.component').then(m => m.LibraryComponent)
  },
  {
    path: 'store',
    loadComponent: () => import('./pages/store/store.component').then(m => m.StoreComponent)
  },
  {
    path: 'stories',
    loadComponent: () => import('./pages/stories/stories.component').then(m => m.StoriesComponent)
  },
  {
    path: 'visit',
    loadComponent: () => import('./pages/visit/visit.component').then(m => m.VisitComponent)
  },
  {
    path: 'cart',
    loadComponent: () => import('./pages/cart/cart.component').then(m => m.CartComponent)
  },
  {
    path: 'ordering',
    loadComponent: () => import('./pages/ordering/ordering.component').then(m => m.OrderingComponent)
  },
  {
    path: 'payment',
    loadComponent: () => import('./pages/payment/payment.component').then(m => m.PaymentComponent)
  },
  {
    path: 'orders',
    loadComponent: () => import('./pages/orders/orders.component').then(m => m.OrdersComponent)
  },
  {
    path: 'payment-success',
    loadComponent: () => import('./pages/payment-success/payment-success.component').then(m => m.PaymentSuccessComponent)
  },
  {
    path: '**',
    loadComponent: () => import('./pages/not-found/not-found.component').then(m => m.NotFoundComponent)
  }
];
