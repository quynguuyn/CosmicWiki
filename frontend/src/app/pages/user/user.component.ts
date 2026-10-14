import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../features/auth/services/auth.service';

@Component({
  selector: 'app-user',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <main class="user-page">
      <section class="user-card cosmic-glass">
        <span class="material-symbols-outlined user-icon">account_circle</span>
        <p class="eyebrow">COSMICWIKI · USER PROFILE</p>
        <h1>Hồ sơ người dùng</h1>
        <p class="user-email">{{ email || 'Chưa cập nhật email' }}</p>
        <div class="user-actions">
          <a routerLink="/orders" class="btn-primary">Xem đơn hàng</a>
          <button type="button" class="btn-secondary" (click)="logout()">Đăng xuất</button>
        </div>
      </section>
    </main>
  `,
  styleUrl: './user.component.scss'
})
export class UserComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly email = this.auth.getEmail();

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
