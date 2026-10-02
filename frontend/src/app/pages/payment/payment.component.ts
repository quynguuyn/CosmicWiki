import { CommonModule } from '@angular/common';
import { Component, DestroyRef, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { timer, switchMap, takeWhile } from 'rxjs';
import { loadStripe, Stripe, StripeElements, StripePaymentElement } from '@stripe/stripe-js';
import { OrderingService } from '../../features/ordering/services/ordering.service';
import { CreateOrderResult, OrderStatus } from '../../features/ordering/models/order.model';
import { CartService } from '../../core/services/cart.service';
import { API_CONFIG } from '../../core/config/api.config';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.scss'
})
export class PaymentComponent implements OnInit {
  @ViewChild('paymentElement') private paymentElement?: ElementRef<HTMLDivElement>;
  private readonly route = inject(ActivatedRoute);
  private readonly orders = inject(OrderingService);
  private readonly cart = inject(CartService);
  private readonly destroyRef = inject(DestroyRef);
  order: CreateOrderResult | OrderStatus | null = null;
  error = '';
  loading = true;
  refreshing = false;
  paymentError = '';
  paymentReady = false;
  paymentFormAvailable = false;
  paymentSubmitting = false;
  private stripe: Stripe | null = null;
  private elements: StripeElements | null = null;
  private paymentControl: StripePaymentElement | null = null;

  async ngOnInit(): Promise<void> {
    const navigationOrder = history.state?.order as CreateOrderResult | undefined;
    const orderId = navigationOrder?.orderId ?? this.route.snapshot.queryParamMap.get('orderId');
    if (!orderId) { this.loading = false; this.error = 'Không tìm thấy đơn hàng.'; return; }
    this.order = navigationOrder ?? null;
    if (navigationOrder?.clientSecret) await this.mountStripeElement(navigationOrder.clientSecret);
    this.refreshOrder(orderId);
    timer(3000, 5000).pipe(
      switchMap(() => this.orders.getOrder(orderId)),
      takeWhile(order => !['Paid', 'PaymentFailed', 'Cancelled'].includes(order.status), true),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: order => {
        this.order = order;
        this.loading = false;
        this.refreshing = false;
      },
      error: () => {
        this.loading = false;
        this.refreshing = false;
        if (!this.order) this.error = 'Không thể đọc trạng thái đơn hàng.';
      }
    });
  }

  async pay(): Promise<void> {
    if (!this.stripe || !this.elements || this.paymentSubmitting) return;
    this.paymentSubmitting = true;
    this.paymentError = '';
    const result = await this.stripe.confirmPayment({
      elements: this.elements,
      confirmParams: { return_url: `${window.location.origin}/payment?orderId=${this.order?.orderId}` },
      redirect: 'if_required'
    });
    this.paymentSubmitting = false;
    if (result.error) {
      this.paymentError = result.error.message ?? 'Không thể xác nhận thanh toán.';
    } else {
      this.cart.clear();
      this.refresh();
    }
  }

  refresh(): void {
    const orderId = this.order?.orderId;
    if (!orderId || this.refreshing) return;
    this.refreshOrder(orderId);
  }

  private refreshOrder(orderId: string): void {
    this.refreshing = true;
    this.orders.getOrder(orderId).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: order => {
        this.order = { ...this.order, ...order } as OrderStatus;
        this.loading = false;
        this.refreshing = false;
      },
      error: () => {
        this.loading = false;
        this.refreshing = false;
        if (!this.order) this.error = 'Không thể đọc trạng thái đơn hàng.';
      }
    });
  }

  private async mountStripeElement(clientSecret: string): Promise<void> {
    if (!API_CONFIG.stripePublishableKey) {
      this.paymentError = 'Frontend chưa được cấu hình Stripe publishable key.';
      return;
    }
    this.stripe = await loadStripe(API_CONFIG.stripePublishableKey);
    if (!this.stripe) {
      this.paymentError = 'Không thể khởi tạo Stripe.';
      return;
    }
    this.elements = this.stripe.elements({ clientSecret });
    this.paymentControl = this.elements.create('payment');
    this.paymentFormAvailable = true;
    setTimeout(() => {
      if (!this.paymentElement || !this.paymentControl) return;
      this.paymentControl.mount(this.paymentElement.nativeElement);
      this.paymentReady = true;
    });
  }
}
