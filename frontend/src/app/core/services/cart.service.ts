import { Injectable, signal, inject } from '@angular/core';
import { Product } from '../../features/product/models/product.model';
import { ApiService } from './api.service';
import { AuthSessionService } from '../../features/auth/services/auth-session.service';

export interface CartLine {
  product: Product;
  quantity: number;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly storageKey = 'cosmicwiki_cart';
  readonly lines = signal<CartLine[]>(this.read());

  private readonly apiService = inject(ApiService);
  private readonly authSession = inject(AuthSessionService);

  constructor() {
    this.syncFromServer();
  }

  private syncFromServer(): void {
    if (this.authSession.isAuthenticated()) {
      this.apiService.get<CartLine[]>('/api/carts').subscribe({
        next: (serverCart) => {
          if (serverCart && Array.isArray(serverCart)) {
            if (serverCart.length > 0) {
              this.lines.set(serverCart);
              localStorage.setItem(this.storageKey, JSON.stringify(serverCart));
            } else {
              const local = this.read();
              if (local.length > 0) {
                this.pushToServer(local);
              }
            }
          }
        },
        error: (err) => console.error('Failed to load cart', err)
      });
    }
  }

  private pushToServer(lines: CartLine[]): void {
    if (this.authSession.isAuthenticated()) {
      this.apiService.post('/api/carts', lines).subscribe({
        error: (err) => console.error('Failed to sync cart', err)
      });
    }
  }

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
      if (existing.quantity < product.stockQuantity) {
        existing.quantity += 1;
      }
    } else {
      if (product.stockQuantity > 0) {
        lines.push({ product, quantity: 1 });
      }
    }
    this.save(lines);
  }

  setQuantity(productId: string, quantity: number): void {
    const lines = this.lines()
      .map(line => {
        if (line.product.id === productId) {
          const safeQty = Math.min(Math.max(0, quantity), line.product.stockQuantity);
          return { ...line, quantity: safeQty };
        }
        return line;
      })
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
    this.pushToServer(lines);
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
