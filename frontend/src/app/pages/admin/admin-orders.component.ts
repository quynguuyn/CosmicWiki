import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminOrder } from '../../features/ordering/models/order.model';
import { OrderingApi } from '../../features/ordering/api/ordering.api';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <main class="admin-orders-page">
      <a routerLink="/admin" class="back-link">← Admin Dashboard</a>
      <header class="admin-orders-header">
        <div>
          <p class="eyebrow">COSMICWIKI · OPERATIONS</p>
          <h1>Đơn hàng & giao hàng</h1>
          <p>Điều phối các đơn được tạo trong ngày hôm nay.</p>
        </div>
        <button type="button" class="refresh-button" (click)="load()">Làm mới</button>
      </header>

      <nav class="admin-filters" aria-label="Lọc đơn quản trị">
        <button *ngFor="let filter of filters" type="button"
                [class.active]="activeFilter === filter.value"
                (click)="activeFilter = filter.value">{{ filter.label }}</button>
      </nav>

      <p class="feedback" *ngIf="error">{{ error }}</p>
      <p class="loading" *ngIf="loading">Đang tải đơn hàng...</p>
      <section class="admin-order-list" *ngIf="!loading">
        <article class="admin-order-card cosmic-glass" *ngFor="let order of filteredOrders">
          <div class="order-main">
            <div>
              <span class="order-id">{{ order.orderId | slice:0:8 }}...</span>
              <h2>{{ order.customerEmail || 'Chưa có email' }}</h2>
              <p>{{ order.createdAtUtc | date:'medium' }} · {{ order.totalAmount | currency:order.currency }}</p>
            </div>
            <span class="status" [class]="order.status.toLowerCase()">{{ label(order.status) }}</span>
          </div>
          <div class="order-actions">
            <button *ngIf="order.status === 'Paid'" type="button" class="primary"
                    (click)="update(order, 'Shipping')">Xác nhận giao ngay</button>
            <button *ngIf="order.status === 'PendingPayment' || order.status === 'Paid'"
                    type="button" class="danger" (click)="update(order, 'Cancelled')">Hủy đơn</button>
            <button *ngIf="order.status === 'Shipping'" type="button" (click)="update(order, 'Delivered')">Đã giao</button>
            <button *ngIf="order.status === 'Delivered'" type="button" (click)="update(order, 'Returned')">Đã trả</button>
          </div>
        </article>
        <p class="empty" *ngIf="filteredOrders.length === 0">Không có đơn phù hợp.</p>
      </section>
    </main>
  `,
  styleUrl: './admin-orders.component.scss'
})
export class AdminOrdersComponent implements OnInit {
  private readonly api = inject(OrderingApi);
  orders: AdminOrder[] = [];
  activeFilter = 'all';
  loading = true;
  error = '';
  readonly filters = [
    { value: 'all', label: 'Tất cả' },
    { value: 'PendingPayment', label: 'Chờ thanh toán' },
    { value: 'Paid', label: 'Chờ xác nhận' },
    { value: 'Shipping', label: 'Đang giao' },
    { value: 'Delivered', label: 'Đã giao' },
    { value: 'Cancelled', label: 'Đã hủy' }
  ];

  get filteredOrders() {
    return this.activeFilter === 'all'
      ? this.orders
      : this.orders.filter(order => order.status === this.activeFilter);
  }

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.api.getAdminOrdersToday().subscribe({
      next: orders => { this.orders = orders; this.loading = false; },
      error: () => { this.error = 'Không thể tải danh sách đơn hàng.'; this.loading = false; }
    });
  }

  update(order: AdminOrder, status: string) {
    this.api.updateAdminStatus(order.orderId, status).subscribe({
      next: updated => {
        const index = this.orders.findIndex(item => item.orderId === updated.orderId);
        if (index >= 0) this.orders[index] = updated;
        this.orders = [...this.orders];
      },
      error: () => this.error = 'Không thể cập nhật trạng thái đơn hàng.'
    });
  }

  label(status: string) {
    return this.filters.find(filter => filter.value === status)?.label
      ?? (status === 'Returned' ? 'Đã trả hàng' : status);
  }
}
