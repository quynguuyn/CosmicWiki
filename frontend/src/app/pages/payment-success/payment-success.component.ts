import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { timer, switchMap, take, takeWhile } from 'rxjs';
import { OrderingService } from '../../features/ordering/services/ordering.service';

@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './payment-success.component.html',
  styleUrls: ['./payment-success.component.scss']
})
export class PaymentSuccessComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly orders = inject(OrderingService);
  orderId: string | null = null;
  orderStatus: string | null = null;
  redirectStatus: string | null = null;
  loading = true;
  error = '';
  paymentIntentId: string | null = null;

  constructor(private readonly route: ActivatedRoute) {}

  ngOnInit(): void {
    this.orderId = this.route.snapshot.queryParamMap.get('orderId');
    this.paymentIntentId = this.route.snapshot.queryParamMap.get('payment_intent');
    this.redirectStatus = this.route.snapshot.queryParamMap.get('redirect_status');
    const orderId = this.orderId;
    if (!orderId) {
      this.loading = false;
      return;
    }

    timer(0, 3000).pipe(
      switchMap(() => this.orders.getOrder(orderId)),
      takeWhile(order => order.status === 'PendingPayment', true),
      take(41),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: order => {
        this.orderStatus = order.status;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Không thể kiểm tra trạng thái đơn hàng. Vui lòng xem lại lịch sử đơn hàng.';
      }
    });
  }
}
