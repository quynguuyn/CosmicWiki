import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderingService } from '../../features/ordering/services/ordering.service';
import { OrderStatus } from '../../features/ordering/models/order.model';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="orders-container">
      <div class="orders-header">
        <h1>Lịch sử đơn hàng</h1>
        <p>Xem lại các hiện vật vũ trụ bạn đã sưu tầm</p>
      </div>

      <nav class="order-filters" aria-label="Lọc đơn hàng">
        <button *ngFor="let filter of filters"
                type="button"
                class="filter-pill"
                [class.active]="activeFilter === filter.value"
                (click)="setFilter(filter.value)">
          {{ filter.label }}
        </button>
      </nav>

      <div class="orders-list">
        <div class="loading" *ngIf="loading">
          <div class="spinner"></div>
          <p>Đang tải dữ liệu...</p>
        </div>

        <div class="no-orders" *ngIf="!loading && filteredOrders.length === 0">
          <p>Bạn chưa có đơn hàng nào.</p>
          <a routerLink="/store" class="btn btn-primary">Đến cửa hàng</a>
        </div>

        <div class="order-card" *ngFor="let order of filteredOrders" [ngClass]="order.status.toLowerCase()">
          <div class="order-header">
            <span class="order-date" *ngIf="order.createdAtUtc">{{ order.createdAtUtc | date:'short' }}</span>
          </div>
          <div class="order-body">
            <div class="order-total">{{ order.totalAmount | currency:order.currency }}</div>
            <div class="order-status">
              <span class="badge" [ngClass]="order.status.toLowerCase()">
                {{ getStatusLabel(order.status) }}
              </span>
            </div>
          </div>
          <div class="order-footer" *ngIf="order.status === 'PendingPayment' && order.clientSecret">
            <span class="payment-countdown">
              {{ remainingSeconds(order) > 0 ? 'Tự hủy sau ' + remaining(order) : 'Đang hủy đơn...' }}
            </span>
            <a *ngIf="remainingSeconds(order) > 0" [routerLink]="['/payment']"
               [state]="{ order: { orderId: order.orderId, totalAmount: order.totalAmount, currency: order.currency, paymentIntentId: order.paymentIntentId, clientSecret: order.clientSecret } }"
               class="btn btn-outline">Thanh toán ngay</a>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./orders.component.scss']
})
export class OrdersComponent implements OnDestroy, OnInit {
  private readonly orderingService = inject(OrderingService);
  orders: OrderStatus[] = [];
  filteredOrders: OrderStatus[] = [];
  activeFilter = 'all';
  loading = true;
  now = Date.now();
  private timer?: ReturnType<typeof setInterval>;
  private refreshTimer?: ReturnType<typeof setInterval>;

  readonly filters = [
    { value: 'all', label: 'Tất cả' },
    { value: 'PendingPayment', label: 'Chờ thanh toán' },
    { value: 'Shipping', label: 'Đang giao hàng' },
    { value: 'Delivered', label: 'Đã giao hàng' },
    { value: 'Returned', label: 'Trả hàng' },
    { value: 'Cancelled', label: 'Đã hủy' }
  ];

  ngOnInit(): void {
    this.timer = setInterval(() => this.now = Date.now(), 1000);
    this.refreshTimer = setInterval(() => {
      if (this.orders.some(order => order.status === 'PendingPayment' && this.remainingSeconds(order) === 0)) {
        this.loadOrders();
      }
    }, 10000);
    this.loadOrders();
  }

  private loadOrders(): void {
    this.orderingService.getOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.applyFilter();
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  ngOnDestroy(): void {
    if (this.timer) clearInterval(this.timer);
    if (this.refreshTimer) clearInterval(this.refreshTimer);
  }

  remaining(order: OrderStatus): string {
    const seconds = this.remainingSeconds(order);
    return `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, '0')}`;
  }

  remainingSeconds(order: OrderStatus): number {
    const expires = order.paymentExpiresAtUtc
      ? new Date(order.paymentExpiresAtUtc).getTime()
      : new Date(order.createdAtUtc).getTime() + 15 * 60 * 1000;
    return Math.max(0, Math.floor((expires - this.now) / 1000));
  }

  setFilter(filter: string): void {
    this.activeFilter = filter;
    this.applyFilter();
  }

  private applyFilter(): void {
    this.filteredOrders = this.activeFilter === 'all'
      ? this.orders
      : this.activeFilter === 'Shipping'
          ? this.orders.filter(order => ['Paid', 'Shipping'].includes(order.status))
          : this.orders.filter(order => order.status === this.activeFilter);
  }

  getStatusLabel(status: string): string {
    return this.filters.find(filter => filter.value === status)?.label
      ?? (status === 'Paid' ? 'Đang giao hàng' : undefined)
      ?? (status === 'PaymentFailed' ? 'Thanh toán thất bại' : status);
  }
}
