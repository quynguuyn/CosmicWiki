import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthApi } from '../../features/auth/api/auth.api';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <main class="recovery-page">
      <section class="recovery-card cosmic-glass">
        <a routerLink="/" class="brand">CosmicWiki</a>
        <p class="eyebrow">ACCOUNT RECOVERY</p>
        <h1>Quên mật khẩu?</h1>
        <p>Nhập email tài khoản để nhận liên kết đặt lại mật khẩu.</p>
        <form *ngIf="!submitted" (ngSubmit)="submit()">
          <label for="email">Email</label>
          <input id="email" name="email" type="email" [(ngModel)]="email"
            autocomplete="email" required>
          <p class="error" *ngIf="error" role="alert">{{ error }}</p>
          <button type="submit" [disabled]="busy">{{ busy ? 'Đang gửi...' : 'Gửi liên kết' }}</button>
        </form>
        <p class="success" *ngIf="submitted" role="status">
          Nếu email thuộc một tài khoản, hướng dẫn đặt lại mật khẩu sẽ được gửi đến hộp thư.
        </p>
        <p class="error" *ngIf="submitted && !emailDeliveryEnabled && !developmentResetUrl">
          Chưa cấu hình gửi email. Hãy cấu hình SMTP hoặc bật liên kết thử nghiệm chỉ cho Development.
        </p>
        <a class="dev-link" *ngIf="developmentResetUrl" [href]="developmentResetUrl">
          Mở liên kết đặt lại (chỉ môi trường Development)
        </a>
        <a routerLink="/login" class="back-link">Quay lại đăng nhập</a>
      </section>
    </main>
  `,
  styles: [`
    :host { display:block; }
    .recovery-page { min-height:100vh; display:grid; place-items:center; padding:2rem; background:#040913; color:var(--on-surface); }
    .recovery-card { display:grid; gap:1rem; width:min(100%,30rem); padding:clamp(1.5rem,5vw,3rem); border:1px solid var(--glass-border); border-radius:var(--radius-lg); }
    .brand { color:var(--primary); font-weight:700; letter-spacing:.12em; text-decoration:none; text-transform:uppercase; }
    .eyebrow { margin:0; color:var(--tertiary); letter-spacing:.15em; }
    h1,p { margin:0; }
    form,label { display:grid; gap:.6rem; }
    form { gap:1rem; margin-top:.5rem; }
    input { padding:.75rem; border:1px solid var(--glass-border); border-radius:.5rem; background:rgba(5,14,28,.7); color:var(--on-surface); font:inherit; }
    button { border:0; border-radius:.5rem; padding:.8rem 1rem; background:var(--primary); color:var(--on-primary); cursor:pointer; font:inherit; font-weight:700; }
    button:disabled { opacity:.65; }
    .error { color:#ffab9e; } .success { color:#9de5b9; }
    .dev-link,.back-link { color:var(--tertiary); }
  `]
})
export class ForgotPasswordComponent {
  private readonly api = inject(AuthApi);
  email = '';
  busy = false;
  submitted = false;
  error = '';
  developmentResetUrl: string | null = null;
  emailDeliveryEnabled = false;

  submit(): void {
    if (this.busy || !this.email.trim()) return;
    this.busy = true;
    this.error = '';
    this.api.requestPasswordReset(this.email.trim()).subscribe({
      next: response => {
        this.submitted = true;
        this.developmentResetUrl = response.developmentResetUrl ?? null;
        this.emailDeliveryEnabled = response.emailDeliveryEnabled;
      },
      error: () => {
        this.error = 'Không thể gửi yêu cầu. Vui lòng thử lại sau.';
        this.busy = false;
      },
      complete: () => this.busy = false
    });
  }
}
