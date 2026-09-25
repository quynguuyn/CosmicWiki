import { Routes } from '@angular/router';

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
  { path: '**', redirectTo: '' }
];
