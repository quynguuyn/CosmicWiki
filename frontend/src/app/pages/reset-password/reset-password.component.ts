import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthApi } from '../../features/auth/api/auth.api';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
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
