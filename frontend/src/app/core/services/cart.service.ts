import { Injectable, signal } from '@angular/core';
import { Product } from '../../features/product/models/product.model';

export interface CartLine {
  product: Product;
  quantity: number;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly storageKey = 'cosmicwiki_cart';
  readonly lines = signal<CartLine[]>(this.read());

  get count(): number {
    return this.lines().reduce((total, line) => total + line.quantity, 0);
  }

  get total(): number {
    return this.lines().reduce(
      (total, line) => total + line.product.salePrice * line.quantity,
      0
    );
  }

  add(product: Product): void {
    const lines = [...this.lines()];
    const existing = lines.find(line => line.product.id === product.id);
    if (existing) {
      existing.quantity += 1;
    } else {
      lines.push({ product, quantity: 1 });
    }
    this.save(lines);
  }

  setQuantity(productId: string, quantity: number): void {
    const lines = this.lines()
      .map(line => line.product.id === productId
        ? { ...line, quantity }
        : line)
      .filter(line => line.quantity > 0);
    this.save(lines);
  }

  remove(productId: string): void {
    this.save(this.lines().filter(line => line.product.id !== productId));
  }

  clear(): void {
    this.save([]);
  }

  private save(lines: CartLine[]): void {
    this.lines.set(lines);
    localStorage.setItem(this.storageKey, JSON.stringify(lines));
  }

  private read(): CartLine[] {
    try {
      const value = JSON.parse(localStorage.getItem(this.storageKey) ?? '[]');
      return Array.isArray(value) ? value : [];
    } catch {
      return [];
    }
  }
}
