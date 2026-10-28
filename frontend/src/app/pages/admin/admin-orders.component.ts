import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminOrder } from '../../features/ordering/models/order.model';
import { OrderingApi } from '../../features/ordering/api/ordering.api';
import { PaginationComponent } from '../../core/components/pagination/pagination.component';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  templateUrl: './admin-orders.component.html',
  styleUrl: './admin-orders.component.scss'
})
export class AdminOrdersComponent implements OnInit {
  private readonly api = inject(OrderingApi);
  orders: AdminOrder[] = [];
  page = 1;
  readonly pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  activeFilter = 'all';
  loading = true;
  error = '';
  readonly filters = [
    { value: 'all', label: 'Tất cả' },
    { value: 'PendingPayment', label: 'Chờ thanh toán' },
    { value: 'Paid', label: 'Chờ nhân viên nhận giao' },
    { value: 'Shipping', label: 'Đang giao' },
    { value: 'Delivered', label: 'Đã giao' },
    { value: 'Cancelled', label: 'Đã hủy' }
  ];

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.error = '';
    this.api.getAdminOrdersToday(
      this.page,
      this.pageSize,
      this.activeFilter === 'all' ? undefined : this.activeFilter
    ).subscribe({
      next: result => {
        this.orders = result.items;
        this.page = result.page;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.loading = false;
      },
      error: () => { this.error = 'Không thể tải danh sách đơn hàng.'; this.loading = false; }
    });
  }

  setFilter(filter: string) {
    this.activeFilter = filter;
    this.page = 1;
    this.load();
  }

  goToPage(page: number) {
    if (page < 1 || page > this.totalPages || page === this.page) return;
    this.page = page;
    this.load();
  }

  update(order: AdminOrder, status: string) {
    this.api.updateAdminStatus(order.orderId, status).subscribe({
      next: updated => {
        const index = this.orders.findIndex(item => item.orderId === updated.orderId);
        if (index >= 0 && (this.activeFilter === 'all' || updated.status === this.activeFilter)) {
          this.orders[index] = updated;
          this.orders = [...this.orders];
        } else {
          this.load();
        }
      },
      error: () => this.error = 'Không thể cập nhật trạng thái đơn hàng.'
    });
  }

  label(status: string) {
    return this.filters.find(filter => filter.value === status)?.label
      ?? (status === 'Returned' ? 'Đã trả hàng' : status);
  }
}
