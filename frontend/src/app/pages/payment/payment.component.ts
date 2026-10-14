import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, DestroyRef, inject, OnInit } from '@angular/core';
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
  private readonly route = inject(ActivatedRoute);
  private readonly orders = inject(OrderingService);
  private readonly cart = inject(CartService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);
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
    if (this.order) {
      this.loading = false;
      this.cdr.detectChanges(); // Ensure *ngIf="order && !loading" renders the container
    }
    if (navigationOrder?.clientSecret) await this.mountStripeElement(navigationOrder.clientSecret);
    this.refreshOrder(orderId);
    timer(3000, 5000).pipe(
      switchMap(() => this.orders.getOrder(orderId)),
      takeWhile(order => !['Paid', 'Shipping', 'PaymentFailed', 'Cancelled'].includes(order.status), true),
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
      confirmParams: { return_url: `${window.location.origin}/payment-success` },
      redirect: 'if_required'
    });
    this.paymentSubmitting = false;
    if (result.error) {
      this.paymentError = result.error.message ?? 'Không thể xác nhận thanh toán.';
    } else {
      this.cart.clear();
      window.location.href = `${window.location.origin}/payment-success`;
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
      this.cdr.detectChanges();
      return;
    }
    this.stripe = await loadStripe(API_CONFIG.stripePublishableKey);
    if (!this.stripe) {
      this.paymentError = 'Không thể khởi tạo Stripe.';
      this.cdr.detectChanges();
      return;
    }
    this.elements = this.stripe.elements({ clientSecret });
    this.paymentControl = this.elements.create('payment');
    // Listen to ready event — fires when Stripe iframe is fully interactive
    this.paymentControl.on('ready', () => {
      this.paymentReady = true;
      this.cdr.detectChanges(); // Stripe callbacks run outside Angular zone
    });
    // Element is always in DOM (section uses [hidden], not *ngIf)
    const el = document.getElementById('stripe-payment-element');
    if (!el) {
      this.paymentError = 'Không tìm thấy container cho Stripe element.';
      this.cdr.detectChanges();
      return;
    }
    this.paymentControl.mount(el);
    this.paymentFormAvailable = true;
    this.cdr.detectChanges();
  }
}
