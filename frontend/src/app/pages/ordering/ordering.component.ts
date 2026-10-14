import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../features/auth/services/auth.service';
import { OrderingService } from '../../features/ordering/services/ordering.service';

@Component({
  selector: 'app-ordering',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './ordering.component.html',
  styleUrl: './ordering.component.scss'
})
export class OrderingComponent {
  readonly cart = inject(CartService);
  private readonly auth = inject(AuthService);
  private readonly orders = inject(OrderingService);
  private readonly router = inject(Router);
  isSubmitting = false;
  error = '';

  submit(): void {
    const customerId = this.auth.getCustomerId();
    if (!customerId || this.cart.lines().length === 0 || this.isSubmitting) {
      this.error = 'Vui lòng đăng nhập và chọn ít nhất một hiện vật.';
      return;
    }
    this.isSubmitting = true;
    this.error = '';
    this.orders.createOrder({
      customerId,
      currency: 'USD',
      lines: this.cart.lines().map(line => ({ productId: line.product.id, quantity: line.quantity }))
    }).subscribe({
      next: order => {
        this.cart.clear();
        this.router.navigate(['/payment'], { state: { order } });
      },
      error: () => { this.error = 'Không thể tạo đơn hàng. Vui lòng kiểm tra tồn kho và thử lại.'; this.isSubmitting = false; }
    });
  }
}
