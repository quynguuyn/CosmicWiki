import { Component, HostListener, signal, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CartService } from '../../core/services/cart.service';

interface NavLink {
  label: string;
  route: string;
  icon?: string;
}

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  private readonly cart = inject(CartService);
  cartCount = computed(() => this.cart.count);
  mobileMenuOpen = signal(false);
  scrolled = signal(false);

  navLinks: NavLink[] = [
    { label: 'Hành trình Vũ trụ', route: '/' },
    { label: 'Thư viện Thiên hà', route: '/library' },
    { label: 'Cửa hàng Lưu niệm', route: '/store' },
    { label: 'Biên niên sử', route: '/stories' },
    { label: 'Đài thiên văn', route: '/visit' },
    { label: 'Thanh toán', route: '/payment' },
  ];

  @HostListener('window:scroll')
  onScroll() {
    this.scrolled.set(window.scrollY > 20);
  }

  toggleMobileMenu() {
    this.mobileMenuOpen.update(v => !v);
  }
}
