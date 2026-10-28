import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DeliveryOrder } from '../../features/ordering/models/order.model';
import { OrderingApi } from '../../features/ordering/api/ordering.api';

@Component({
  selector: 'app-delivery',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './delivery.component.html',
  styleUrl: './delivery.component.scss'
})
export class DeliveryComponent implements OnInit, OnDestroy {
  private readonly api = inject(OrderingApi);
  orders: DeliveryOrder[] = [];
  loading = true;
  busy = new Set<string>();
  error = '';
  notice = '';
  sharingLocation = false;
  private watchId: number | null = null;
  private lastLocationSentAt = 0;

  get hasActiveDeliveries(): boolean {
    return this.orders.some(order => !order.availableToClaim && order.status === 'Shipping');
  }

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.stopLocationSharing();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.api.getDeliveryOrders().subscribe({
      next: orders => {
        this.orders = orders;
        this.loading = false;
      },
      error: () => {
        this.error = 'Không thể tải danh sách đơn giao hàng.';
        this.loading = false;
      }
    });
  }

  claim(order: DeliveryOrder): void {
    if (this.busy.has(order.orderId)) return;
    this.runForOrder(order.orderId, this.api.claimDeliveryOrder(order.orderId), 'Đã nhận đơn giao hàng.');
  }

  complete(order: DeliveryOrder): void {
    if (this.busy.has(order.orderId)) return;
    this.runForOrder(order.orderId, this.api.markDeliveryComplete(order.orderId), 'Đơn hàng đã được xác nhận giao thành công.');
  }

  toggleLocationSharing(): void {
    if (this.sharingLocation) {
      this.stopLocationSharing();
      this.notice = 'Đã dừng chia sẻ vị trí.';
      return;
    }
    if (!navigator.geolocation) {
      this.error = 'Trình duyệt không hỗ trợ định vị.';
      return;
    }

    this.error = '';
    this.notice = '';
    this.watchId = navigator.geolocation.watchPosition(
      position => this.sendLocation(position.coords.latitude, position.coords.longitude),
      () => {
        this.error = 'Không thể lấy vị trí. Hãy cấp quyền định vị và thử lại.';
        this.stopLocationSharing();
      },
      { enableHighAccuracy: true, maximumAge: 5000, timeout: 15000 }
    );
    this.sharingLocation = true;
    this.notice = 'Đang chia sẻ vị trí cho các đơn bạn đang giao.';
  }

  private sendLocation(latitude: number, longitude: number): void {
    const now = Date.now();
    if (now - this.lastLocationSentAt < 15000) return;
    this.lastLocationSentAt = now;

    for (const order of this.orders.filter(item => !item.availableToClaim && item.status === 'Shipping')) {
      this.api.updateDeliveryLocation(order.orderId, latitude, longitude).subscribe({
        next: updated => this.replaceOrder(updated),
        error: () => this.error = `Không thể cập nhật vị trí đơn #${order.orderId.slice(0, 8)}.`
      });
    }
  }

  private runForOrder(
    orderId: string,
    request: ReturnType<OrderingApi['claimDeliveryOrder']>,
    successMessage: string
  ): void {
    this.busy.add(orderId);
    this.error = '';
    request.subscribe({
      next: updated => {
        this.replaceOrder(updated);
        this.notice = successMessage;
      },
      error: () => {
        this.busy.delete(orderId);
        this.load();
        this.error = 'Không thể cập nhật đơn hàng. Danh sách đã được làm mới.';
      },
      complete: () => this.busy.delete(orderId)
    });
  }

  private replaceOrder(updated: DeliveryOrder): void {
    this.orders = [
      ...this.orders.filter(order => order.orderId !== updated.orderId),
      ...(updated.status === 'Shipping' || updated.availableToClaim ? [updated] : [])
    ].sort((left, right) => left.createdAtUtc.localeCompare(right.createdAtUtc));
  }

  private stopLocationSharing(): void {
    if (this.watchId !== null && navigator.geolocation) {
      navigator.geolocation.clearWatch(this.watchId);
    }
    this.watchId = null;
    this.sharingLocation = false;
  }
}
