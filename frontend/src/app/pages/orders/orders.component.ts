import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderingService } from '../../features/ordering/services/ordering.service';
import { OrderStatus } from '../../features/ordering/models/order.model';
import { RouterLink } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

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
          <section class="delivery-tracking" *ngIf="order.status === 'Shipping'">
            <h3>Thông tin giao hàng</h3>
            <p *ngIf="order.deliveryPersonName">Nhân viên giao hàng: {{ order.deliveryPersonName }}</p>
            <p *ngIf="order.deliveryLocationUpdatedAtUtc">
              Vị trí cập nhật lúc {{ order.deliveryLocationUpdatedAtUtc | date:'short' }}
            </p>
            <p *ngIf="order.deliveryLatitude == null || order.deliveryLongitude == null">
              Nhân viên giao hàng chưa chia sẻ vị trí. Bản đồ sẽ cập nhật khi có tín hiệu GPS.
            </p>
            <iframe *ngIf="mapUrl(order)" [src]="mapUrl(order)" title="Vị trí đơn hàng trên bản đồ"
              loading="lazy" referrerpolicy="no-referrer"></iframe>
          </section>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./orders.component.scss']
})
export class OrdersComponent implements OnDestroy, OnInit {
  private readonly orderingService = inject(OrderingService);
  private readonly sanitizer = inject(DomSanitizer);
  orders: OrderStatus[] = [];
  filteredOrders: OrderStatus[] = [];
  activeFilter = 'all';
  loading = true;
  now = Date.now();
  private timer?: ReturnType<typeof setInterval>;
  private refreshTimer?: ReturnType<typeof setInterval>;
  private readonly mapUrlCache = new Map<string, { coordinates: string; url: SafeResourceUrl }>();

  readonly filters = [
    { value: 'all', label: 'Tất cả' },
    { value: 'PendingPayment', label: 'Chờ thanh toán' },
    { value: 'Paid', label: 'Chờ nhân viên nhận giao' },
    { value: 'Shipping', label: 'Đang giao hàng' },
    { value: 'Delivered', label: 'Đã giao hàng' },
    { value: 'Returned', label: 'Trả hàng' },
    { value: 'Cancelled', label: 'Đã hủy' }
  ];

  ngOnInit(): void {
    this.timer = setInterval(() => this.now = Date.now(), 1000);
    this.refreshTimer = setInterval(() => {
      if (this.orders.some(order =>
        (order.status === 'PendingPayment' && this.remainingSeconds(order) === 0)
        || order.status === 'Shipping'
      )) {
        this.loadOrders();
      }
    }, 15000);
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
      : this.orders.filter(order => order.status === this.activeFilter);
  }

  getStatusLabel(status: string): string {
    return this.filters.find(filter => filter.value === status)?.label
      ?? (status === 'Paid' ? 'Chờ nhân viên nhận giao' : undefined)
      ?? (status === 'PaymentFailed' ? 'Thanh toán thất bại' : status);
  }

  mapUrl(order: OrderStatus): SafeResourceUrl | null {
    if (order.deliveryLatitude == null || order.deliveryLongitude == null) return null;
    const latitude = order.deliveryLatitude;
    const longitude = order.deliveryLongitude;
    if (!Number.isFinite(latitude) || !Number.isFinite(longitude)
      || latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180) return null;

    const coordinates = `${latitude},${longitude}`;
    const cached = this.mapUrlCache.get(order.orderId);
    if (cached?.coordinates === coordinates) return cached.url;
    const box = `${longitude - 0.01},${latitude - 0.01},${longitude + 0.01},${latitude + 0.01}`;
    const url = `https://www.openstreetmap.org/export/embed.html?bbox=${box}&layer=mapnik&marker=${latitude},${longitude}`;
    const safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
    this.mapUrlCache.set(order.orderId, { coordinates, url: safeUrl });
    return safeUrl;
  }
}
