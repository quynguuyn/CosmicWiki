import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { DiscoveryService } from '../../features/discovery/services/discovery.service';
import { ProductService } from '../../features/product/services/product.service';
import { Product } from '../../features/product/models/product.model';
import { CartService } from '../../core/services/cart.service';
import { NewsletterService } from '../../features/newsletter/services/newsletter.service';

interface Chapter {
  id: number;
  emoji: string;
  label: string;
  chapterNum: string;
  chapterSub: string;
  title: string;
  description: string;
  meta: string;
  imageUrl: string;
  imageAlt: string;
  wide?: boolean;
  tags?: string[];
  date?: string;
  icons?: string[];
}

interface ArchiveItem {
  code: string;
  category: string;
  distance: string;
  title: string;
  description: string;
  classification: string;
  imageUrl: string;
  imageAlt: string;
}

interface StoreItem {
  productId?: string;
  badge: string;
  origin: string;
  title: string;
  description: string;
  price: string;
  imageUrl: string;
  imageAlt: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  private readonly discovery = inject(DiscoveryService);
  private readonly productsApi = inject(ProductService);
  private readonly cart = inject(CartService);
  private readonly newsletter = inject(NewsletterService);
  newsletterSubmitted = false;
  private products: Product[] = [];

  subscribeNewsletter(email: string): void {
    if (!email.trim()) return;
    this.newsletter.subscribe(email).subscribe({
      next: () => this.newsletterSubmitted = true
    });
  }
  chapters: Chapter[] = [];

  archiveItems: ArchiveItem[] = [];

  storeItems: StoreItem[] = [];

  cartItems: { name: string; price: string }[] = [];

  constructor() {
    this.loadDatabaseContent();
  }

  addToCart(name: string, price: string) {
    const product = this.products.find(item => item.name === name);
    if (product) {
      this.cart.add(product);
    }
    this.cartItems = this.cart.lines().map(line => ({
      name: line.product.name,
      price: this.formatCurrency(line.product.salePrice)
    }));
  }

  private loadDatabaseContent(): void {
    this.discovery.search(undefined, undefined, 1, 20).subscribe({
      next: result => {
        if (result.items.length === 0) {
          return;
        }

        this.archiveItems = result.items.slice(0, 3).map(item => ({
          code: item.sourceType,
          category: item.sourceType,
          distance: 'CosmicWiki Archive',
          title: item.name,
          description: item.description ?? 'Chưa có mô tả.',
          classification: item.sourceType,
          imageUrl: item.imageUrl ?? 'assets/images/cosmic-placeholder.svg',
          imageAlt: item.name
        }));

        this.chapters = result.items.slice(0, 5).map((item, index) => ({
          id: index + 1,
          emoji: '✦',
          label: item.sourceType,
          chapterNum: `Chương ${String(index + 1).padStart(2, '0')}`,
          chapterSub: 'Tư liệu lưu trữ',
          title: item.name,
          description: item.description ?? 'Tư liệu thiên văn được lập chỉ mục.',
          meta: item.sourceId,
          imageUrl: item.imageUrl ?? 'assets/images/cosmic-placeholder.svg',
          imageAlt: item.name,
          wide: index === 4
        }));
      },
      error: error => console.error('Home discovery error:', error)
    });

    this.productsApi.getProducts(1, 3).subscribe({
      next: result => {
        this.products = result.items;
        this.storeItems = result.items.map(product => ({
          productId: product.id,
          badge: product.status,
          origin: product.sku,
          title: product.name,
          description: product.description ?? 'Chưa có mô tả.',
          price: this.formatCurrency(product.salePrice),
          imageUrl: product.images[0]?.url ?? 'assets/images/cosmic-placeholder.svg',
          imageAlt: product.name
        }));
      },
      error: error => console.error('Home product error:', error)
    });
  }

  private formatCurrency(value: number): string {
    return `${new Intl.NumberFormat('vi-VN').format(value)} ₫`;
  }
}
