import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ApiService } from './api.service';
import { CartService } from './cart.service';
import { AuthSessionService } from '../../features/auth/services/auth-session.service';
import { Product } from '../../features/product/models/product.model';

describe('CartService selection', () => {
  let cart: CartService;

  const product = (id: string, salePrice: number): Product => ({
    id,
    sku: `SKU-${id}`,
    name: `Product ${id}`,
    slug: `product-${id}`,
    description: null,
    originalPrice: salePrice,
    salePrice,
    stockQuantity: 5,
    status: 'Active',
    categoryId: 'category',
    images: []
  });

  beforeEach(() => {
    localStorage.removeItem('cosmicwiki_cart');
    localStorage.removeItem('cosmicwiki_cart_selection');
    TestBed.configureTestingModule({
      providers: [
        CartService,
        { provide: ApiService, useValue: { get: () => of([]), post: () => of({}) } },
        { provide: AuthSessionService, useValue: { isAuthenticated: () => false } }
      ]
    });
    cart = TestBed.inject(CartService);
  });

  afterEach(() => {
    localStorage.removeItem('cosmicwiki_cart');
    localStorage.removeItem('cosmicwiki_cart_selection');
    TestBed.resetTestingModule();
  });

  it('uses the selected lines for totals and removes only selected products', () => {
    cart.add(product('a', 10));
    cart.add(product('b', 20));
    cart.add(product('b', 20));

    cart.setSelected('b', false);

    expect(cart.selectedLines().map(line => line.product.id)).toEqual(['a']);
    expect(cart.selectedCount).toBe(1);
    expect(cart.selectedTotal).toBe(10);

    cart.removeSelected();

    expect(cart.lines().map(line => line.product.id)).toEqual(['b']);
    expect(cart.selectedCount).toBe(0);
    expect(cart.count).toBe(2);
  });

  it('selects and deselects every line with select all', () => {
    cart.add(product('a', 10));
    cart.add(product('b', 20));

    cart.selectAll(false);
    expect(cart.selectedLines()).toEqual([]);
    expect(cart.areAllSelected()).toBeFalse();

    cart.selectAll(true);
    expect(cart.areAllSelected()).toBeTrue();
    expect(cart.selectedTotal).toBe(30);
  });
});
