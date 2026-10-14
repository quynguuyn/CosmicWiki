import { Component, HostListener, signal, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../features/auth/services/auth.service';

interface NavLink {
  label: string;
  route: string;
  icon?: string;
}

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule, FormsModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  private readonly cart = inject(CartService);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  cartCount = computed(() => this.cart.count);
  isAuthenticated = computed(() => this.auth.authenticated());
  userMenuOpen = signal(false);
  mobileMenuOpen = signal(false);
  scrolled = signal(false);
  searchTerm = '';

  navLinks: NavLink[] = [
    { label: 'Hành trình Vũ trụ', route: '/' },
    { label: 'Thư viện Thiên hà', route: '/library' },
    { label: 'Cửa hàng Lưu niệm', route: '/store' },
    { label: 'Biên niên sử', route: '/stories' },
    { label: 'Đài thiên văn', route: '/visit' },
  ];

  @HostListener('window:scroll')
  onScroll() {
    this.scrolled.set(window.scrollY > 20);
  }

  toggleMobileMenu() {
    this.mobileMenuOpen.update(v => !v);
  }

  toggleUserMenu(): void {
    this.userMenuOpen.update(value => !value);
  }

  logout(): void {
    this.auth.logout();
    this.userMenuOpen.set(false);
    this.router.navigate(['/']);
  }

  search(): void {
    const query = this.searchTerm.trim();
    if (query) {
      this.router.navigate(['/store'], { queryParams: { search: query } });
    }
  }
}
