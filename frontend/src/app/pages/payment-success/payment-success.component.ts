import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="success-container">
      <div class="success-icon">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
          <polyline points="22 4 12 14.01 9 11.01"></polyline>
        </svg>
      </div>
      
      <h1>Thanh toán thành công!</h1>
      <p>Cảm ơn bạn đã mua hàng. Đơn hàng của bạn đang được xử lý.</p>
      
      <div class="actions">
        <a routerLink="/orders" class="btn btn-primary">Xem lịch sử đơn hàng</a>
        <a routerLink="/store" class="btn btn-outline">Tiếp tục mua sắm</a>
      </div>
    </div>
  `,
  styleUrls: ['./payment-success.component.scss']
})
export class PaymentSuccessComponent implements OnInit {
  paymentIntentId: string | null = null;

  constructor(private readonly route: ActivatedRoute) {}

  ngOnInit(): void {
    this.paymentIntentId = this.route.snapshot.queryParamMap.get('payment_intent');
  }
}
