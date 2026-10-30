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
  private readonly selectionStorageKey = 'cosmicwiki_cart_selection';
  readonly lines = signal<CartLine[]>(this.read());
  readonly selectedProductIds = signal<string[]>(this.readSelectedProductIds());

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
              const hadLocalCart = this.lines().length > 0;
              this.lines.set(serverCart);
              localStorage.setItem(this.storageKey, JSON.stringify(serverCart));
              const availableIds = new Set(serverCart.map(line => line.product.id));
              const selectedIds = hadLocalCart
                ? this.selectedProductIds().filter(id => availableIds.has(id))
                : serverCart.map(line => line.product.id);
              this.saveSelection(selectedIds);
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

  get selectedCount(): number {
    return this.selectedLines().reduce((total, line) => total + line.quantity, 0);
  }

  get selectedTotal(): number {
    return this.selectedLines().reduce(
      (total, line) => total + line.product.salePrice * line.quantity,
      0
    );
  }

  selectedLines(): CartLine[] {
    const selectedIds = new Set(this.selectedProductIds());
    return this.lines().filter(line => selectedIds.has(line.product.id));
  }

  isSelected(productId: string): boolean {
    return this.selectedProductIds().includes(productId);
  }

  areAllSelected(): boolean {
    return this.lines().length > 0 &&
      this.lines().every(line => this.isSelected(line.product.id));
  }

  setSelected(productId: string, selected: boolean): void {
    const selectedIds = new Set(this.selectedProductIds());
    if (selected) {
      selectedIds.add(productId);
    } else {
      selectedIds.delete(productId);
    }
    this.saveSelection([...selectedIds]);
  }

  selectAll(selected: boolean): void {
    this.saveSelection(selected ? this.lines().map(line => line.product.id) : []);
  }

  add(product: Product): void {
    const lines = [...this.lines()];
    const existing = lines.find(line => line.product.id === product.id);
    if (existing) {
      if (existing.quantity < product.stockQuantity) {
        const index = lines.indexOf(existing);
        lines[index] = { ...existing, quantity: existing.quantity + 1 };
      }
    } else {
      if (product.stockQuantity > 0) {
        lines.push({ product, quantity: 1 });
      }
    }
    const selectedIds = new Set(this.selectedProductIds());
    if (product.stockQuantity > 0) {
      selectedIds.add(product.id);
    }
    this.save(lines, [...selectedIds]);
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
    this.save(
      this.lines().filter(line => line.product.id !== productId),
      this.selectedProductIds().filter(id => id !== productId)
    );
  }

  removeProducts(productIds: string[]): void {
    const idsToRemove = new Set(productIds);
    this.save(
      this.lines().filter(line => !idsToRemove.has(line.product.id)),
      this.selectedProductIds().filter(id => !idsToRemove.has(id))
    );
  }

  removeSelected(): void {
    this.removeProducts(this.selectedProductIds());
  }

  clear(): void {
    this.save([]);
  }

  private save(lines: CartLine[], selectedIds = this.selectedProductIds()): void {
    this.lines.set(lines);
    localStorage.setItem(this.storageKey, JSON.stringify(lines));
    const availableIds = new Set(lines.map(line => line.product.id));
    this.saveSelection(selectedIds.filter(id => availableIds.has(id)));
    this.pushToServer(lines);
  }

  private saveSelection(productIds: string[]): void {
    const uniqueIds = [...new Set(productIds)];
    this.selectedProductIds.set(uniqueIds);
    localStorage.setItem(this.selectionStorageKey, JSON.stringify(uniqueIds));
  }

  private read(): CartLine[] {
    try {
      const value = JSON.parse(localStorage.getItem(this.storageKey) ?? '[]');
      return Array.isArray(value) ? value : [];
    } catch {
      return [];
    }
  }

  private readSelectedProductIds(): string[] {
    try {
      const saved = localStorage.getItem(this.selectionStorageKey);
      if (saved === null) {
        return this.lines().map(line => line.product.id);
      }

      const value: unknown = JSON.parse(saved);
      if (!Array.isArray(value)) {
        return [];
      }

      const availableIds = new Set(this.lines().map(line => line.product.id));
      return [...new Set(value.filter((id): id is string =>
        typeof id === 'string' && availableIds.has(id)))];
    } catch {
      return this.lines().map(line => line.product.id);
    }
  }
}
