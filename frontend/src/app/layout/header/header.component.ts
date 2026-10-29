import { Component, HostListener, signal, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../features/auth/services/auth.service';
import { NotificationService } from '../../features/notifications/notification.service';
import { UserNotification } from '../../features/notifications/notification.model';

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
  readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);
  cartCount = computed(() => this.cart.count);
  isAuthenticated = computed(() => this.auth.authenticated());
  userMenuOpen = signal(false);
  notificationsOpen = signal(false);
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

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  openUserMenu(): void {
    this.userMenuOpen.set(true);
  }

  closeUserMenuOnFocusExit(event: FocusEvent): void {
    const wrapper = event.currentTarget;
    if (!(wrapper instanceof HTMLElement) ||
        !(event.relatedTarget instanceof Node) ||
        !wrapper.contains(event.relatedTarget)) {
      this.userMenuOpen.set(false);
    }
  }

  @HostListener('document:click', ['$event'])
  closeUserMenuOnOutsideClick(event: MouseEvent): void {
    if (!(event.target instanceof Element) ||
        !event.target.closest('.user-menu-wrapper')) {
      this.userMenuOpen.set(false);
    }
    if (!(event.target instanceof Element) ||
        !event.target.closest('.notification-wrapper')) {
      this.notificationsOpen.set(false);
    }
  }

  toggleNotifications(): void {
    const opening = !this.notificationsOpen();
    this.notificationsOpen.set(opening);
    if (opening) this.notifications.refresh();
  }

  openNotification(notification: UserNotification): void {
    this.notifications.markRead(notification);
    this.notificationsOpen.set(false);
    this.router.navigate(['/orders']);
  }

  logout(): void {
    this.auth.logout();
    this.userMenuOpen.set(false);
    this.router.navigate(['/login']);
  }

  search(): void {
    const query = this.searchTerm.trim();
    if (query) {
      this.router.navigate(['/library'], { queryParams: { search: query } });
    }
  }

  openObservatory(): void {
    this.router.navigate(['/visit']);
  }
}
