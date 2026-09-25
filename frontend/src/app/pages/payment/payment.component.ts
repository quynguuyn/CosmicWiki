import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderingService } from '../../features/ordering/services/ordering.service';
import { CreateOrderResult, OrderStatus } from '../../features/ordering/models/order.model';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.scss'
})
export class PaymentComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly orders = inject(OrderingService);
  private readonly cart = inject(CartService);
  order: CreateOrderResult | OrderStatus | null = null;
  error = '';
  loading = true;

  ngOnInit(): void {
    const navigationOrder = history.state?.order as CreateOrderResult | undefined;
    const orderId = navigationOrder?.orderId ?? this.route.snapshot.queryParamMap.get('orderId');
    if (!orderId) { this.loading = false; this.error = 'Không tìm thấy đơn hàng.'; return; }
    this.orders.getOrder(orderId).subscribe({
      next: order => { this.order = order; this.loading = false; this.cart.clear(); },
      error: () => { this.loading = false; this.error = 'Không thể đọc trạng thái đơn hàng.'; }
    });
  }
}
