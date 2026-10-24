import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DeliveryOrder } from '../../features/ordering/models/order.model';
import { OrderingApi } from '../../features/ordering/api/ordering.api';

@Component({
  selector: 'app-delivery',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <main class="delivery-page">
      <a routerLink="/" class="back-link">← CosmicWiki</a>
      <header>
        <p class="eyebrow">COSMICWIKI · DELIVERY</p>
        <h1>Đơn giao hàng</h1>
        <p>Nhận đơn, chia sẻ vị trí khi đang giao và cập nhật trạng thái cho khách hàng.</p>
      </header>
      <div class="toolbar">
        <button type="button" (click)="toggleLocationSharing()" [disabled]="!hasActiveDeliveries && !sharingLocation">
          {{ sharingLocation ? 'Dừng chia sẻ vị trí' : 'Bắt đầu chia sẻ vị trí' }}
        </button>
        <button type="button" class="secondary" (click)="load()">Làm mới</button>
      </div>
      <p class="notice" *ngIf="notice" role="status">{{ notice }}</p>
      <p class="error" *ngIf="error" role="alert">{{ error }}</p>
      <p class="loading" *ngIf="loading">Đang tải đơn hàng...</p>
      <section class="delivery-list" *ngIf="!loading">
        <article class="delivery-card cosmic-glass" *ngFor="let order of orders">
          <div class="card-heading">
            <div>
              <p class="order-code">Đơn #{{ order.orderId | slice:0:8 }}</p>
              <p>{{ order.createdAtUtc | date:'medium' }}</p>
            </div>
            <span class="status" [class.available]="order.availableToClaim">
              {{ order.availableToClaim ? 'Chờ nhận giao' : 'Đang giao' }}
            </span>
          </div>
          <p class="amount">{{ order.totalAmount | currency:order.currency }}</p>
          <p class="position" *ngIf="order.deliveryPersonName">Phụ trách: {{ order.deliveryPersonName }}</p>
          <p class="position" *ngIf="order.deliveryLatitude !== null && order.deliveryLongitude !== null">
            Vị trí gần nhất: {{ order.deliveryLatitude | number:'1.4-4' }},
            {{ order.deliveryLongitude | number:'1.4-4' }}
            <span *ngIf="order.deliveryLocationUpdatedAtUtc">
              · {{ order.deliveryLocationUpdatedAtUtc | date:'shortTime' }}
            </span>
          </p>
          <div class="actions">
            <button *ngIf="order.availableToClaim" type="button" (click)="claim(order)">
              Nhận giao đơn này
            </button>
            <button *ngIf="!order.availableToClaim && order.status === 'Shipping'"
              type="button" class="complete" (click)="complete(order)">
              Xác nhận đã giao
            </button>
          </div>
        </article>
        <p class="empty" *ngIf="orders.length === 0">Hiện không có đơn chờ giao hoặc được giao cho bạn.</p>
      </section>
    </main>
  `,
  styles: [`
    :host { display:block; min-height:100vh; background:#040913; color:var(--on-surface); }
    .delivery-page { width:min(100% - 2rem, 70rem); margin:auto; padding:3rem 0; }
    .back-link { color:var(--tertiary); text-decoration:none; }
    header { margin:2rem 0; }
    .eyebrow { color:var(--tertiary); letter-spacing:.15em; }
    h1 { margin:.35rem 0; font-size:clamp(2rem,5vw,3rem); }
    header p:last-child,.card-heading p,.position { color:var(--on-surface-variant); }
    .toolbar,.actions { display:flex; flex-wrap:wrap; gap:.75rem; }
    button { border:0; border-radius:.55rem; padding:.7rem 1rem; background:var(--primary); color:var(--on-primary); cursor:pointer; font:inherit; font-weight:700; }
    button.secondary { border:1px solid var(--glass-border); background:transparent; color:var(--on-surface); }
    button.complete { background:#247a58; color:white; }
    button:disabled { cursor:not-allowed; opacity:.5; }
    .notice { color:#9de5b9; } .error { color:#ffab9e; }
    .delivery-list { display:grid; gap:1rem; margin-top:1.5rem; }
    .delivery-card { padding:1.3rem; border:1px solid var(--glass-border); border-radius:var(--radius-lg); }
    .card-heading { display:flex; align-items:flex-start; justify-content:space-between; gap:1rem; }
    .card-heading p { margin:.25rem 0; }
    .order-code { color:var(--on-surface); font-weight:700; }
    .status { padding:.35rem .65rem; border-radius:999px; background:rgba(114,160,193,.14); color:var(--tertiary); white-space:nowrap; }
    .status.available { background:rgba(201,168,106,.16); color:#e4c487; }
    .amount { font-size:1.15rem; font-weight:700; }
    .position { font-size:.9rem; }
    .empty,.loading { padding:2rem; text-align:center; color:var(--on-surface-variant); }
  `]
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
