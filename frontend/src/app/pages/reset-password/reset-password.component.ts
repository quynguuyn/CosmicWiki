import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthApi } from '../../features/auth/api/auth.api';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <main class="recovery-page">
      <section class="recovery-card cosmic-glass">
        <a routerLink="/" class="brand">CosmicWiki</a>
        <p class="eyebrow">ACCOUNT RECOVERY</p>
        <h1>Đặt lại mật khẩu</h1>
        <form *ngIf="!completed" (ngSubmit)="submit()">
          <label for="password">Mật khẩu mới</label>
          <input id="password" name="password" type="password" [(ngModel)]="password"
            autocomplete="new-password" minlength="8" required>
          <label for="confirm">Nhập lại mật khẩu</label>
          <input id="confirm" name="confirm" type="password" [(ngModel)]="confirmation"
            autocomplete="new-password" minlength="8" required>
          <p class="error" *ngIf="error" role="alert">{{ error }}</p>
          <button type="submit" [disabled]="busy || !token">{{ busy ? 'Đang cập nhật...' : 'Cập nhật mật khẩu' }}</button>
        </form>
        <p class="success" *ngIf="completed" role="status">Mật khẩu đã được cập nhật. Hãy đăng nhập lại.</p>
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
    .back-link { color:var(--tertiary); }
  `]
})
export class ResetPasswordComponent {
  private readonly api = inject(AuthApi);
  private readonly route = inject(ActivatedRoute);
  token = this.route.snapshot.queryParamMap.get('token') ?? '';
  password = '';
  confirmation = '';
  busy = false;
  completed = false;
  error = '';

  submit(): void {
    if (this.busy || !this.token) return;
    if (this.password.length < 8) {
      this.error = 'Mật khẩu phải có ít nhất 8 ký tự.';
      return;
    }
    if (this.password !== this.confirmation) {
      this.error = 'Mật khẩu nhập lại không khớp.';
      return;
    }

    this.busy = true;
    this.error = '';
    this.api.resetPassword(this.token, this.password).subscribe({
      next: () => {
        this.completed = true;
        this.password = '';
        this.confirmation = '';
      },
      error: () => {
        this.error = 'Liên kết đặt lại không hợp lệ hoặc đã hết hạn.';
        this.busy = false;
      },
      complete: () => this.busy = false
    });
  }
}
