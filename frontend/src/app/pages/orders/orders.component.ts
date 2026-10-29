import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderingService } from '../../features/ordering/services/ordering.service';
import { OrderStatus } from '../../features/ordering/models/order.model';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.scss']
})
export class OrdersComponent implements OnDestroy, OnInit {
  private readonly orderingService = inject(OrderingService);
  orders: OrderStatus[] = [];
  filteredOrders: OrderStatus[] = [];
  activeFilter = 'all';
  loading = true;
  loadError = '';
  now = Date.now();
  private timer?: ReturnType<typeof setInterval>;
  private refreshTimer?: ReturnType<typeof setInterval>;

  readonly filters = [
    { value: 'all', label: 'Tất cả' },
    { value: 'PendingPayment', label: 'Chờ thanh toán' },
    { value: 'Paid', label: 'Chờ xác nhận' },
    { value: 'Confirmed', label: 'Chờ nhân viên nhận giao' },
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
    this.loading = true;
    this.loadError = '';
    this.orderingService.getOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.applyFilter();
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.loadError = 'Không thể tải lịch sử đơn hàng. Kiểm tra kết nối rồi thử lại.';
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

  reload(): void {
    this.loadOrders();
  }

  private applyFilter(): void {
    this.filteredOrders = this.activeFilter === 'all'
      ? this.orders
      : this.orders.filter(order => order.status === this.activeFilter);
  }

  getStatusLabel(status: string): string {
    return this.filters.find(filter => filter.value === status)?.label
      ?? (status === 'Paid' ? 'Chờ xác nhận' : undefined)
      ?? (status === 'PaymentFailed' ? 'Thanh toán thất bại' : status);
  }

  mapUrl(order: OrderStatus): string | null {
    if (order.deliveryLatitude == null || order.deliveryLongitude == null) return null;
    const latitude = order.deliveryLatitude;
    const longitude = order.deliveryLongitude;
    if (!Number.isFinite(latitude) || !Number.isFinite(longitude)
      || latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180) return null;

    const coordinates = `${latitude},${longitude}`;
    return `https://www.google.com/maps/search/?api=1&query=${latitude},${longitude}`;
  }
}
