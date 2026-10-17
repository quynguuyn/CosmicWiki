import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../features/auth/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <main class="register-page">
      <section class="register-card cosmic-glass">
        <a routerLink="/" class="register-brand">CosmicWiki</a>
        <p class="eyebrow">COSMIC ACCESS</p>
        <h1>Tạo tài khoản</h1>
        <p class="register-intro">Đăng ký để tiếp tục hành trình khám phá vũ trụ.</p>
        <form *ngIf="!registered" (ngSubmit)="submit()">
          <label>Tên tài khoản
            <input name="username" [(ngModel)]="username" autocomplete="username" required minlength="3">
          </label>
          <label>Email
            <input name="email" [(ngModel)]="email" type="email" autocomplete="email" required>
          </label>
          <label>Mật khẩu
            <input name="password" [(ngModel)]="password" type="password"
              autocomplete="new-password" required minlength="8">
          </label>
          <p class="register-error" *ngIf="errorMessage" role="alert">{{ errorMessage }}</p>
          <button type="submit" [disabled]="isSubmitting">
            {{ isSubmitting ? 'Đang tạo tài khoản...' : 'Tạo tài khoản' }}
          </button>
        </form>
        <p class="register-success" *ngIf="registered" role="status">
          Tài khoản đã được tạo. Bạn có thể đăng nhập ngay.
        </p>
        <a routerLink="/login" class="register-login">{{ registered ? 'Đăng nhập' : 'Đã có tài khoản? Đăng nhập' }}</a>
      </section>
    </main>
  `,
  styles: [`
    :host { display: block; }
    .register-page { min-height: 100vh; display: grid; place-items: center; padding: 2rem; background: radial-gradient(circle at 20% 20%, rgba(114,160,193,.12), transparent 35%), #040913; color: var(--on-surface); }
    .register-card { display: grid; gap: 1rem; width: min(100%, 30rem); padding: clamp(1.5rem, 5vw, 3rem); border: 1px solid var(--glass-border); border-radius: var(--radius-lg); }
    .register-brand { color: var(--primary); font-weight: 700; letter-spacing: .12em; text-decoration: none; text-transform: uppercase; }
    .eyebrow { margin: 0; color: var(--tertiary); letter-spacing: .15em; }
    h1, p { margin: 0; }
    .register-intro { color: var(--on-surface-variant); }
    form, label { display: grid; gap: .55rem; }
    form { gap: 1rem; margin-top: .5rem; }
    input { width: 100%; padding: .75rem; border: 1px solid var(--glass-border); border-radius: .5rem; background: rgba(5,14,28,.7); color: var(--on-surface); font: inherit; }
    button { border: 0; border-radius: .5rem; padding: .8rem 1rem; background: var(--primary); color: var(--on-primary); cursor: pointer; font: inherit; font-weight: 700; }
    button:disabled { cursor: wait; opacity: .65; }
    .register-error { color: #ffab9e; }
    .register-success { color: #9de5b9; }
    .register-login { color: var(--tertiary); text-align: center; }
  `]
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);

  username = '';
  email = '';
  password = '';
  isSubmitting = false;
  registered = false;
  errorMessage = '';

  submit(): void {
    if (this.isSubmitting) return;

    this.isSubmitting = true;
    this.errorMessage = '';
    this.auth.register({
      username: this.username.trim(),
      email: this.email.trim(),
      password: this.password
    }).subscribe({
      next: () => {
        this.registered = true;
        this.password = '';
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = error.status === 409
          ? 'Tên tài khoản hoặc email đã được sử dụng.'
          : 'Không thể tạo tài khoản. Vui lòng kiểm tra thông tin và thử lại.';
        this.isSubmitting = false;
      },
      complete: () => {
        this.isSubmitting = false;
      }
    });
  }
}
