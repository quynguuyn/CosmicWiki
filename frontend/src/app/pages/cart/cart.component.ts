import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss'
})
export class CartComponent {
  readonly cart = inject(CartService);

  get allItemsSelected(): boolean {
    return this.cart.areAllSelected();
  }

  get someItemsSelected(): boolean {
    return this.cart.selectedProductIds().length > 0 && !this.allItemsSelected;
  }

  toggleSelectAll(event: Event): void {
    this.cart.selectAll((event.target as HTMLInputElement).checked);
  }

  toggleItemSelection(productId: string, event: Event): void {
    this.cart.setSelected(productId, (event.target as HTMLInputElement).checked);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }
}
