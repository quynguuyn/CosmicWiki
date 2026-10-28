import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthApi } from '../../features/auth/api/auth.api';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
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
